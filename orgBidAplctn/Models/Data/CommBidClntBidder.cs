using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace orgBidAplctn.Models.Data
{
    [Table("COMM_BID_CLNT_BIDDER")]
    public partial class CommBidClntBidder
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [Column("SYS_ID")]
        public long SysId { get; set; }
        [Column("BID_ID")]
        public long BidId { get; set; }
        [Required]
        [Column("PARTY_ID")]
        [StringLength(12)]
        public string PartyId { get; set; }
        [Required]
        [Column("SMS_CONT_NO")]
        [StringLength(20)]
        public string SmsContNo { get; set; }
        [Column("SMS_REC_REF")]
        public long? SmsRecRef { get; set; }
        [Column("SMS_REC_TM", TypeName = "datetime")]
        public DateTime? SmsRecTm { get; set; }
        [Column("SMS_RAW_MSG")]
        public string SmsRawMsg { get; set; }
        [Column("BID_QNTY", TypeName = "decimal(18, 3)")]
        public decimal? BidQnty { get; set; }
        [Column("BID_RATE", TypeName = "money")]
        public decimal? BidRate { get; set; }
        [Column("BID_ATTN_STAT")]
        public byte? BidAttnStat { get; set; }
        [Column("ALLOC_QNTY", TypeName = "decimal(18, 3)")]
        public decimal? AllocQnty { get; set; }
        [Column("ALLOC_RATE", TypeName = "money")]
        public decimal? AllocRate { get; set; }
        [Column("SMS_SEND_STAT")]
        public byte? SmsSendStat { get; set; }
        [Column("SMS_ALLOC_STAT")]
        public byte? SmsAllocStat { get; set; }
        [Column("SMS_SEND_TXT")]
        public string SmsSendTxt { get; set; }
        [Column("SMS_RPLY_API")]
        public string SmsRplyApi { get; set; }

        [ForeignKey(nameof(BidId))]
        [InverseProperty(nameof(CommBidMstr.CommBidClntBidder))]
        public virtual CommBidMstr Bid { get; set; }
        
        [ForeignKey(nameof(PartyId))]
        [InverseProperty(nameof(CommPartyInfo.CommBidClntBidder))]
        public virtual CommPartyInfo Party { get; set; }
    }
}
