using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace orgBidAplctn.Models.Data
{
    [Table("COMM_COMP_INFO")]
    public partial class CommCompInfo
    {
        public CommCompInfo()
        {
            CommLoginInfo = new HashSet<CommLoginInfo>();
            CommPartyInfo = new HashSet<CommPartyInfo>();
            CommProdInfo = new HashSet<CommProdInfo>();
            CommWarehsInfo = new HashSet<CommWarehsInfo>();
            CommBidMstr = new HashSet<CommBidMstr>();
        }

        [Key]
        [Column("COMP_ID")]
        public int CompId { get; set; }
        [Required]
        [Column("COMP_NAME")]
        [StringLength(50)]
        public string CompName { get; set; }
        [Required]
        [Column("ADDRESS")]
        [StringLength(250)]
        public string Address { get; set; }
        [Required]
        [Column("COMP_AREA")]
        [StringLength(50)]
        public string CompArea { get; set; }
        [Required]
        [Column("COMP_DIST")]
        [StringLength(50)]
        public string CompDist { get; set; }
        [Required]
        [Column("COMP_COUNTRY")]
        [StringLength(50)]
        public string CompCountry { get; set; }
        [Column("POST_CODE")]
        public int? PostCode { get; set; }
        [Column("BIN_NO")]
        [StringLength(20)]
        public string BinNo { get; set; }
        [Column("VAT_PRC", TypeName = "decimal(18, 2)")]
        public decimal? VatPrc { get; set; }
        [Column("CONT_PERSN")]
        [StringLength(50)]
        public string ContPersn { get; set; }
        [Column("CONT_EMAIL")]
        [StringLength(100)]
        public string ContEmail { get; set; }
        [Column("CONT_SMS_NO")]
        [StringLength(14)]
        public string ContSmsNo { get; set; }
        [Column("CONT_OTH_NO")]
        [StringLength(50)]
        public string ContOthNo { get; set; }
        [Required]
        [Column("DEF_CURR")]
        [StringLength(3)]
        public string DefCurr { get; set; }
        [Required]
        [Column("COMP_STRT_NO")]
        [StringLength(2)]
        public string CompStrtNo { get; set; }
        [Column("DSCNTND")]
        public byte Dscntnd { get; set; }
        [Column("LOGO_FILE_NM")]
        [StringLength(50)]
        public string LogoFileNm { get; set; }
        [Column("LOGO_DB", TypeName = "image")]
        public byte[] LogoDb { get; set; }

        [InverseProperty("Comp")]
        public virtual ICollection<CommLoginInfo> CommLoginInfo { get; set; }
        [InverseProperty("Comp")]
        public virtual ICollection<CommPartyInfo> CommPartyInfo { get; set; }
        [InverseProperty("Comp")]
        public virtual ICollection<CommProdInfo> CommProdInfo { get; set; }
        [InverseProperty("Comp")]
        public virtual ICollection<CommWarehsInfo> CommWarehsInfo { get; set; }
        [InverseProperty("Comp")]
        public virtual ICollection<CommBidMstr> CommBidMstr { get; set; }
    }
}
