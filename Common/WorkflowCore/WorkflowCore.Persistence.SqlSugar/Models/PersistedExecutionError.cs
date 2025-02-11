﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace WorkflowCore.Persistence.SqlSugar.Models
{
    [Table("Wfc_ExecutionError")]
    public class PersistedExecutionError
    {
        [Key]
        public long PersistenceId { get; set; }

        [MaxLength(100)]
        public string WorkflowId { get; set; }

        [MaxLength(100)]
        public string ExecutionPointerId { get; set; }
        
        public DateTime ErrorTime { get; set; }

        public string Message { get; set; }

    }
}
