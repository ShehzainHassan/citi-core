using citi_core.Enums;
using citi_core.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace citi_core.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserPreferences> UserPreferences { get; set; } = null!;
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Card> Cards { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<OTPVerification> OTPVerifications { get; set; } = null!;
        public DbSet<AuthAuditLog> AuthAuditLogs { get; set; } = null!;
        public DbSet<UserSecuritySettings> UserSecuritySettings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType).Property(nameof(BaseEntity.CreatedAt)).HasDefaultValueSql("GETUTCDATE()");
                    modelBuilder.Entity(entityType.ClrType).Property(nameof(BaseEntity.UpdatedAt)).HasDefaultValueSql("GETUTCDATE()");
                }
            }

            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PhoneNumber).HasMaxLength(20);
                entity.Property(u => u.KycStatus).IsRequired();
            });

            // Configure UserPreferences
            modelBuilder.Entity<UserPreferences>(entity =>
            {
                entity.HasKey(up => up.PreferencesId);
                entity.Property(up => up.Language).IsRequired().HasMaxLength(10).HasDefaultValue("en");
                entity.Property(up => up.Currency).IsRequired().HasMaxLength(10).HasDefaultValue("USD");
                entity.Property(up => up.NotificationsEnabled).HasDefaultValue(false);
                entity.Property(up => up.Theme).IsRequired().HasDefaultValue(Theme.Light);
            });

            // Configure Account
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(a => a.AccountId);
                entity.Property(a => a.AccountNumber).IsRequired().HasMaxLength(50);
                entity.Property(a => a.Currency).IsRequired().HasMaxLength(10);
                entity.Property(a => a.Branch).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Balance).HasColumnType("decimal(18,2)");
                entity.Property(a => a.AvailableBalance).HasColumnType("decimal(18,2)");
                entity.Property(a => a.InterestRate).HasColumnType("decimal(18,2)");
            });

            // Configure Card
            modelBuilder.Entity<Card>(entity =>
            {
                entity.HasKey(c => c.CardId);
                entity.Property(c => c.CardNumber).IsRequired().HasMaxLength(256);
                entity.Property(c => c.CVV).IsRequired().HasMaxLength(10);
                entity.Property(c => c.Last4Digits).IsRequired().HasMaxLength(4);
                entity.Property(c => c.CardHolderName).IsRequired().HasMaxLength(256);
                entity.Property(c => c.CardName).HasMaxLength(256);
                entity.Property(c => c.ValidFrom).IsRequired().HasMaxLength(5);
                entity.Property(c => c.ExpiryDate).IsRequired().HasMaxLength(5);
                entity.Property(c => c.CreditLimit).HasColumnType("decimal(18,2)");
                entity.Property(c => c.AvailableCredit).HasColumnType("decimal(18,2)");
                entity.Property(c => c.IssuedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Transaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.TransactionId);
                entity.Property(t => t.TransactionReference).IsRequired().HasMaxLength(50);
                entity.Property(t => t.TransactionType).IsRequired();
                entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
                entity.Property(t => t.BalanceBefore).HasColumnType("decimal(18,2)");
                entity.Property(t => t.BalanceAfter).HasColumnType("decimal(18,2)");
                entity.Property(t => t.Currency).IsRequired().HasMaxLength(10);
                entity.Property(t => t.Description).HasMaxLength(256);
                entity.Property(t => t.FromAccount).HasMaxLength(50);
                entity.Property(t => t.ToAccount).HasMaxLength(50);
                entity.Property(t => t.BeneficiaryName).HasMaxLength(100);
                entity.Property(t => t.TransactionDate).IsRequired();
                entity.Property(t => t.Status).IsRequired();
            });

            // Configure RefreshToken
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.RefreshTokenId);
                entity.Property(rt => rt.Token).IsRequired().HasMaxLength(500);
                entity.Property(rt => rt.ExpiresAt).IsRequired();
                entity.Property(rt => rt.ReplacedByToken).HasMaxLength(500);
                entity.Property(rt => rt.CreatedByIp).HasMaxLength(50);
                entity.Property(rt => rt.RevokedByIp).HasMaxLength(50);
            });

            // Configure OTPVerification 
            modelBuilder.Entity<OTPVerification>(entity =>
            {
                entity.HasKey(o => o.OTPVerificationId);
                entity.Property(o => o.Code).IsRequired().HasMaxLength(64);
                entity.Property(o => o.Purpose).IsRequired();
                entity.Property(o => o.ExpiresAt).IsRequired();
                entity.Property(o => o.IsUsed).HasDefaultValue(false);
                entity.Property(o => o.AttemptCount).HasDefaultValue(0);
            });

            modelBuilder.Entity<AuthAuditLog>(entity =>
            {
                entity.HasKey(a => a.AuthAuditLogId);
                entity.Property(a => a.Email).HasMaxLength(100);
                entity.Property(a => a.ActionType).IsRequired();
                entity.Property(a => a.IpAddress).HasMaxLength(50);
                entity.Property(a => a.UserAgent).HasMaxLength(256);
                entity.Property(a => a.DeviceId).HasMaxLength(100);
                entity.Property(a => a.AdditionalInfo).HasColumnType("nvarchar(max)");
                entity.Property(a => a.ActionDate).IsRequired();
            });

            modelBuilder.Entity<UserSecuritySettings>(entity =>
            {
                entity.HasKey(u => u.SecuritySettingsId);
                entity.Property(u => u.BiometricPublicKey).HasMaxLength(500);
                entity.Property(u => u.TwoFactorEnabled).HasDefaultValue(false);
                entity.Property(u => u.BiometricEnabled).HasDefaultValue(false);
                entity.Property(u => u.FailedLoginAttempts).HasDefaultValue(0);
            });


            // Indexes
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            //modelBuilder.Entity<User>().HasIndex(u => u.PhoneNumber).IsUnique();

            modelBuilder.Entity<Account>().HasIndex(a => a.AccountNumber).IsUnique();
            modelBuilder.Entity<Account>().HasIndex(a => a.UserId);
            modelBuilder.Entity<Account>().HasIndex(a => new { a.UserId, a.Status });

            modelBuilder.Entity<Card>().HasIndex(c => c.UserId);
            modelBuilder.Entity<Card>().HasIndex(c => c.Last4Digits);
            modelBuilder.Entity<Card>().HasIndex(c => new { c.UserId, c.Status });

            modelBuilder.Entity<Transaction>().HasIndex(t => t.TransactionReference).IsUnique();
            modelBuilder.Entity<Transaction>().HasIndex(t => new { t.AccountId, t.TransactionDate }).HasDatabaseName("IX_Transaction_AccountId_TransactionDate");
            modelBuilder.Entity<Transaction>().HasIndex(t => new { t.CardId, t.TransactionDate }).HasDatabaseName("IX_Transaction_CardId_TransactionDate");

            modelBuilder.Entity<RefreshToken>().HasIndex(rt => rt.Token).IsUnique();
            modelBuilder.Entity<RefreshToken>().HasIndex(rt => new { rt.UserId, rt.ExpiresAt });

            modelBuilder.Entity<OTPVerification>().HasIndex(o => new { o.PhoneNumber, o.ExpiresAt, o.Purpose }).HasDatabaseName("IX_OTP_Phone_Expires_Purpose");
            modelBuilder.Entity<OTPVerification>().HasIndex(o => new { o.Email, o.ExpiresAt, o.Purpose }).HasDatabaseName("IX_OTP_Email_Expires_Purpose");

            modelBuilder.Entity<AuthAuditLog>().HasIndex(a => new { a.UserId, a.ActionDate });
            modelBuilder.Entity<AuthAuditLog>().HasIndex(a => new { a.ActionType, a.ActionDate });

            modelBuilder.Entity<UserSecuritySettings>().HasIndex(u => u.UserId).IsUnique();

            // Relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserPreferences)
                .WithOne(up => up.User)
                .HasForeignKey<UserPreferences>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Accounts)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Cards)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Account>()
                .HasMany(a => a.Cards)
                .WithOne(c => c.Account)
                .HasForeignKey(c => c.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Card)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CardId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AuthAuditLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuthAuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserSecuritySettings>().HasOne(u => u.User)
                  .WithOne(user => user.SecuritySettings)
                  .HasForeignKey<UserSecuritySettings>(u => u.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Query Filters for Soft Delete
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Account>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<Card>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<UserPreferences>().HasQueryFilter(up => !up.IsDeleted);
            modelBuilder.Entity<Transaction>().HasQueryFilter(t => !t.IsDeleted);
            modelBuilder.Entity<RefreshToken>().HasQueryFilter(rt => !rt.IsDeleted);
            modelBuilder.Entity<OTPVerification>().HasQueryFilter(otp => !otp.IsDeleted);
            modelBuilder.Entity<AuthAuditLog>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<UserSecuritySettings>().HasQueryFilter(us => !us.User.IsDeleted);

        }
    }
}