using Elsa.QuizAPI.Data.Configuration;
using Elsa.QuizAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Elsa.QuizAPI.Data;

public class QuizDbContext : DbContext
{
    public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options) { }

    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserQuiz> UserQuizzes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new QuizConfiguration());
        modelBuilder.ApplyConfiguration(new QuizQuestionConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserQuizConfiguration());
        modelBuilder.ApplyConfiguration(new UserQuizQuestionConfiguration());
    }
}