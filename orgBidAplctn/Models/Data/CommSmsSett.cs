using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace orgBidAplctn.Models.Data
{
    [Table("COMM_SMS_SETT")]
    public partial class CommSmsSett
    {
        [Key]
        [Column("SYS_ID")]
        public int SysId { get; set; }
        [Column("TOKEN_NO")]
        public string TokenNo { get; set; }
        [Column("SENDER_NO")]
        [StringLength(50)]
        public string SenderNo { get; set; }
        [Column("RECEIVE_NO")]
        [StringLength(50)]
        public string ReceiveNo { get; set; }
    }
}
