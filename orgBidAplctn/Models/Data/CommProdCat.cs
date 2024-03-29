using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace orgBidAplctn.Models.Data
{
    [Table("COMM_PROD_CAT")]
    public partial class CommProdCat
    {
        public CommProdCat()
        {
            CommProdInfo = new HashSet<CommProdInfo>();
        }
        [Key]
        [Column("CAT_ID")]
        [StringLength(12)]
        public string CatId { get; set; }
        [Required]
        [Column("CAT_NAME")]
        [StringLength(50)]
        public string CatName { get; set; }
        [Required]
        [Column("BASE_UNIT")]
        [StringLength(5)]
        public string BaseUnit { get; set; }
        
        [Column("IS_INV_ITEM")]
        public byte? IsInvItem { get; set; }
        
        [Column("IS_DISCONTINUE")]
        public byte IsDiscontinue { get; set; }
        [Column("COMP_ID")]
        public int CompId { get; set; }
        [Column("ADD_BY")]
        public long AddBy { get; set; }
        [Column("CREATE_TM", TypeName = "datetime")]
        public DateTime? CreateTm { get; set; }
        [InverseProperty("Cat")]
        public virtual ICollection<CommProdInfo> CommProdInfo { get; set; }
    }
}
