using Elsa.QuizAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elsa.QuizAPI.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.UserId);

        builder.Property(u => u.UserId)
            .ValueGeneratedOnAdd();
        
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Metadata.FindNavigation(nameof(User.QuizAttempts))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
            
        builder.HasMany(u => u.QuizAttempts)
            .WithOne()
            .HasForeignKey(uq => uq.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}