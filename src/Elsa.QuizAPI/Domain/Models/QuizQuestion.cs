namespace Elsa.QuizAPI.Domain.Models;

public class QuizQuestion
{
    private QuizQuestion() { }

    public QuizQuestion(string text, int points, List<QuizQuestionOption>? options)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Question text cannot be empty", nameof(text));
        if (points < 0)
            throw new ArgumentException("Points cannot be negative", nameof(points));

        Text = text.Trim();
        Points = points;
        if (options?.Count > 0)
        { 
            foreach (var option in options)
            {
                AddOption(option.OptionText, option.IsCorrect);
            }
        }
    }

    public Guid QuestionId { get; init; }
    public Guid QuizId { get; init; }
    public string Text { get; init; } = string.Empty;
    public int Points { get; init; }
    public List<QuizQuestionOption> Options { get; init; } = new();

    public void AddOption(string optionText, bool correctOption)
    {
        if (string.IsNullOrWhiteSpace(optionText))
            throw new ArgumentException("Option cannot be empty");
        if (Options.Any(o => o.OptionText.Equals(optionText, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Option already exists");

        Options.Add(new QuizQuestionOption(optionText, correctOption));
    }

    public void RemoveOption(string optionText)
    {
        var option = Options.SingleOrDefault(o => o.OptionText.Equals(optionText, StringComparison.OrdinalIgnoreCase));
        if (option is not null)
        {
            if (option.IsCorrect && Options.Where(o => o.IsCorrect).Count() == 1)
                throw new ArgumentException("Can not remove the correct answer");

            Options.Remove(option);
        }
    }

    public bool IsAnswerCorrect(string submittedAnswer)
    {
        if (string.IsNullOrWhiteSpace(submittedAnswer))
            return false;

        return Options.Any(o => o.IsCorrect && o.OptionText.Equals(submittedAnswer, StringComparison.OrdinalIgnoreCase));
    }
}

public class QuizQuestionOption
{
    public QuizQuestionOption(string optionText, bool isCorrect)
    {
        OptionText = optionText;
        IsCorrect = isCorrect;
    }

    public string OptionText { get; init; }
    public bool IsCorrect { get; init; }
}