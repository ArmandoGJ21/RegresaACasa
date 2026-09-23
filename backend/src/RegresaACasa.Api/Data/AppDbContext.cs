using Microsoft.EntityFrameworkCore;
using RegresaACasa.Api.Models.Entities;

namespace RegresaACasa.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pet>(pet =>
        {
            pet.ToTable("pets");
            pet.HasKey(p => p.Id);
            pet.Property(p => p.Id).HasColumnName("id");
            pet.Property(p => p.PetType).HasColumnName("pet_type").HasMaxLength(30).IsRequired();
            pet.Property(p => p.Name).HasColumnName("name").HasMaxLength(60);
            pet.Property(p => p.Breed).HasColumnName("breed").HasMaxLength(60);
            pet.Property(p => p.ColorDescription).HasColumnName("color_description").HasMaxLength(200).IsRequired();
            pet.Property(p => p.Zone).HasColumnName("zone").HasMaxLength(80).IsRequired();
            pet.Property(p => p.ContactInfo).HasColumnName("contact_info").HasMaxLength(80).IsRequired();
            pet.Property(p => p.ImageUrl).HasColumnName("image_url").HasMaxLength(500).IsRequired();
            pet.Property(p => p.CreatedAt).HasColumnName("created_at");
            pet.HasIndex(p => p.CreatedAt);

            pet.HasMany(p => p.Comments)
                .WithOne(c => c.Pet)
                .HasForeignKey(c => c.PetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Comment>(comment =>
        {
            comment.ToTable("comments");
            comment.HasKey(c => c.CommentId);
            comment.Property(c => c.CommentId).HasColumnName("comment_id");
            comment.Property(c => c.PetId).HasColumnName("pet_id");
            comment.Property(c => c.UserName).HasColumnName("user_name").HasMaxLength(60).IsRequired();
            comment.Property(c => c.Text).HasColumnName("text").HasMaxLength(500).IsRequired();
            comment.Property(c => c.CreatedAt).HasColumnName("created_at");
        });
    }
}
