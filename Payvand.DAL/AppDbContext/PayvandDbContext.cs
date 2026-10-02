using Microsoft.EntityFrameworkCore;
using Payvand.DAL.Entities;


namespace Payvand.DAL.AppDbContext
{
    public class PayvandDbContext : DbContext
    {
        public PayvandDbContext(DbContextOptions options) : base(options)
        {
        }

        protected PayvandDbContext()
        {
        }

        public DbSet<ClickLog> ClickLogs { get; set; }
        public DbSet<GuestSession> GuestSessions { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ViolationReport> ViolationReports { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<QRCodeEntity> QrCodes { get; set; }
        public DbSet<ShortenedLink> ShortenedLinks { get; set; }
        public DbSet<Otp> Otp { get; set; }
        public DbSet<Article> Articles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(c => c.GoogleSubject)
                .IsUnique()
                .HasFilter("[GoogleSubject] IS NOT NULL");
            modelBuilder.Entity<GuestSession>()
                .HasIndex(c => c.SessionTokenHash)
                .IsUnique()
                .HasFilter("[SessionTokenHash] IS NOT NULL");
            modelBuilder.Entity<ShortenedLink>()
                .HasIndex(c => c.ShorteCode)
                .IsUnique();
            modelBuilder.Entity<Article>()
                .HasIndex(c => c.Slug)
                .IsUnique();
            modelBuilder.Entity<Article>()
                .Property(c => c.Title)
                .HasMaxLength(180);
            modelBuilder.Entity<Article>()
                .Property(c => c.Slug)
                .HasMaxLength(180);
            modelBuilder.Entity<Article>()
                .Property(c => c.Category)
                .HasMaxLength(80);
            modelBuilder.Entity<Article>()
                .HasQueryFilter(c => c.IsPublished);
            modelBuilder.Entity<ShortenedLink>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<ClickLog>().HasQueryFilter(c => !c.Link.IsDeleted);
            modelBuilder.Entity<QRCodeEntity>().HasQueryFilter(c => !c.ISDeleted && !c.User.IsDeleted);
            modelBuilder.Entity<Message>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<ViolationReport>().HasQueryFilter(c => !c.IsDeleted);
        }
    }
}
