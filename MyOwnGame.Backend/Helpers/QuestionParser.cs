using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.SiqPackage;
using MyOwnGame.Backend.Parsers.QuestionInfo;
using MyOwnGame.Backend.Parsers.QuestionInfo.Answer;
using MyOwnGame.Backend.Parsers.QuestionInfo.QuestionParsers;

namespace MyOwnGame.Backend.Helpers;

public class QuestionParser
{
    private readonly IEnumerable<IAnswerParser> _answerParsers;

    private readonly IEnumerable<IQuestionParser> _questionParsers;
    
    public QuestionParser(IEnumerable<IQuestionParser> questionParsers, IEnumerable<IAnswerParser> answerParsers)
    {
        _questionParsers = questionParsers;
        _answerParsers = answerParsers;
    }

    public QuestionInfo Parse(Question question)
    {

        var questionParser = GetQuestionParser(question);
        var answerParser = GetAnswerParser(question);
        
        var questionInfo = new QuestionInfo()
        {
            Answer = answerParser.ParseAnswer(question),
            Question = questionParser.ParseQuestion(question),
            Price = question.Price
        };

        return questionInfo;
    }

    private IQuestionParser GetQuestionParser(Question question)
    {
        var atom = question.Scenario.Atom.FirstOrDefault();

        if (atom is null)
        {
            throw new Exception("Не найден элемент atom в вопросе лол");
        }

        IQuestionParser? parser = null;

        if (atom.Type is null)
        {
            parser = _questionParsers.OfType<TextQuestionParser>().FirstOrDefault();
        }
        else
        {
            parser = _questionParsers.OfType<MediaQuestionParser>().FirstOrDefault();
        }
        
        if (parser is null)
        {
            throw new Exception("Не найден парсер для вопрсоа");
        }

        return parser;
    }

    private IAnswerParser GetAnswerParser(Question question)
    {
        IAnswerParser? parser = null;
        
        if (question.Scenario.Atom.Count > 1 && question.Scenario.Atom.Exists(a=> a.Type == "marker") && !question.Scenario.Atom.Exists(a=> a.Type == "say"))
        {
            parser = _answerParsers.OfType<MediaAnswerParser>().FirstOrDefault();
        }
        else
        {
            parser = _answerParsers.OfType<TextAnswerParser>().FirstOrDefault();
        }

        if (parser is null)
        {
            throw new Exception("Не найден парсер для ответов");
        }

        return parser;
    }
}