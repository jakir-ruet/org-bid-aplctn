using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace orgBidAplctn.Models.Data
{
    [Table("COMM_PARTY_INFO")]
    public partial class CommPartyInfo
    {
        public CommPartyInfo()
        {
            CommBidClntBidder = new HashSet<CommBidClntBidder>();
        }

        [Key]
        [Column("PARTY_ID")]
        [StringLength(12)]
        public string PartyId { get; set; }
        [Required]
        [Column("PARTY_NAME")]
        [StringLength(100)]
        public string PartyName { get; set; }
        [Column("PARTY_CODE")]
        [StringLength(5)]
        public string PartyCode { get; set; }
        [Column("PARTY_ADD")]
        [StringLength(250)]
        public string PartyAdd { get; set; }
        [Column("DIST_NM")]
        [StringLength(50)]
        public string DistNm { get; set; }
        [Column("CNTRY_NM")]
        [StringLength(50)]
        public string CntryNm { get; set; }
        [Required]
        [Column("CAT_ID")]
        [StringLength(12)]
        public string CatId { get; set; }
        [Column("SMS_CONT_NO")]
        [StringLength(20)]
        public string SmsContNo { get; set; }
        [Column("OTH_CONT_NO")]
        [StringLength(50)]
        public string OthContNo { get; set; }
        [Column("EMAIL_ADD")]
        [StringLength(100)]
        public string EmailAdd { get; set; }
        [Column("DOB", TypeName = "datetime")]
        public DateTime? Dob { get; set; }
        [Required]
        [Column("CUST_TYPE")]
        [StringLength(1)]
        public string CustType { get; set; }
        [Column("IS_DISCONTINUE")]
        public byte IsDiscontinue { get; set; }
        [Column("IMG_FILE_NM")]
        [StringLength(50)]
        public string ImgFileNm { get; set; }
        [Column("COMP_ID")]
        public int CompId { get; set; }
        [Column("ADD_BY")]
        public long? AddBy { get; set; }
        [Column("CREATE_TM", TypeName = "datetime")]
        public DateTime? CreateTm { get; set; }

        [ForeignKey(nameof(CatId))]
        [InverseProperty(nameof(CommPartyCat.CommPartyInfo))]
        public virtual CommPartyCat Cat { get; set; }
        [ForeignKey(nameof(CompId))]
        [InverseProperty(nameof(CommCompInfo.CommPartyInfo))]
        public virtual CommCompInfo Comp { get; set; }
        [InverseProperty("Party")]
        public virtual ICollection<CommBidClntBidder> CommBidClntBidder { get; set; }
    }
}
