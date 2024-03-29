using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace orgBidAplctn.Models.Data
{
    [Table("COMM_NOTIFY_LIST")]
    [Index("IsRead", Name = "IX_COMM_NOTIFY_LIST")]
    public partial class CommNotifyList
    {
        [Key]
        [Column("NOT_ID")]
        public long NotId { get; set; }
        [Column("NOT_BY")]
        public long NotBy { get; set; }
        [Column("NOT_MSG")]
        [StringLength(250)]
        public string NotMsg { get; set; }
        [Column("IS_READ")]
        public byte? IsRead { get; set; }
        [Column("NOT_TP")]
        public int? NotType { get; set; }
        [Column("CREAT_TM", TypeName = "datetime")]
        public DateTime? CreatTm { get; set; }

        [ForeignKey("NotBy")]
        [InverseProperty("CommNotifyLists")]
        public virtual CommLoginInfo NotByNavigation { get; set; }
    }
}
