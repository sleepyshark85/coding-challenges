using Elsa.QuizAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elsa.QuizAPI.Data.Configuration;

public class UserQuizConfiguration : IEntityTypeConfiguration<UserQuiz>
{
    public void Configure(EntityTypeBuilder<UserQuiz> builder)
    {
        builder.HasKey(uq => uq.UserQuizId);

        builder.Property(uq => uq.UserQuizId)
            .ValueGeneratedOnAdd();
        
        builder.Property(uq => uq.Status)
            .HasConversion<int>()
            .IsRequired();
            
        builder.Property(uq => uq.StartedAt)
            .IsRequired();
            
        builder.Property(uq => uq.ExpiresAt)
            .IsRequired();
            
        builder.Metadata.FindNavigation(nameof(UserQuiz.QuestionAttempts))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
            
        builder.HasMany(uq => uq.QuestionAttempts)
            .WithOne()
            .HasForeignKey(uqq => uqq.UserQuizId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}