using Microsoft.EntityFrameworkCore;
using movie_wed_api.Common;
using movie_wed_api.Models;


namespace movie_wed_api.Database
{
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Episode> Episodes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Charset/Collation cho MySQL
            modelBuilder.HasCharSet("utf8mb4").UseCollation("utf8mb4_unicode_ci");

            // ===== RÀNG BUỘC & QUAN HỆ =====

            // Users
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Episodes: 1 phim không trùng số tập
            modelBuilder.Entity<Episode>()
                .HasIndex(e => new { e.MovieId, e.EpisodeNumber })
                .IsUnique();

            // Ratings: 1 user chỉ được rate 1 lần/1 phim
            modelBuilder.Entity<Rating>()
                .HasIndex(r => new { r.UserId, r.MovieId })
                .IsUnique();

            // Favorites: tránh trùng
            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.MovieId })
                .IsUnique();

            // Quan hệ
            modelBuilder.Entity<Movie>()
                .HasMany(m => m.Episodes)
                .WithOne(e => e.Movie)
                .HasForeignKey(e => e.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Movie>()
                .HasMany(m => m.Comments)
                .WithOne(c => c.Movie)
                .HasForeignKey(c => c.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Movie)
                .WithMany(m => m.Ratings)
                .HasForeignKey(r => r.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Movie)
                .WithMany(m => m.Favorites)
                .HasForeignKey(f => f.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== Default CURRENT_TIMESTAMP cho CreatedAt =====
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var createdAtProp = entity.FindProperty("CreatedAt");
                if (createdAtProp != null)
                {
                    createdAtProp.SetColumnType("timestamp");
                    createdAtProp.SetDefaultValueSql("CURRENT_TIMESTAMP");
                }
            }

            // ===== Snake_case tất cả tên bảng/cột/index/constraint =====
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName()?.ToSnakeCase(true));

                foreach (var property in entity.GetProperties())
                    property.SetColumnName(property.Name.ToSnakeCase(true));

                foreach (var key in entity.GetKeys())
                    key.SetName(key.GetName()?.ToSnakeCase(true));

                foreach (var foreignKey in entity.GetForeignKeys())
                    foreignKey.SetConstraintName(foreignKey.GetConstraintName()?.ToSnakeCase(true));

                foreach (var index in entity.GetIndexes())
                    index.SetDatabaseName(index.GetDatabaseName()?.ToSnakeCase(true));
            }
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Modified)
                {
                    var updatedAtProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                    if (updatedAtProp != null)
                    {
                        updatedAtProp.CurrentValue = DateTime.UtcNow;
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Modified)
                {
                    var updatedAtProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                    if (updatedAtProp != null)
                    {
                        updatedAtProp.CurrentValue = DateTime.UtcNow;
                    }
                }
            }

            return base.SaveChanges();
        }



    }
}
