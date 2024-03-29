using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace orgBidAplctn.Models.Data
{
    [Table("COMM_COUNTRY_INFO")]
    public partial class CommCountryInfo
    {
        [Key]
        [Column("CNTRY_ID")]
        [StringLength(2)]
        public string CntryId { get; set; }
        [Column("CNTRY_NAME")]
        [StringLength(50)]
        public string CntryName { get; set; }
    }
}
