using Elsa.QuizAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elsa.QuizAPI.Data.Configuration;

public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.HasKey(qq => qq.QuestionId);

        builder.Property(qq => qq.QuestionId)
            .ValueGeneratedOnAdd();
        
        builder.Property(qq => qq.Text)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(qq => qq.Points)
            .IsRequired();
            
        builder.Property(qq => qq.Options)
            .HasConversion(
                options => System.Text.Json.JsonSerializer.Serialize(options, (System.Text.Json.JsonSerializerOptions?)null),
                json => System.Text.Json.JsonSerializer.Deserialize<List<QuizQuestionOption>>(json, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<QuizQuestionOption>())
            .HasColumnType("jsonb");
    }
}