using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace orgBidAplctn.Models.Data
{
    [Table("COMM_LOGIN_INFO")]
    public partial class CommLoginInfo
    {
        public CommLoginInfo()
        {
            CommNotifyLists = new HashSet<CommNotifyList>();
        }

        [Key]
        [Column("USER_ID")]
        public long UserId { get; set; }

        [Required]
        [Column("USER_NM")]
        [StringLength(50)]
        public string UserNm { get; set; }

        [Required]
        [Column("USER_PASS")]
        [StringLength(32)]
        public string UserPass { get; set; }

        [Column("FAST_NAME")]
        [StringLength(100)]
        public string FastName { get; set; }

        [Column("LAST_NAME")]
        [StringLength(100)]
        public string LastName { get; set; }

        [Column("GENDER")]
        [StringLength(1)]
        public string Gender { get; set; }

        [Column("BRTH_DT", TypeName = "datetime")]
        public DateTime? BrthDt { get; set; }

        [Column("USER_ADD")]
        [StringLength(250)]
        public string UserAdd { get; set; }

        [Column("USER_CITY")]
        [StringLength(50)]
        public string UserCity { get; set; }

        [Column("USER_CNTRY")]
        [StringLength(50)]
        public string UserCntry { get; set; }

        [Column("EMAIL_ADD")]
        [StringLength(100)]
        public string EmailAdd { get; set; }

        [Column("CONT_NO")]
        [StringLength(50)]
        public string ContNo { get; set; }

        [Column("LOGIN_TP")]
        public int LoginTp { get; set; }

        [Column("CAN_MOD")]
        public byte? CanMod { get; set; }

        [Column("CAN_DEL")]
        public byte? CanDel { get; set; }

        [Column("IS_ACTIVE")]
        public byte IsActive { get; set; }

        [Column("PROFILE_PIC")]
        [StringLength(50)]
        public string ProfileImg { get; set; }

        [Column("COMP_ID")]
        public int CompId { get; set; }

        [Column("LAST_LOG_TM", TypeName = "datetime")]
        public DateTime? LastLogTm { get; set; }

        [ForeignKey(nameof(CompId))]
        [InverseProperty(nameof(CommCompInfo.CommLoginInfo))]
        public virtual CommCompInfo Comp { get; set; }

        [InverseProperty("NotByNavigation")]
        public virtual ICollection<CommNotifyList> CommNotifyLists { get; set; }
    }
}
