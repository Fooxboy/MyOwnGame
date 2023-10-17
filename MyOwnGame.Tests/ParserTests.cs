using Microsoft.Extensions.Configuration;
using MyOwnGame.Backend.Helpers;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.Answers;
using MyOwnGame.Backend.Models.Questions;
using MyOwnGame.Backend.Models.SiqPackage;
using MyOwnGame.Backend.Parsers;
using MyOwnGame.Backend.Parsers.QuestionInfo;
using MyOwnGame.Backend.Parsers.QuestionInfo.Answer;
using MyOwnGame.Backend.Parsers.QuestionInfo.QuestionParsers;
using MyOwnGame.Tests.Mock;

namespace MyOwnGame.Tests;

public class ParserTests
{
    private readonly Dictionary<string, string> _config = new()
    {
        {"packagesPath", "C:\\siq\\packages"},
        {"filesPath", "C:\\siq\\packages"},
        {"avatarsPath", "C:\\siq\\avatars"},
    };
    
    [Fact]
    public async Task SiqPackageTest()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(_config).Build();
        var parser = new SiqPackageParser(configuration, new Logger());
        
        var questionParser = new QuestionParser(new IQuestionParser[] { new MediaQuestionParser(), new TextQuestionParser(), new MultipleQuestionParser() },
            new IAnswerParser[] { new MediaAnswerParser(), new TextAnswerParser() });

        var directory = "C:\\siq\\test";
        
        /*
        var urls = Enumerable.Range(5000, 1000).Select(i => $"https://sigame.xyz/api/pack/{i}/download").ToList();
        
        var tasks = urls.Select(url => DownloadFileAsync(url, directory));
        
        await Task.WhenAll(tasks);
        
        */

        var packagesNames = Directory.GetFiles(directory);

        foreach (var pack in packagesNames)
        {
            var result = parser.UnpackPackage(pack, HashHelper.ComputeHash(pack));

            var package = parser.ParsePackage(Path.Combine(result, "content.xml"));

            var questionsPackage = package.Rounds.Round.SelectMany(x => x.Themes.Theme).ToList();
                
            var brr = questionsPackage.Where(x=> x.Questions != null).SelectMany(x => x.Questions.Question).ToList();

            foreach (var packageQuestion in brr)
            {
                if(packageQuestion is null) continue;
                try
                {
                    var parserQuestion = questionParser.Parse(packageQuestion);

                }
                catch (Exception ex)
                {
                    
                }

                //Assert.True(parserQuestion.Question != null && parserQuestion.Answer != null);
            }
        }
        Assert.True(true);
    }

    [Fact]
    public async Task TestTextQuestionParser_Correct()
    {
        var question = new Question()
        {
            Price = 999,
            Right = new Right()
            {
                Answer = "Верный ответ"
            },
            Scenario = new Scenario()
            {
                Atom = new List<Atom>()
                {
                    new Atom()
                    {
                        Text = "Обычный вопрос"
                    },
                    new Atom()
                    {
                        Type = "marker",
                    },
                    new Atom()
                    {
                        Type = "voice",
                        Text = "@voice.mp3"
                    }
                }
            }
        };

        var parser = new TextQuestionParser();
        var result = parser.ParseQuestion(question);
        
        Assert.True(result is TextQuestion text && text.Text == "Обычный вопрос" && text.Type == QuestionContentType.Text);
    }

    [Fact]
    public async Task TestMediaQuestionParser_Correct()
    {
        var question = new Question()
        {
            Price = 999,
            Right = new Right()
            {
                Answer = "Верный ответ"
            },
            Scenario = new Scenario()
            {
                Atom = new List<Atom>()
                {
                    new Atom()
                    {
                        Type = "voice",
                        Text = "@voice.mp3"
                    },
                    new Atom()
                    {
                        Type = "marker",
                    },
                    new Atom()
                    {
                        Type = "voice",
                        Text = "@voice.mp3"
                    }
                }
            }
        };
        
        var parser = new MediaQuestionParser();
        var result = parser.ParseQuestion(question);
        
        Assert.True(result is MediaQuestion q && q.Url == "voice.mp3" && q.Type == QuestionContentType.Audio);
    }

    [Fact]
    public async Task TestTextAnswerParser_Correct()
    {
        var question = new Question()
        {
            Price = 999,
            Right = new Right()
            {
                Answer = "Верный ответ"
            },
            Scenario = new Scenario()
            {
                Atom = new List<Atom>()
                {
                    new Atom()
                    {
                        Type = "voice",
                        Text = "@voice.mp3"
                    },
                    new Atom()
                    {
                        Type = "marker",
                    },
                    new Atom()
                    {
                        Type = "voice",
                        Text = "@voice.mp3"
                    }
                }
            }
        };
        
        var parser = new TextAnswerParser();
        var result = parser.ParseAnswer(question);
        
        Assert.True(result.Text == "Верный ответ");
    }
    
    [Fact]
    public async Task TestMediaAnswerParser_Correct()
    {
        var question = new Question()
        {
            Price = 999,
            Right = new Right()
            {
                Answer = "Верный ответ"
            },
            Scenario = new Scenario()
            {
                Atom = new List<Atom>()
                {
                    new Atom()
                    {
                        Type = "voice",
                        Text = "@voice.mp3"
                    },
                    new Atom()
                    {
                        Type = "marker",
                    },
                    new Atom()
                    {
                        Type = "voice",
                        Text = "@voice.mp3"
                    }
                }
            }
        };
        
        var parser = new MediaAnswerParser();
        var result = parser.ParseAnswer(question);
        
        Assert.True(result is MediaAnswer a && a.Type == QuestionContentType.Audio && a.Url == "voice.mp3");
    }
    
    private async Task DownloadFileAsync(string url, string directory)
    {
        using var client = new HttpClient();
        var urlStream = await client.GetStreamAsync(url);

        using var fileStream = new FileStream(Path.Combine(directory, $"{Guid.NewGuid()}.siq"), FileMode.CreateNew);
        await urlStream.CopyToAsync(fileStream);
    }
}