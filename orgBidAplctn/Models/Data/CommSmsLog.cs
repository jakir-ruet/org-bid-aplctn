using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace orgBidAplctn.Models.Data
{
    [Table("COMM_SMS_LOG")]
    public partial class CommSmsLog
    {
        [Key]
        [Column("AUTO_ID")]
        public long AutoId { get; set; }
        [Column("SMS_PHONE")]
        [StringLength(30)]
        public string SmsPhone { get; set; }
        [Column("SMS_DT_TM", TypeName = "datetime")]
        public DateTime? SmsDtTm { get; set; }
        [Column("SMS_MSG")]
        public string SmsMsg { get; set; }
        [Column("SMS_FROM")]
        [StringLength(3)]
        public string SmsFrom { get; set; }
        [Column("REF_ID")]
        [StringLength(20)]
        public string RefId { get; set; }
        [Column("PART_NO")]
        [StringLength(20)]
        public string PartNo { get; set; }
        [Column("SMS_TYPE")]
        [StringLength(1)]
        public string SmsType { get; set; }
        [Column("SMS_CONTAIN")]
        [StringLength(1)]
        public string SmsContain { get; set; }
        [Column("SMS_STAT")]
        [StringLength(1)]
        public string SmsStat { get; set; }
        [Column("SMS_UPL_STAT")]
        [StringLength(1)]
        public string SmsUplStat { get; set; }
        [Column("SMS_VALIDITY")]
        [StringLength(1)]
        public string SmsValidity { get; set; }

        [NotMapped]
        public string PartyName { get; set; }
    }
}
