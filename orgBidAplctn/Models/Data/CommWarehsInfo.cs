using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace orgBidAplctn.Models.Data
{
    [Table("COMM_WAREHS_INFO")]
    public partial class CommWarehsInfo
    {
        public CommWarehsInfo()
        {
            CommBidMstr = new HashSet<CommBidMstr>();
        }

        [Key]
        [Column("WARE_ID")]
        [StringLength(12)]
        public string WareId { get; set; }
        [Required]
        [Column("WARE_NAME")]
        [StringLength(50)]
        public string WareName { get; set; }
        [Column("WARE_ADD")]
        [StringLength(250)]
        public string WareAdd { get; set; }
        [Column("IS_DISCONTINUE")]
        public byte IsDiscontinue { get; set; }
        [Column("COMP_ID")]
        public int CompId { get; set; }
        [Column("ADD_BY")]
        public long? AddBy { get; set; }
        [Column("CREATE_TM", TypeName = "datetime")]
        public DateTime? CreateTm { get; set; }

        [ForeignKey(nameof(CompId))]
        [InverseProperty(nameof(CommCompInfo.CommWarehsInfo))]
        public virtual CommCompInfo Comp { get; set; }

        [InverseProperty("Warehouse")]
        public virtual ICollection<CommBidMstr> CommBidMstr { get; set; }
    }
}
