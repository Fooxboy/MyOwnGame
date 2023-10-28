using System.Text.Json.Serialization;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.SiqPackage;

namespace MyOwnGame.Backend.Domain;

public class Session
{
    public List<Player> Players { get; } = new();
    
    public DateTime CreatedAt { get; }
    
    [JsonIgnore]
    public Package Package { get; }

    [JsonIgnore]
    public bool AdminCanConnect => !Players.Any();
    
    public RoundInfo? CurrentRound { get; private set; }
    
    public GameInfo GameInfo { get; }
    
    /// <summary>
    /// Отвечайющий игрок
    /// </summary>
    public Player? RespondingPlayer { get; private set; }
    
    /// <summary>
    /// Игрок который выбирает вопрос
    /// </summary>
    public Player? SelectQuestionPlayer { get; private set; }
    
    public SessionState State { get; private set; }
    
    [JsonIgnore]
    public QuestionInfo? CurrentQuestion { get; private set; }

    [JsonIgnore]
    private readonly List<(Player, DateTime)> _playersReadyToAnswer = new();

    [JsonIgnore] 
    public DateTime? ReadyToAnswerTime { get; private set; }
    
    [JsonIgnore]
    public string PackageHash { get; private set; }
    
    public List<FinalAnswer> FinalAnswers { get; private set; }
    
    public SessionSettings Settings { get; private set; }
    
    public List<AuctionPrice>? AuctionPrices { get; private set; }
    
    public Session(Package package, SessionSettings? settings = null)
    {
        CreatedAt = DateTime.UtcNow;
        
        Package = package;

        State = SessionState.Created;

        var themesName = (package.Rounds.Round.SelectMany(round => round.Themes.Theme, (round, theme) => theme.Name)).ToList();

        GameInfo = new GameInfo
        {
            PackageName = package.Name, 
            PackageCreatedAt = DateTime.Parse(package.Date),
            Themes = themesName,
            Rounds = package.Rounds.Round.Select(x=> new ShortRoundInfo() {IsFinal = x.Type == "final", Name = x.Name}).ToList()
        };

        FinalAnswers = new List<FinalAnswer>();

        Settings = settings ?? new SessionSettings() { TimeToReadyAnswer = 5 };
    }

    public void AddAuctionPrice(Player player, int score)
    {
        AuctionPrices ??= new List<AuctionPrice>();
        
        AuctionPrices.Add(new AuctionPrice(){ Player = player, Price = score});
    }

    public void ResetAuctionPrice()
    {
        AuctionPrices = null;
    }
    
    public void SetSelectQuestionPlayer(Player player)
    {
        SelectQuestionPlayer = player;
    }

    public void ResetSelectQuestionPlayer()
    {
        SelectQuestionPlayer = null;
    }

    public void SetPackageHash(string hash)
    {
        PackageHash = hash;
    }

    public void ChangeRound(int roundNumber)
    {
        var siqRound = Package.Rounds.Round[roundNumber];

        var round = RoundInfo.Parse(siqRound);
        round.Number = roundNumber;

        CurrentRound = round;
        
        ResetRespondingPlayer();
    }

    public void AddPlayer(Player player)
    {
        Players.Add(player);
    }

    public void ChangeRespondingPlayer(Player player)
    {
        if (player is null)
        {
            throw new Exception("Не передали плеера лол");
        }

        if (!Players.Exists(p => p.ConnectionId == player.ConnectionId))
        {
            throw new Exception("Переданный отвечающий игрок не находится в этой сессии");
        }

        RespondingPlayer = player;
    }

    public void ResetRespondingPlayer()
    {
        RespondingPlayer = null;
    }

    public int ChangeStateToQuestion(int countQuestions)
    {
        State = SessionState.Question;
        var seconds = Settings.TimeToReadyAnswer * countQuestions;
        
        ReadyToAnswerTime = DateTime.UtcNow + TimeSpan.FromSeconds(seconds);

        return seconds;
    }

    public void ChangeStateToAnswer()
    {
        State = SessionState.Answer;

        ReadyToAnswerTime = null;
    }

    public void ChangeStateToTable()
    {
        State = SessionState.Table;

        ResetRespondingPlayer();
    }

    public void AddReadyToAnswerPlayer(Player player, DateTime time)
    {
        _playersReadyToAnswer.Add((player, time));
    }

    public void SelectRespondingPlayer()
    {
        var player = _playersReadyToAnswer.OrderBy(x => x.Item2).FirstOrDefault().Item1;

        RespondingPlayer = player;
    }

    public void SelectCurrentQuestion(QuestionInfo questionInfo)
    {
        CurrentQuestion = questionInfo;
    }

    public void ResetCurrentQuestion()
    {
        CurrentQuestion = null;
    }

    public void RemovePlayer(string playerConnectionId)
    {
        var removingPlayer = Players.FirstOrDefault(x => x.ConnectionId == playerConnectionId);
        
        if (removingPlayer is null)
        {
            throw new Exception("Не найден игрок для удаления из сессии или его нет уже в сесиии");
        }
        
        this.Players.Remove(removingPlayer);
    }

    public void RemoveTheme(int position)
    {
        if (CurrentRound is null)
        {
            throw new Exception("Не получилось удалить финальный раунд, потому что нет выбранного раунда в сессии");
        }

        if (!CurrentRound.IsFinal)
        {
            throw new Exception("Нельзя убрать тему не у финальной темы");
        }
        
        CurrentRound.Themes.Remove(CurrentRound.Themes[position]);
    }

    public void AddFinalAnswer(Player player, string answer, int price)
    {
        FinalAnswers.Add(new FinalAnswer(){ Player = player, Answer = answer, Price = price});
    }
}