﻿using AIStudio.Util.Mapper;
using AIStudio.Entity.Base_Manage;
using AIStudio.Util;
using AIStudio.Util.Common;
using System.Collections.Generic;

namespace AIStudio.Entity.DTO.Base_Manage
{
    [Map(typeof(Base_User))]
    public class Base_UserDTO : Base_User
    {
        public string RoleNames { get => string.Join(",", RoleNameList ?? new List<string>()); }
        public IEnumerable<string> RoleIdList { get; set; }
        public IEnumerable<string> RoleNameList { get; set; }
        public RoleTypes RoleType
        {
            get
            {
                int type = 0;

                var values = typeof(RoleTypes).GetEnumValues();
                foreach (var aValue in values)
                {
                    if (RoleNames.Contains(aValue.ToString()))
                        type += (int)aValue;
                }

                return (RoleTypes)type;
            }
        }
        public string? Base_DepartmentName { get; set; }//查询的是Base_Department中的Name 从表规则【 class+字段名】
        public string? SexText { get => Sex.GetDescription(); }
        public string? BirthdayText { get => Birthday?.ToString("yyyy-MM-dd"); }
    }
}
