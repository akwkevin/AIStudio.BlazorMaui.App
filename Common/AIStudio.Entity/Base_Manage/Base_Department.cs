﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIStudio.Entity.Base_Manage
{
    /// <summary>
    /// 部门表
    /// </summary>
    [Table("Base_Department")]
    public class Base_Department : BaseEntity
    {
        /// <summary>
        /// 部门名
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 上级部门Id
        /// </summary>
        public string? ParentId { get; set; }
    }
}