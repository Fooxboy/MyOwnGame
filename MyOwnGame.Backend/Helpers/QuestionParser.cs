using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.Questions;
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
            Questions = question.Scenario.Atom.Count == 1 
                ? new List<QuestionBase>() {questionParser.ParseQuestion(question)} 
                : questionParser.ParseQuestions(question),
            Price = question.Price
        };

        return questionInfo;
    }

    private IQuestionParser GetQuestionParser(Question question)
    {
        var atoms = question.Scenario.Atom;
        
        IQuestionParser? parser = null;

        if (atoms.Count == 1)
        {
            var atom = atoms.FirstOrDefault();
            
            if (atom.Type is null)
            {
                parser = _questionParsers.OfType<TextQuestionParser>().FirstOrDefault();
            }
            else
            {
                parser = _questionParsers.OfType<MediaQuestionParser>().FirstOrDefault();
            }
        }
        else
        {
            parser = _questionParsers.OfType<MultipleQuestionParser>().FirstOrDefault();
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
        
        if (CheckIsMediaAnswer(question))
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

    private bool CheckIsMediaAnswer(Question question)
    {
        var one = question.Scenario.Atom.Count > 1 && question.Scenario.Atom.Exists(a => a.Type == "marker")
                                                   && question.Scenario.Atom.LastOrDefault().Type != "marker";

        var marker = question.Scenario.Atom.FirstOrDefault(x => x.Type == "marker");

        if (marker is null)
        {
            return false;
        }

        var markerIndex = question.Scenario.Atom.IndexOf(marker);

        var lastMediaContent = question.Scenario.Atom.LastOrDefault(x => x.Type == "image" || x.Type == "video" || x.Type == "voice");

        var lastMediaContentIndex = question.Scenario.Atom.IndexOf(lastMediaContent);

        return one && lastMediaContentIndex > markerIndex;
    }
}