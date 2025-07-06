using Elsa.QuizAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elsa.QuizAPI.Data.Configuration;

public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.HasKey(q => q.QuizId);

        builder.Property(q => q.QuizId)
            .ValueGeneratedOnAdd();
        
        builder.Property(q => q.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(q => q.Description)
            .HasMaxLength(1000);
            
        // builder.Metadata.FindNavigation(nameof(Quiz.Questions))!
        //     .SetPropertyAccessMode(PropertyAccessMode.Field);
            
        builder.HasMany(q => q.Questions)
            .WithOne()
            .HasForeignKey(qq => qq.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}