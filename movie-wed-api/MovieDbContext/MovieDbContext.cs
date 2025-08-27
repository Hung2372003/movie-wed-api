using Microsoft.EntityFrameworkCore;
using movie_wed_api.Common;
using movie_wed_api.Models;


namespace movie_wed_api.MovieDbContext
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

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Thay đổi tên bảng
                entity.SetTableName(entity?.GetTableName()?.ToSnakeCase(true));

                // Thay đổi tên cột
                foreach (var property in entity!.GetProperties())
                {
                    property.SetColumnName(property.Name.ToSnakeCase(true));
                }

                // Thay đổi tên khóa
                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key?.GetName()?.ToSnakeCase(true));
                }

                // Thay đổi tên khóa ngoại
                foreach (var foreignKey in entity.GetForeignKeys())
                {
                    foreignKey.SetConstraintName(foreignKey?.GetConstraintName()?.ToSnakeCase(true));
                }

                // Thay đổi tên chỉ mục
                foreach (var index in entity.GetIndexes())
                {
                    index.SetDatabaseName(index?.GetDatabaseName()?.ToSnakeCase(true));
                }
            }
            // Unique constraint cho Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Quan hệ Movie - Episode (1-n)
            modelBuilder.Entity<Movie>()
                .HasMany(m => m.Episodes)
                .WithOne(e => e.Movie)
                .HasForeignKey(e => e.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ Movie - Comment
            modelBuilder.Entity<Movie>()
                .HasMany(m => m.Comments)
                .WithOne(c => c.Movie)
                .HasForeignKey(c => c.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ User - Comment
            modelBuilder.Entity<User>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ Rating
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Movie)
                .WithMany(m => m.Ratings)
                .HasForeignKey(r => r.MovieId);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.UserId);

            // Quan hệ Favorite
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Movie)
                .WithMany(m => m.Favorites)
                .HasForeignKey(f => f.MovieId);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId);
        }
    }
}
