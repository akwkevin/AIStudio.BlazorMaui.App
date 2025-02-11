﻿using AIStudio.Api.Controllers;
using AIStudio.Common.Filter.FilterAttribute;
using AIStudio.Common.Swagger;
using AIStudio.Entity.Base_Manage;
using AIStudio.IBusiness.Base_Manage;
using AIStudio.Util;
using AIStudio.Util.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AIStudio.Common.Authentication.Jwt.JwtHelper;

namespace AIStudio.Api.Controllers.Base_Manage
{
    /// <summary>
    /// Base_Test
    /// </summary>

    [ApiExplorerSettings(GroupName = nameof(ApiVersionInfo.V1))]
    [Route("/Base_Manage/[controller]/[action]")]
    public class Base_TestController : ApiControllerBase
    {
        #region DI
        IBase_TestBusiness _Base_TestBus { get; }

        /// <summary>
        /// Base_Test控制器
        /// </summary>
        /// <param name="Base_TestBus"></param>
        public Base_TestController(IBase_TestBusiness Base_TestBus)
        {
            _Base_TestBus = Base_TestBus;
        }
        #endregion

        #region 获取
        /// <summary>
        /// 获取Base_Test列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<PageResult<Base_Test>> GetDataList(PageInput input)
        {
            return await _Base_TestBus.GetDataListAsync(input);
        }

        /// <summary>
        /// 根据Id获取Base_Test
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Base_Test> GetTheData(IdInputDTO input)
        {
            return await _Base_TestBus.GetTheDataAsync(input.id);
        }

        #endregion

        #region 提交

        /// <summary>
        /// 保存Base_Test
        /// </summary>
        /// <param name="theData">保存的数据</param>
        [RequestRecord]
        [HttpPost]
        [Authorize(Permissions.Auto)]
        public async Task SaveData(Base_Test theData)
        {
            await _Base_TestBus.SaveDataAsync(theData);
        }

        /// <summary>
        /// 删除Base_Test
        /// </summary>
        /// <param name="ids">id数组,JSON数组</param>
        [RequestRecord]
        [HttpPost]
        [Authorize(Permissions.Auto)]
        public async Task DeleteData(List<string> ids)
        {
            await _Base_TestBus.DeleteDataAsync(ids);
        }

        #endregion
    }
}