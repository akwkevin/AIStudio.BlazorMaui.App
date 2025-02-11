﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIStudio.Entity.Base_Manage
{
    /// <summary>
    /// 用户角色表
    /// </summary>
    [Table("Base_UserRole")]
    public class Base_UserRole
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key, Column(Order = 1)]
        [MaxLength(50)]
        public string? Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人Id
        /// </summary>
        public string? CreatorId { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [MaxLength(255)]
        public string? CreatorName { get; set; }

        /// <summary>
        /// 否已删除
        /// </summary>
        public Boolean Deleted { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// 角色Id
        /// </summary>
        public string? RoleId { get; set; }

    }
}