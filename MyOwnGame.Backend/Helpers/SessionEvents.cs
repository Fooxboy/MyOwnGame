namespace MyOwnGame.Backend.Helpers;

public enum SessionEvents
{
    /// <summary>
    /// Пользователь подключился к сессии
    /// </summary>
    PlayerConnectedToSession,
    
    /// <summary>
    /// Раунд изменен
    /// </summary>
    RoundChanged,
    
    /// <summary>
    /// Игра на паузе
    /// </summary>
    GamePaused,
    
    /// <summary>
    /// Игра снята с паузы
    /// </summary>
    GameResumed,
    
    /// <summary>
    /// Выбран вопрос
    /// </summary>
    QuestionSelected,
    
    /// <summary>
    /// Выбран вопрос (админу прилетает ответ)
    /// </summary>
    QuestionSelectedAdmin,
    
    /// <summary>
    /// Игрок отвечает
    /// </summary>
    PlayerAnswer,
    
    /// <summary>
    /// Игрок попытался ответить
    /// </summary>
    PlayerTryedAnswer,
    
    /// <summary>
    /// Игрок дал верный ответ
    /// </summary>
    AcceptAnswer,
    
    /// <summary>
    /// Игрок дал не верный ответ
    /// </summary>
    RejectAnswer,
    
    /// <summary>
    /// Вопрос пропустили (сразу дается ответ)
    /// </summary>
    SkipQuestion,
    
    /// <summary>
    /// Изменен игрок, который выбирает вопрос
    /// </summary>
    ChangeSelectQuestionPlayer,
}