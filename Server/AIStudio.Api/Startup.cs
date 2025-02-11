﻿using AIStudio.Common.AppSettings;
using AIStudio.Common.Authentication.Jwt;
using AIStudio.Common.Authorization;
using AIStudio.Common.Cache;
using AIStudio.Common.DI;
using AIStudio.Common.DI.AOP;
using AIStudio.Common.EventBus.EventHandlers;
using AIStudio.Common.EventBus.Models;
using AIStudio.Common.Filter;
using AIStudio.Common.IdGenerator;
using AIStudio.Common.Mapper;
using AIStudio.Common.Quartz;
using AIStudio.Common.Service;
using AIStudio.Common.SqlSuger;
using AIStudio.Common.Swagger;
using AIStudio.Common.Types;
using AIStudio.Common.Workflow;
using AIStudio.Util;
using AIStudio.Util.Helper;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting.WindowsServices;
using NLog;
using NLog.Web;
using System.Reflection;

namespace AIStudio.Api
{
    /// <summary>
    /// 启动类，只是便于多个项目共享启动代码
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="serviceAction"></param>
        /// <param name="applicationAction"></param>
        public static void Run(string[] args, Action<IServiceCollection>? serviceAction = null, Action<WebApplication>? applicationAction = null)
        {           
            var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
            logger.Debug("启动中……");

            try
            {
                var options = new WebApplicationOptions
                {
                    Args = args,
                    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
                };
                var builder = WebApplication.CreateBuilder(options);

                //支持打包成window服务 
                //sc create AIStudio.Service BinPath = XXXXX 添加服务
                //sc stop AIStudio.Service 停止服务
                //sc delete AIStudio.Service 删除服务
                builder.Host.UseWindowsService();

                // Add services to the container.

                //读取配置文件appsettings
                AppSettingsConfig.Configure_(builder.Configuration);

                //支持https的证书
                if (AppSettingsConfig.AppSettingsOptions.UseKestrel)
                {
                    builder.WebHost.UseKestrel(options =>
                    {
                        var urls = AppSettingsConfig.urls.Split(":");
                        foreach(var url in urls)
                        {
                            if (url.StartsWith("http:"))
                            {
                                var port = url.Substring(url.LastIndexOf(":") + 1);
                                options.Listen(System.Net.IPAddress.Any, int.Parse(port));
                            }
                            else if (url.StartsWith("https:"))
                            {
                                var port = url.Substring(url.LastIndexOf(":") + 1);
                                options.Listen(System.Net.IPAddress.Any, int.Parse(port),
                                     listenOptions =>
                                     {
                                         //var x509ca = new X509Certificate2("aistudio.pfx", "aistudio");
                                         //listenOptions.UseHttps(x509ca);

                                         //var password = AesHelper.AesEncryptor("aistudio", "aistudiov1_54eeff444ferfkny6oxi8");
                                         listenOptions.UseHttps(AppSettingsConfig.CertificateOptions.Path, AesHelper.AesDecryptor(AppSettingsConfig.CertificateOptions.Password, AppSettingsConfig.CertificateOptions.Key));

                                     });
                            }
                        }
                    });
                }

                // 日志
                // NLog: Setup NLog for Dependency injection,需要先清除之前的，不然会打印两次
                builder.Logging.ClearProviders();
                builder.Host.UseNLog();

                //默认事件总线
                builder.Services.AddEventBusDefault_();
                //添加事件总线(Local)
                builder.Services.AddEventBusLocal_().AddSubscriber(subscribers =>
                {
                    subscribers.Add<TestEvent, TestEventHandler>();
                    subscribers.Add<ExceptionEvent, Business.Base_Manage.Base_LogExceptionBusiness>();
                    subscribers.Add<RequestEvent, Business.Base_Manage.Base_LogOperatingBusiness>();
                    subscribers.Add<VisitEvent, Business.Base_Manage.Base_LogVisitBusiness>();
                    subscribers.Add<SystemEvent, Business.Base_Manage.Base_LogSystemBusiness>();
                });

                ////数据过滤与Json配置
                builder.Services.AddControllers()
                    .AddDataValidation_() //数据验证
                    .AddFilter_()   //过滤器    //ApiExceptionMiddleware(异常捕获) RequestActionFilter（记录日志） AjaxResultActionFilter（结果输出）
                    .AddNewtonsoftJson(options =>
                    {
                        options.SerializerSettings.GetType().GetProperties().ForEach(aProperty =>
                        {
                            var value = aProperty.GetValue(JsonExtention.DefaultJsonSetting);
                            aProperty.SetValue(options.SerializerSettings, value);
                        });
                    });

                // 设置雪花id的workerId，确保每个实例workerId都应不同
                var workerId = (ushort)(AppSettingsConfig.SnowIdOptions.WorkerId);
                IdHelper.SetWorkId(workerId);

                // 缓存
                builder.Services.AddCache_();

                builder.Services.AddSqlSugar_();             

                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();

                //swagger
                builder.Services.AddSwaggerGen_();

                //注册 IHttpContextAccessor
                builder.Services.AddHttpContextAccessor();

                //自动注册服务
                builder.Services.AddServices_(GlobalType.AllTypes);

                //手动注入服务AOP
#if DEBUG   
                // 添加自定义的拦截类，声明周期为
                builder.Services.AddSingleton<IInterceptor, TestAOP>();
                //注入AOP
                builder.Services.AddProxiedService<Controllers.Test.IValuesService, Controllers.Test.ValuesService>(ServiceLifetime.Transient, typeof(TestAOP));
#endif

                //jwt Authentication 
                builder.Services.AddJwtAuthentication_();
                // 授权
                builder.Services.AddAuthorization_();
                // 替换默认 PermissionChecker,测试使用
                //builder.Services.Replace(new ServiceDescriptor(typeof(IPermissionChecker), typeof(TestPermissionChecker), ServiceLifetime.Transient));
                builder.Services.Replace(new ServiceDescriptor(typeof(IPermissionChecker), typeof(Business.Base_Manage.PermissionBusiness), ServiceLifetime.Transient));

                //使用AutoMapper自动映射拥有MapAttribute的类
                builder.Services.AddMapper_(GlobalType.AllTypes, GlobalType.AllAssemblies);

                // 跨域
                builder.Services.AddCors_();

                //注入MiniProfiler, 本地访问地址 http://localhost:5000/profiler/results
                builder.Services.AddMiniProfiler_();

                // 定时任务
                builder.Services.AddJobScheduling_(options =>
                {
                    options.StartHandle = async sp =>
                    {
                        bool withoutTestJob = true;
#if DEBUG
                        //初始化测试数据
                        SeedData.EnsureSeedQuartzData(sp);
                        withoutTestJob = false;
#endif
                        var jobService = sp.GetRequiredService<Business.Quartz_Manage.IQuartz_TaskBusiness>();
                        if (jobService == null) return;

                        await jobService.StartAllAsync(withoutTestJob);
                    };
                });

                // 工作流
                if (AppSettingsConfig.AppSettingsOptions.UseWorkflow)
                {
                    builder.Services.AddWorkflow_(options =>
                    {
                        options.StartHandle = async sp =>
                        {
                            SeedData.EnsureSeedWorkflow(sp);

                            var defForm = sp.GetRequiredService<Business.OA_Manage.IOA_DefFormBusiness>();
                            await defForm.LoadDefinitionAsync();

                            var userForm = sp.GetRequiredService<Business.OA_Manage.IOA_UserFormBusiness>();
                            await userForm.LoadDefinitionAsync();
                        };
                    });
                }

                //其他外部服务注册
                serviceAction?.Invoke(builder.Services);

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        //如果使用了版本控制，别忘了修改此处
                        foreach (var field in ApiVersionInfo.GetFieldValues())
                        {
                            c.SwaggerEndpoint($"/swagger/{field.Key}/swagger.json", $"{field.Key}");
                        }

                        //使用加入了MiniProfiler的html替换默认的
                        c.IndexStream = () => Assembly.GetExecutingAssembly().GetManifestResourceStream("AIStudio.Api.wwwroot.index.html");
                    });
                }

                //使用了Kestrel，此处就不能用了
                if (!AppSettingsConfig.AppSettingsOptions.UseKestrel)
                {
                    app.UseHttpsRedirection();
                }

                // UseCors 必须在 UseRouting 之后，UseResponseCaching、UseAuthorization 之前
                app.UseCors();

                //启用MiniProfiler，该方法必须在app.UseEndpoints以前
                app.UseMiniProfiler_();

                // 添加自定义中间件（包含：Body重复读取、异常处理）
                app.UseMiddleware_();

                //开启静态文件功能
                app.UseStaticFiles(new StaticFileOptions
                {
                    ServeUnknownFileTypes = true,
                    DefaultContentType = "application/octet-stream"
                });

                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();

                //其他外部App启用
                applicationAction?.Invoke(app);

                //服务提供器
                ServiceLocator.AppliactionServices = app.Services;
                ServiceLocator.RequestServices = builder.Services.BuildServiceProvider(false);
                //初始化数据
                SeedData.EnsureSeedData(ServiceLocator.RequestServices);

                app.Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "由于发生异常，导致程序中止！");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }
    }
}
