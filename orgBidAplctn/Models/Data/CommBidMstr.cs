using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace orgBidAplctn.Models.Data
{
    [Table("COMM_BID_MSTR")]
    public partial class CommBidMstr
    {
        public CommBidMstr()
        {
            CommBidClntBidder = new HashSet<CommBidClntBidder>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        [Column("BID_ID")]
        public long BidId { get; set; }
        [Column("BID_DATE", TypeName = "datetime")]
        public DateTime BidDate { get; set; }
        [Required]
        [Column("PROD_ID")]
        [StringLength(12)]
        public string ProdId { get; set; }
        [Column("BID_QNTY", TypeName = "decimal(18, 3)")]
        public decimal BidQnty { get; set; }
        [Column("BID_RATE", TypeName = "money")]
        public decimal BidRate { get; set; }
        [Column("ALLOC_TIME", TypeName = "datetime")]
        public DateTime? AllocTime { get; set; }
        [Column("ALLOC_QNTY", TypeName = "decimal(18, 3)")]
        public decimal? AllocQnty { get; set; }
        [Column("ALLOC_RATE", TypeName = "money")]
        public decimal? AllocRate { get; set; }
        [Column("BID_STRT_TM", TypeName = "datetime")]
        public DateTime BidStrtTm { get; set; }
        [Column("BID_END_TM", TypeName = "datetime")]
        public DateTime BidEndTm { get; set; }
        [Column("BID_STAT")]
        public byte BidStat { get; set; }
        [Column("BID_PROC_STAT")]
        public byte BidSmsProcStat { get; set; }
        [Column("BID_NOTE")]
        public string BidNote { get; set; }
        [Required]
        [Column("WARE_ID")]
        [StringLength(12)]
        public string WareId { get; set; }
        [Column("COMP_ID")]
        public int CompId { get; set; }
        [Column("ADD_BY")]
        [StringLength(50)]
        public string AddBy { get; set; }
        [Column("ACT_BY")]
        [StringLength(50)]
        public string ActivatedBy { get; set; }
        [Column("PROC_BY")]
        [StringLength(50)]
        public string ProcessBy { get; set; }
        [Column("CLOSE_BY")]
        [StringLength(50)]
        public string CloseBy { get; set; }
        [Column("ENTRY_TM", TypeName = "datetime")]
        public DateTime? EntryTm { get; set; }
        [Column("ACT_TIME", TypeName = "datetime")]
        public DateTime? ActivationTm { get; set; }
        [Column("PROC_TIME", TypeName = "datetime")]
        public DateTime? ProcessTm { get; set; }
        [Column("CLOSE_TIME", TypeName = "datetime")]
        public DateTime? CloseTm { get; set; }


        [NotMapped]
        public int? NoOfPartyEng { get; set; }

        [ForeignKey(nameof(CompId))]
        [InverseProperty(nameof(CommCompInfo.CommBidMstr))]
        public virtual CommCompInfo Comp { get; set; }

        [ForeignKey(nameof(ProdId))]
        [InverseProperty(nameof(CommProdInfo.CommBidMstr))]
        public virtual CommProdInfo Prod { get; set; }

        [ForeignKey(nameof(WareId))]
        [InverseProperty(nameof(CommProdInfo.CommBidMstr))]
        public virtual CommWarehsInfo Warehouse { get; set; }

        [InverseProperty("Bid")]
        public virtual ICollection<CommBidClntBidder> CommBidClntBidder { get; set; }
    }
}
