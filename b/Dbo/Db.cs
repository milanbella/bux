using Microsoft.EntityFrameworkCore;
using Bux.Dbo.Model;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace Bux.Dbo
{
    public class Db : DbContext, IDataProtectionKeyContext
    {
        private IConfiguration Configuration;
        private IWebHostEnvironment Environment;

        public Db(DbContextOptions<Db> options, IConfiguration configuration, IWebHostEnvironment environment) : base(options)
        {
            this.Configuration = configuration;
            this.Environment = environment;
        }

        public DbSet<Bux.Dbo.Model.User> User => Set<Bux.Dbo.Model.User>();
        public DbSet<JWT> JWT => Set<JWT>();
        public DbSet<Device> Device => Set<Device>();
        public DbSet<UserVerificationCode> UserVerificationCode => Set<UserVerificationCode>();
        public DbSet<Session> Session => Set<Session>();
        public DbSet<Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey> DataProtectionKeys => Set<Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey>();
        public DbSet<SessionUser> SessionUser => Set<SessionUser>();

        public DbSet<BuxEarned> BuxEarned => Set<BuxEarned>();
        public DbSet<BuxRedeemed> BuxRedeemed => Set<BuxRedeemed>();
        public DbSet<ClickGame> ClickGame => Set<ClickGame>();
        public DbSet<GuessGame> GuessGame => Set<GuessGame>();
        public DbSet<Avatar> Avatar => Set<Avatar>();
        public DbSet<AyetUser> AyetUser => Set<AyetUser>();
        public DbSet<AyetOfferWallCallback> AyeOfferWallCallback => Set<AyetOfferWallCallback>();



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User
            modelBuilder.Entity<Bux.Dbo.Model.User>().HasKey(e => e.Id);
            modelBuilder.Entity<Bux.Dbo.Model.User>().HasIndex(e => e.Name).IsUnique(true);
            modelBuilder.Entity<Bux.Dbo.Model.User>().HasIndex(e => e.Email).IsUnique(true);
            modelBuilder.Entity<Bux.Dbo.Model.User>().HasIndex(e => e.EmailVerificationCode).IsUnique(true);
            modelBuilder.Entity<Bux.Dbo.Model.User>().HasOne(e => e.ReferralUser).WithMany().HasForeignKey(e => e.ReferralUserId);
            modelBuilder.Entity<Bux.Dbo.Model.User>().HasIndex(e => new { e.CreatedAt, e.Id, e.Name });

            // JWT
            modelBuilder.Entity<JWT>().HasKey(e => e.Id);
            modelBuilder.Entity<JWT>().HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            modelBuilder.Entity<JWT>().HasOne(e => e.Device).WithMany().HasForeignKey(e => e.DeviceId);

            // Device
            modelBuilder.Entity<Device>().HasKey(e => e.Id);
            modelBuilder.Entity<Device>().HasOne(e => e.Session).WithMany().HasForeignKey(e => e.SessionId);
            modelBuilder.Entity<Device>().HasIndex(e =>  e.DeviceId ).IsUnique(true);


            // UserVerificationCode
            modelBuilder.Entity<UserVerificationCode>().HasKey(e => e.Id);
            modelBuilder.Entity<UserVerificationCode>().HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            modelBuilder.Entity<UserVerificationCode>().HasIndex(e => e.Code).IsUnique(true);

            // Session
            modelBuilder.Entity<Session>().HasKey(e => e.Id);
            modelBuilder.Entity<Session>().HasIndex(e =>  e.SessionId ).IsUnique(true);

            // SessionUser
            modelBuilder.Entity<SessionUser>().HasKey(e => e.Id);
            modelBuilder.Entity<SessionUser>().HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            modelBuilder.Entity<SessionUser>().HasOne(e => e.Session).WithMany().HasForeignKey(e => e.SessionId);


            // BuxEarned
            modelBuilder.Entity<BuxEarned>().HasKey(e => e.Id);
            modelBuilder.Entity<BuxEarned>().HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            modelBuilder.Entity<BuxEarned>().HasIndex(e => new { e.Amount, e.UserId }).IsDescending(true, false);
            modelBuilder.Entity<BuxEarned>().HasIndex(e => new { e.Amount1, e.UserId }).IsDescending(true, false);
            modelBuilder.Entity<BuxEarned>().HasIndex(e =>  e.UserId).IsUnique(true);

            // BuxRedeemed
            modelBuilder.Entity<BuxRedeemed>().HasKey(e => e.Id);
            modelBuilder.Entity<BuxRedeemed>().HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);


            // ClickGame
            modelBuilder.Entity<ClickGame>().HasKey(e => e.Id);
            modelBuilder.Entity<ClickGame>().HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);

            // GuessGame
            modelBuilder.Entity<GuessGame>().HasKey(e => e.Id);
            modelBuilder.Entity<GuessGame>().HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);

            // Avatar
            modelBuilder.Entity<Avatar>().HasKey(e => e.Id);
            modelBuilder.Entity<Avatar>().HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);

            // AyeOfferWallUser
            modelBuilder.Entity<AyetUser>().HasKey(e => e.Id);
            modelBuilder.Entity<AyetUser>().HasIndex(e => e.AyetUserId).IsUnique(true);
            modelBuilder.Entity<AyetUser>().HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);

            // AyeOfferWallCallback
            modelBuilder.Entity<AyetOfferWallCallback>().HasKey(e => e.Id);
            modelBuilder.Entity<AyetOfferWallCallback>().HasIndex(e => e.TransactionId).IsUnique(true);
            modelBuilder.Entity<AyetOfferWallCallback>().HasIndex(e => e.ReceivedAt);

			Bux.Seed.Users.Seed(modelBuilder, Configuration, Environment);

        }
    }
}


