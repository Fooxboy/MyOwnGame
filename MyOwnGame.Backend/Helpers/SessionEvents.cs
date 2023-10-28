namespace MyOwnGame.Backend.Helpers;

public enum SessionEvents
{
    /// <summary>
    /// Пользователь подключился к сессии
    /// </summary>
    PlayerConnectedToSession,
    
    /// <summary>
    /// Пользователь отключился от сессиииииииии
    /// </summary>
    PlayerDisconnectedFromSession,
    
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
    
    /// <summary>
    /// Пользователь отключился из за сетевой ошибки
    /// </summary>
    PlayerOffline,
    
    /// <summary>
    /// Администратор сменился
    /// </summary>
    AdminChanged,
    
    /// <summary>
    /// Изменилось количество очков у игрока
    /// </summary>
    ScoreChanged,
    
    /// <summary>
    /// Удалена финальная тема
    /// </summary>
    FinalThemeRemoved,
    
    /// <summary>
    /// Отвечена финальная тема юзером
    /// </summary>
    FinalQuestionResponsed,
    
    /// <summary>
    /// Финальный ответ пользователя
    /// </summary>
    UserFinalAnswer,
    
    /// <summary>
    /// Пользователь может нажать кнопку "Ответить"
    /// </summary>
    PlayerCanAnswer,
    
    /// <summary>
    /// Нужно установить цену
    /// </summary>
    NeedSetQuestionPrice,
    
    //Цена вопроса установлена :)
    QuestionPriceInstalled,
    
    /// <summary>
    /// Пользователь устанавливает цену вопроса
    /// </summary>
    PlayerInstallingQuestionPrice,
}