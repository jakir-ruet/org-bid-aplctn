using Microsoft.EntityFrameworkCore;
using orgBidAplctn.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace orgBidAplctn.Models
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public virtual DbSet<CommCompInfo> CommCompInfos { get; set; }
        public virtual DbSet<CommLoginInfo> CommLoginInfos { get; set; }
        public virtual DbSet<CommPartyCat> CommPartyCats { get; set; }
        public virtual DbSet<CommPartyInfo> CommPartyInfos { get; set; }
        public virtual DbSet<CommProdCat> CommProdCats { get; set; }
        public virtual DbSet<CommProdInfo> CommProdInfos { get; set; }
        public virtual DbSet<CommSmsLog> CommSmsLogs { get; set; }
        public virtual DbSet<CommWarehsInfo> CommWarehsInfos { get; set; }
        public virtual DbSet<CommBidClntBidder> CommBidClntBidders { get; set; }
        public virtual DbSet<CommBidMstr> CommBidMstrs { get; set; }
        public virtual DbSet<CommCountryInfo> CommCountryInfos { get; set; }
        public virtual DbSet<CommSmsSett> CommSmsSetts { get; set; }
        public virtual DbSet<CommNotifyList> CommNotifyLists { get; set; }
        public virtual DbSet<GetVoucherIdPK> GetVoucherIdPKs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GetVoucherIdPK>().HasNoKey();

            modelBuilder.Entity<CommCompInfo>(entity =>
            {
                entity.Property(e => e.CompStrtNo)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.ContEmail).IsUnicode(false);

                entity.Property(e => e.ContOthNo).IsUnicode(false);

                entity.Property(e => e.ContSmsNo)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.DefCurr)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasDefaultValueSql("('BDT')");

                entity.Property(e => e.LogoFileNm).IsUnicode(false);

                entity.Property(e => e.VatPrc).HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<CommLoginInfo>(entity =>
            {
                entity.HasIndex(e => new { e.UserNm, e.UserPass, e.CompId })
                    .HasDatabaseName("IX_COMM_LOGIN_INFO");

                entity.Property(e => e.CanDel).HasDefaultValueSql("((0))");

                entity.Property(e => e.CanMod).HasDefaultValueSql("((0))");

                entity.Property(e => e.ContNo).IsUnicode(false);

                entity.Property(e => e.EmailAdd).IsUnicode(false);

                entity.Property(e => e.Gender)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LoginTp)
                    .HasDefaultValueSql("((1))")
                    .HasComment("1 for User, 2 for Manager, 3 for Admin");

                entity.Property(e => e.UserPass).IsUnicode(false);

                entity.HasOne(d => d.Comp)
                    .WithMany(p => p.CommLoginInfo)
                    .HasForeignKey(d => d.CompId)
                    .HasConstraintName("FK_COMM_LOGIN_INFO_COMM_COMP_INFO");
            });

            modelBuilder.Entity<CommPartyCat>(entity =>
            {
                entity.Property(e => e.CatId)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.CatName).IsUnicode(false);

                entity.Property(e => e.CreateTm).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<CommPartyInfo>(entity =>
            {
                entity.HasIndex(e => e.CatId)
                    .HasDatabaseName("IX_COMM_PARTY_INFO");

                entity.HasIndex(e => new { e.PartyName, e.CompId })
                    .HasDatabaseName("IX_COMM_PARTY_INFO_NMCM");

                entity.Property(e => e.PartyId)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.CatId)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.CreateTm).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CustType)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasDefaultValueSql("('C')");

                entity.Property(e => e.EmailAdd).IsUnicode(false);

                entity.Property(e => e.ImgFileNm).IsUnicode(false);

                entity.Property(e => e.OthContNo).IsUnicode(false);

                entity.Property(e => e.PartyCode)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.SmsContNo).IsUnicode(false);

                entity.HasOne(d => d.Cat)
                    .WithMany(p => p.CommPartyInfo)
                    .HasForeignKey(d => d.CatId)
                    .HasConstraintName("FK_COMM_PARTY_INFO_COMM_PARTY_CAT");

                entity.HasOne(d => d.Comp)
                    .WithMany(p => p.CommPartyInfo)
                    .HasForeignKey(d => d.CompId)
                    .HasConstraintName("FK_COMM_PARTY_INFO_COMM_COMP_INFO");
            });

            modelBuilder.Entity<CommProdCat>(entity =>
            {
                entity.Property(e => e.CatId)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.BaseUnit)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.CreateTm).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsInvItem).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<CommProdInfo>(entity =>
            {
                entity.HasIndex(e => e.CatId)
                    .HasDatabaseName("IX_COMM_PROD_INFO");

                entity.HasIndex(e => new { e.ProdName, e.CompId })
                    .HasDatabaseName("IX_COMM_PROD_INFO_NMCM");

                entity.Property(e => e.ProdId)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.BarcdId).IsUnicode(false);

                entity.Property(e => e.CatId)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.EntryTm).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ImgFileNm).IsUnicode(false);

                entity.Property(e => e.ProdCode)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Wght)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.HasOne(d => d.Cat)
                    .WithMany(p => p.CommProdInfo)
                    .HasForeignKey(d => d.CatId)
                    .HasConstraintName("FK_COMM_PROD_INFO_COMM_PROD_CAT");

                entity.HasOne(d => d.Comp)
                    .WithMany(p => p.CommProdInfo)
                    .HasForeignKey(d => d.CompId)
                    .HasConstraintName("FK_COMM_PROD_INFO_COMM_COMP_INFO");
            });

            modelBuilder.Entity<CommSmsLog>(entity =>
            {
                entity.HasIndex(e => e.SmsDtTm)
                    .HasDatabaseName("IX_COMM_SMS_LOG");

                entity.Property(e => e.PartNo).IsUnicode(false);

                entity.Property(e => e.RefId)
                    .IsUnicode(false)
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.SmsContain)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.SmsFrom)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.SmsPhone).IsUnicode(false);

                entity.Property(e => e.SmsStat)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasDefaultValueSql("('U')");

                entity.Property(e => e.SmsType)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasDefaultValueSql("('A')")
                    .HasComment("A=Attendance, S=Survey, W=Web Sales");

                entity.Property(e => e.SmsUplStat)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasDefaultValueSql("('N')");

                entity.Property(e => e.SmsValidity)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasDefaultValueSql("('V')")
                    .HasComment("Valid Or Invalid Message");
            });

            modelBuilder.Entity<CommWarehsInfo>(entity =>
            {
                entity.Property(e => e.WareId)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.CreateTm).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Comp)
                    .WithMany(p => p.CommWarehsInfo)
                    .HasForeignKey(d => d.CompId)
                    .HasConstraintName("FK_COMM_WAREHS_INFO_COMM_COMP_INFO");
            });

            modelBuilder.Entity<CommBidClntBidder>(entity =>
            {
                entity.Property(e => e.AllocQnty).HasDefaultValueSql("((0))");

                entity.Property(e => e.AllocRate).HasDefaultValueSql("((0))");

                entity.Property(e => e.BidAttnStat).HasDefaultValueSql("((0))");

                entity.Property(e => e.BidQnty).HasDefaultValueSql("((0))");

                entity.Property(e => e.BidRate).HasDefaultValueSql("((0))");

                entity.Property(e => e.PartyId)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.SmsContNo).IsUnicode(false);

                entity.Property(e => e.SmsSendStat).HasDefaultValueSql("((0))"); // SmsAllocStat
                
                entity.Property(e => e.SmsAllocStat).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.Bid)
                    .WithMany(p => p.CommBidClntBidder)
                    .HasForeignKey(d => d.BidId)
                    .HasConstraintName("FK_COMM_BID_CLNT_BIDDER_COMM_BID_MSTR");

                entity.HasOne(d => d.Party)
                    .WithMany(p => p.CommBidClntBidder)
                    .HasForeignKey(d => d.PartyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_COMM_BID_CLNT_BIDDER_COMM_PARTY_INFO");
            });

            modelBuilder.Entity<CommBidMstr>(entity =>
            {
                entity.Property(e => e.AllocRate).HasDefaultValueSql("((0))");

                entity.Property(e => e.EntryTm).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ProdId)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.AllocQnty).HasDefaultValueSql("((0))");

                entity.Property(e => e.AllocRate).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.Comp)
                    .WithMany(p => p.CommBidMstr)
                    .HasForeignKey(d => d.CompId)
                    .HasConstraintName("FK_COMM_BID_MSTR_COMM_COMP_INFO");

                entity.HasOne(d => d.Prod)
                    .WithMany(p => p.CommBidMstr)
                    .HasForeignKey(d => d.ProdId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_COMM_BID_MSTR_COMM_PROD_INFO");

                entity.Property(e => e.WareId)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.HasOne(d => d.Warehouse)
                    .WithMany(p => p.CommBidMstr)
                    .HasForeignKey(d => d.WareId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<CommCountryInfo>(entity =>
            {
                entity.Property(e => e.CntryId)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.CntryName).IsUnicode(false);
            });

            modelBuilder.Entity<CommSmsSett>(entity =>
            {
                entity.Property(e => e.ReceiveNo).IsUnicode(false);

                entity.Property(e => e.SenderNo).IsUnicode(false);

                entity.Property(e => e.TokenNo).IsUnicode(false);
            });

            modelBuilder.Entity<CommNotifyList>(entity =>
            {
                entity.Property(e => e.CreatTm).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.IsRead).HasDefaultValueSql("((0))");
                entity.Property(e => e.NotType).HasDefaultValueSql("((0))");
                entity.HasOne(d => d.NotByNavigation)
                    .WithMany(p => p.CommNotifyLists)
                    .HasForeignKey(d => d.NotBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
