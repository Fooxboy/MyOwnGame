using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.Questions;
using MyOwnGame.Backend.Models.SiqPackage;

namespace MyOwnGame.Backend.Parsers.QuestionInfo.QuestionParsers;

public class MultipleQuestionParser : IQuestionParser
{
    public QuestionBase ParseQuestion(Question question)
    {
        throw new NotImplementedException();
    }

    public List<QuestionBase> ParseQuestions(Question question)
    {
        var questions = new List<QuestionBase>();

        foreach (var atom in question.Scenario.Atom.Where(atom => !string.IsNullOrEmpty(atom.Text) || atom.Type == "marker"))
        {
            switch (atom.Type)
            {
                case "marker":
                    return questions;
                case "say":
                    questions.Add(new TextQuestion(){ Text = atom.Text, Type = QuestionContentType.Text });
                
                    continue;
                default:
                    questions.Add(new MediaQuestion()
                    {
                        Type = StringToQuestionContentTypeMapper.Map(atom.Type), 
                        Url = atom.Text.Replace("@", string.Empty)
                    });
                    break;
            }
        }

        return questions;
    }
}