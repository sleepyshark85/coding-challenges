using Elsa.QuizAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elsa.QuizAPI.Data.Configuration;

public class UserQuizQuestionConfiguration : IEntityTypeConfiguration<UserQuizQuestion>
{
    public void Configure(EntityTypeBuilder<UserQuizQuestion> builder)
    {
        builder.HasKey(uqq => uqq.UserQuizQuestionId);

        builder.Property(uqq => uqq.UserQuizQuestionId)
            .ValueGeneratedOnAdd();
        
        builder.Property(uqq => uqq.SubmittedAnswer)
            .HasMaxLength(500);
            
        builder.Property(uqq => uqq.IsAnswered)
            .IsRequired();
            
        builder.Property(uqq => uqq.IsCorrect)
            .IsRequired();
            
        builder.Property(uqq => uqq.PointsEarned)
            .IsRequired();
    }
}