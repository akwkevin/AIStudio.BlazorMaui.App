﻿using AIStudio.Common.AppSettings;
using AIStudio.Common.CustomAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AIStudio.Common.Types
{
    /// <summary>
    /// 
    /// </summary>
    public static class GlobalType
    {
        static GlobalType()
        {
            if (AppSettingsConfig.AppSettingsOptions.UseWorkflow)
            {
                AssemblyPattern.Add("WorkflowCore");
            }

            string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
            AllAssemblies = Directory.GetFiles(rootPath, "*.dll")
                .Where(x => AssemblyPattern.Any(y => new FileInfo(x).Name.Contains(y)))
                .Select(x => Assembly.LoadFrom(x))
                .Where(x => !x.IsDynamic)
                .ToList();

            AllAssemblies.ForEach(aAssembly =>
            {
                try
                {
                    AllTypes.AddRange(aAssembly.GetTypes());
                }
                catch
                {

                }
            });

            PhysicDeleteTypes.AddRange(AllTypes.Where(p => p.GetCustomAttributes<PhysicDeleteTypeAttribute>().Count() > 0));
        }

        /// <summary>
        /// 解决方案程序集匹配名
        /// </summary>
        public static readonly List<string> AssemblyPattern = new List<string> { "AIStudio" };

        /// <summary>
        /// 解决方案所有程序集
        /// </summary>
        public static readonly List<Assembly> AllAssemblies;

        /// <summary>
        /// 解决方案所有自定义类
        /// </summary>
        public static readonly List<Type> AllTypes = new List<Type>();

        /// <summary>
        /// 框架物理删除的类
        /// </summary>
        public static readonly List<Type> PhysicDeleteTypes = new List<Type>();
    }
}
