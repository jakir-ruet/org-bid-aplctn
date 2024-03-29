using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace orgBidAplctn.Models.Data
{
    [Table("COMM_PROD_INFO")]
    public partial class CommProdInfo
    {
        public CommProdInfo()
        {
            CommBidMstr = new HashSet<CommBidMstr>();
        }

        [Key]
        [Column("PROD_ID")]
        [StringLength(12)]
        public string ProdId { get; set; }
        [Required]
        [Column("PROD_NAME")]
        [StringLength(100)]
        public string ProdName { get; set; }
        [Column("PROD_CODE")]
        [StringLength(5)]
        public string ProdCode { get; set; }
        [Column("BARCD_ID")]
        [StringLength(25)]
        public string BarcdId { get; set; }
        [Required]
        [Column("CAT_ID")]
        [StringLength(12)]
        public string CatId { get; set; }
        [Required]
        [Column("WGHT")]
        [StringLength(5)]
        public string Wght { get; set; }
        [Column("PROD_SPEC")]
        public string ProdSpec { get; set; }
        [Column("LEAD_TM")]
        public int LeadTm { get; set; }
        [Column("MIN_QNTY", TypeName = "decimal(18, 3)")]
        public decimal MinQnty { get; set; }
        [Column("MAX_QNTY", TypeName = "decimal(18, 3)")]
        public decimal MaxQnty { get; set; }
        [Column("IS_DISCONTINUE")]
        public byte IsDiscontinue { get; set; }
        [Column("IMG_FILE_NM")]
        [StringLength(50)]
        public string ImgFileNm { get; set; }
        [Column("COMP_ID")]
        public int CompId { get; set; }
        [Column("ADD_BY")]
        public long AddBy { get; set; }
        [Column("ENTRY_TM", TypeName = "datetime")]
        public DateTime EntryTm { get; set; }

        [ForeignKey(nameof(CatId))]
        [InverseProperty(nameof(CommProdCat.CommProdInfo))]
        public virtual CommProdCat Cat { get; set; }

        [ForeignKey(nameof(CompId))]
        [InverseProperty(nameof(CommCompInfo.CommProdInfo))]
        public virtual CommCompInfo Comp { get; set; }
        [InverseProperty("Prod")]
        public virtual ICollection<CommBidMstr> CommBidMstr { get; set; }
    }
}
