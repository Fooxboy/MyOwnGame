
# Документация для методов

> [!tip] *Описание всех классов подробно - в конце документа (ctrl+F и все - нашел)*

----

# SignalR Методы

----
### `ConnectToSession` 
Подключиться к сессии
#### **Аргументы:**
1. `long sessionId` - *ИД сессии*
2. `long userId` - *ИД юзера*

#### **Результат:** `SessionDto`

>[!info] **Примечание**
>Если пользователь уже есть в сессии и вылетел или не вызвал метод `DisconnectFromSession`, тогда он подключиться к этой же сессии с сохранением очков. Если он попытается подключиться к другой сессии, не выйдя из предыдущей - получит ошибку

-----
### `DisconnectFromSession` 
Отключиться от сессии
#### **Аргументы:** 

#### **Результат:** `null`

>[!info] **Примечание**
>Если пользователь вызвал этот метод, он больше не сможет вернуться в сессию. Возможно, он сможет снова подключиться к ней, но все очки обнулятся. Возможно, и не сможет - это требует проверки.

----
### `ChangeRound` 
Изменить раунд игры
#### **Аргументы:** 
1. `int roundNumber` - *номер раунда (начинается с 0)*

#### **Результат:** `Ничего`

----
### `SelectQuestion `
Выбрать вопрос в таблице
#### Аргументы:
1. `int themeNumber` - *номер темы (начинается с 0)*
2. `int priceNumber` - *номер вопроса (начинается с 0)*

#### Результат: `null`

> [!info] Примечание
> 1. По сути вопросы и темы это матрица формата (x,y), где x это индекс темы, y это индекс вопроса. Их сюда и передаем
> 2. Этот метод может вызвать только тот, кто может выбирать вопрос. Тот кто может выбирать вопрос, приходит в эвенте `ChangeSelectQuestionPlayer`


----
### `ReadyToAnswer` 
Метод, который будет вызываться при нажатии кнопки "ответ". Он регистрирует то, что пользователь хочет ответить 
#### Аргументы:
1. `DateTime time` - *локальное вермя нажатия (в UTC!!!!!)*

#### Результат: `null`

>[!info] Примечание
>Сервер в течении 500мс собирает  все нажатия и выбирает самые ранние
Может выкинуть ошибку, если пользотваель опоздал или слишком рано нажал

----
### `AcceptAnswer` 
Принять ответ. По сути админ скажет, что вы отвечающий прав и начислит ему баллы.
#### Аргументы: 

#### Результат: `null`

>[!info] Примечание
Этот метод может вызывать только админ сессии

----
### `RejectAnswer` 
Сказать что отвечающий не прав. С него спишутся баллы
#### Аргументы: 

#### Результат: `null`

>[!info] Примечание
Этот метод может вызывать только админ сессии

----
### `SkipQuestion` 
Пропустить вопрос. Админ может нажать если никто не знает ответа прям ваще. А ещё потому что автоматически ниче нет
#### Аргументы:

#### Результат: `null`


>[!info] Примечание
Этот метод может вызывать только админ сессии

----
### `RemoveFinalTheme` 
убрать тему из финала.
#### Аргументы:
1. `int number` - *номер темы (начинается с 0)*

#### Результат: `null`

>[!info] Примечание
>Этот метод может вызывать только тот, кто может выбирать вопрос, риходит в эвенте `ChangeSelectQuestionPlayer`

----
### `Pause` 
Пауза игры, а че не очевидно чтоли???
#### Аргументы: 

#### Результат: `null`

> [!info] Примечание
Этот метод может вызвать только админ. Всем игрокам приходит событие `GamePaused`

----
### `Resume` 
Продолжить игру после паузы
#### Аргументы: 

#### Результат: `null`

> [!info] Примечание
Этот метод может вызывать только админ. Всем игрокам приходит событие `GameResumed`
Выкенет ошибку, если вызвать метод, не вызвав `Pause`

----
### `SetScore`
Установить значение очков пользователю
#### Аргументы:
1. `int playerId` - *ID пользотваеля, которому нужно изменить очки*
2. `int score` - *Количество очков, которое нужно установить пользователю*

#### Результат: `null`

> [!info] Примечание
Этот метод может вызывать только админ. Всем игрокам приходит событие `ScoreChanged`

----
### `SetAdmin`
Снять с себя должность админа и дать его другому 😁
#### Аргументы:
1. `int playerId` - *ID пользотваеля, которому передать админство*

#### Результат: `null`

> [!info] Примечание
Этот метод может вызывать только админ. Всем игрокам приходит событие `AdminChanged`

----
### `ChangeSelectQuestionPlayer`
Изменить игрока, который может выбирать вопрос
#### Аргументы:
1. `int playerId` - *ID пользотваеля, который может выбирать вопрос*

#### Результат: `null`

> [!info] Примечание
Этот метод может вызывать только админ. Всем игрокам приходит событие `ChangeSelectQuestionPlayer`


----
### `SetFinalPrice`
Указать цену игры в финале
#### Аргументы:
1`int score` - *Количество очков поставленных на игру*

#### Результат: `null`

> [!info] Примечание
Невозможно указать меньше 1 или больше чем очков у игрока

----

----
### `SendFinalAnswer`
Отправить ответ на финал
#### Аргументы:
1. `string text` - *Ответ на вопрос*
2. `int score` - *НИЧЕГО НЕ ДЕЛАЕТ, ОСТАВЛЕНО ДЛЯ ОБРАТНОЙ СОВМЕСТИМОСТИ*

#### Результат: `null`

> [!info] Примечание
Всем придет эвент о том что чел ответил

----

----
### `ShowFinalAnswer`
Показать финальный ответ игрока всем пользователям и админу тоже
#### Аргументы:
1. `int playerId` - *ID пользотваеля, которого показать ответ*

#### Результат: `null`

> [!info] Примечание
Этот метод может вызывать только админ

----
### `SetQuestionPrice`
Установить цену для вопроса, если пришел евент, который это требует
#### Аргументы:
1. `int price` - *Цена. Не может быть меньше или больше переданного в эвенте*

#### Результат: `null`

----
### `ForwardQuestion`
Передать вопрос другому игроку, если это требуется
#### Аргументы:
1. `int playerId` - *Id пользователя, которому передан вопрос*

#### Результат: `null`

# SignalR События

>[!info] Примечание
> **Арументы** - это то что приходит вместе с эвентом.
>Указывается только тип данных, который приходит.

----
### `PlayerConnectedToSession` 
Игрок подключился к сессии

#### Аргументы
1. `PlayerDto`  - *Описание игрока, который зашел в сессию*

> [!info] Примечание
>1. Если пользователь, который присоеденился уже есть у вас в юае, просто обновите его Score пожалуйста из более актуальной инфы (та которая приходит)
>2. Если пользотвателя нет - ну добавь его на юай ебать

----
### `PlayerDisconnectedFromSession` 
Игрок отключился от сессии (именно ручками)

#### Аргументы:
1. `PlayerDto` - *Описание игрока, который отключился*

-----
### `PlayerOffline`
Игрок отключился от сессии из-за сетевой ошибки

#### Аргументы:
1. `PlayerDto` - *Описание игрока, который отключился*

-----
### `AdminChanged`
Изменился админ в сессии

#### Аргументы:
1. `PlayerDto` - *Описание игрока, который стал админом*

-----
### `RoundChanged` 
Изменен раунд игры. Может быть админ просто скипнул раунд или это начало игры. 

#### Аргументы:
1. `RoundInfo` - *краткая информация о раунде (можно показывать перед началом или хуй знает че ты хочешь с этим делать мне похуй)*

----
### `GamePaused` 
Игра на паузе

#### Аргументы:
1. `SessionDto` - *инфа о сесии*

> [!info] Примечание
Надо запрещать юзверам все что угодно делать ебать

----
### `GameResumed` 
Игра снята с паузы

#### Аргументы:
1. `SessionDto` - *инфа о сесии*

----
### `QuestionSelected` 
Игрок выбрал вопрос

#### Аргументы:
1. `Question[]` - *вопросы (вопросы могут быть сразу из нескольких штук. Текст. Медиа и текст. Или че там ещё шизоид создатель пака придумает. Показывай их по очереди с каким нибудь таймаутом)*
2. `QuestionPackInfo?` - *инфа о выбранном вопросе (тип вопроса, цена его, тема, если это какой-то особенный вопрос, напрмиер кот  в мешке)*
3. `int` - *время в секундах, через сколько можно отвечать. Нужно наверное для анимации и все такое. Время на все вопросы. Чем больше вопросов, тем больше времени*
4. `QuestionSelectedPosition` - *позиция вопроса (чтобы подсечивать другим юзерам, че там выбрал игрок ебаный)*

>[!info] Примечание
Надо всем показать сначала какой вопрос выбрал игрок на сетке, а потом показать вопрос лол.

----
### `QuestionSelectedAdmin` 
Игрок выбрал вопрос. Событие только для админа. Ему приходит ответ.

#### Аргументы:
1. `Answer` - *Ответ*

>[!info] Примечание:
Это хуета приходит только админу. Можешь конечно подписаться для всех пользователей для простоты. Кому не надо не придет,не переживай, бля зуб даю

----
### `PlayerAnswer` 
игрок отвечает (не попытался ответить. А именно этот игрок отвечает. Нажал и его сервеер выбрал для ответа!!!!!)

#### Агрументы:
1. `PlayerDto` - *объект игрока, который отвечает, лол.*

>[!info] Примечание:
Ну типа выделять пользователя как то, который отвечает. 

----
### `PlayerTriedAnswer` 
игрок попытался ответить, но он или поздно нажал или рано ( можно как в оригинале подсечать иконку лол)

#### Аргументы:
1. `PlayerDto` - *объект игрока, который попытался ответить но облажался*

>[!info] Примечание:
Как то показать что этот игрок попытался ответить, но обосрался лол

----
### `AcceptAnswer` 
Админ сказал, что ответ отвечающего игрока верный

#### Аргументы:
1. `PlayerDto` - *игрок, который отвечал*
2. `int` - *Новое значение количество очков игрока*
3. `Answer` - *Ответ. Показываем этот ответ, типа то что он реально прав ебать!!!*

> [!info] Примечание
Ну тут все понятно ебать и без слов. Там ещё и баллы прибавляются. Может ебануть какую нибудь анимацию типа +N нахуй!

----
### `RejectAnswer` 
Админ сказал, что ответ отвечающего нихуя не верный, лох, ахахаххахах. 

#### Аргументы:
1. `PlayerDto` - *игрок, который отвечал*
2. `int` - *Новое значение количество очков игрока*

> [!info] Примечание
Ну тут идеи точно такие же как и в ацепте

----
### `SkipQuestion` 
Пропуск вопроса, сразу показывается ответ всем игрокам

#### Аргументы:
1. `Answer` - *ответ на пропущенный вопрос, кек*

> [!info] Примечание
Показываем ответ сначала, а потом показываем сетку из вопросов без этого ответа лол

----
### `ChangeSelectQuestionPlayer` 
Говорит о том, какой игрок щас должен выбирать ответ ебать

#### Аргументы:
1. `PlayerDto` - *игрок который должен выбирать вопрос*

>[!info] Примечание:
Выделять как то юзвера который должен выбирать ответ, чтобы знать, кого пиздить если долго не выбирает. Другим блокировать выбор на сетке

----
### `ScoreChanged`
У пользователя админ ручками изменил значение очков

#### Аргументы:
1. `PlayerDto` - *игрок у которого изменили очки*
2. `int` - *новое значение очков у игрока*

>[!info] Примечание:
Выделять как то юзвера который должен выбирать ответ, чтобы знать, кого пиздить если долго не выбирает. Другим блокировать выбор на сетке

----
### `FinalThemeRemoved`
Удалена финальная тема из финала

#### Аргументы:
1. `RoundTheme[]` - *Новый массив финальных тем*

>[!info] Примечание:
Индексы тоже изменились. Если ты убрал например 0 тему, то предудщая 1 стала 0 и т.д.

----
### `FinalQuestionResponsed`
Пользователь ответил в финале

#### Аргументы:
1. `PlayerDto` - *Игрок, который ответил на финальную тему*


----
### `UserFinalAnswer`
Показан ответ пользователя в финале

#### Аргументы:
1. `PlayerDto` - *Игрок, который ответил на финальную тему*

----
### `PlayerCanAnswer`
Эвент, который говорит, что можно нажать на кнопку ответа

#### Аргументы:

`нет`

----
### `NeedSetQuestionPrice`
Эвент, который присылается конкретному юзеру и говорит, что необхоидмо задать цену вопроса в определенном диапазоне

#### Аргументы:

1. `SelectPriceWithStep` - объект описывающий цену

----
### `PlayerInstallingQuestionPrice`
Эвент, который прилетает всем и говорит, что конкретный игрок выбирает цену вопроса 

#### Аргументы:

1. `PlayerDto` - игрок, который выбирает цену вопроса


----
### `QuestionPriceInstalled`
Пользователь указал цену вопроса

#### Аргументы:

1. `PlayerDto` - игрок, который выбрал цену вопроса
2. `int?` - цена, которую установил пользователь (может быть null, если она должна быть скрыта)

----
### `NeedForwardQuestion`
Нужно выбрать игрока, которому передать вопрос

#### Аргументы:

ничего

----
### `PlayerChoosesQuestionPlayer`
Игрок выбирает кому передать вопрос

#### Аргументы:

ничего

----
### `QuestionForwarded`
Был передан вопрос этому игроку

#### Аргументы:

1. `PlayerDto` - игрок, котому передали вопрос

----

### `NeedSetFinalPrice`
Необходимо выбрать цену игры в финале

#### Аргументы:

ничего

----
### `SessionClosed`
Эта сессия завершилась. Надо отключиться от хаба

#### Аргументы:

ничего


----
# Объекты

>[!info] Примечание
>А ВОТ ПОЛНОЕ ОПИСАНИЕ ВСЕХ КЛАССОВ МОДЕЛЕЙ ТИПОВ ХУИПОВ
>ЕСЛИ В КОННЦЕ НАЗВАИВАНИИ ТИПА СТОИТ "`?`" ЭТО ЗНАЧИТ ЧТО ОН **МОЖЕТ БЫТЬ NULL** !!!!!!!

----
### `DateTime` 

ЭТО СТАНДАРТНЫЙ ТИП ДАТЫ И ВРЕМЕНИ В СИШАРПЕ ПЕРЕДАВАЙ НОРМАЛЬНОЕ ДАТУ И ВРЕМЯ И ВСЕ БУДЕТ НОРМАЛЬНО

**ПОГУГЛИ ЕСЛИ НЕ РАБОТАЕТ**

---
### `SessionDto`  
Информация о сессии
#### Поля
1. `List<PlayerDto> Players`  - *Игроки в этой сесии*
2. `DateTime CreatedAt`  - *Дата создания сессии*
3. `RoundInfo? CurrentRound` - *Текущий раунд*
4. `GameInfo GameInfo` - *Краткая информация  об паке*
5. `PlayerDto? RespondingPlayer`  - *Игрок, который щас отвечает*
6. `PlayerDto? SelectQuestionPlayer`  - *Игрок, который щас выбирает вопрос* 
7. `SessionState State`  - *Состояние игры*

----
### `RoundInfo`
Краткая инфа о раунде
#### Поля
 1. `string Name` - *название раунда*
 2. `bool IsFinal` - *признак того что это финальный раунд (ну вот так вот сделал как разраб, не осуждай меня блять)*
 3. `List<RoundTheme> Themes`  - *список тем в раунде*
 4. `int Number`  - *номер раунда (вроде с 0), но хуй знает что это*

----
### `RoundTheme`
информация о теме в раунде ебать
#### Поля
 1. `string Name `- *название темы*
 2.  `List<PriceInfo> Prices `- *Массив цен вопросов в этой теме*

----
### `PriceInfo`
Информация о вопросе и его цене
#### Поля
1. `bool IsAnswered` - *Был ли отвечен этот вопрос*
2. `int Price` - *Ценник вопроса ебать.*

---
### `GameInfo`
Краткая инфа об паке
#### Поля
1.  `string PackageName` - *Имя пака*
2.  `List<string> Themes`  - *Все темы в паке в виде массива*
3.  `DateTime PackageCreatedAt`  - *Время создания пака (СПАРШЕНО ИЗ ХМЛ)* 

----
### `PlayerDto`
Инфа о игрокке
#### Поля
1. `long Id`  - *его айдишник*
2. ` string Name`  - *имя пользователя*
3.  `string AvatarImage` - *ссылка на аватар (не забывай что все аватарки живут по пути `/avatars/имя.лох`*
4. `bool IsAdmin`  - *Флаг, который говорит, что этот игрок в этой сессии админ*
5. `int Score` - *Его игровые очки*
6. `string ConnectionId` - *его коннекшен  id (тебе это не надо а мне надо, а удалять лень)*
7. `long SessionId`  - *его ид подключения (тебе будет только текщая сессия всегда, но у меня нет, и так же лень удалять, и не факт что это работает забудь просто про это поле и не еби мозги)*
8. `bool IsDisconnected` - *Находится ли пользователь офлайн (из-за того что у него умер интернет, например)*

----
### `Question`
Вопрос
#### Поля
1. `string? Url`  - *Юрл на медиа вопрос (вернется, только если это медиа вопрос)*
2. `string Text`  - *Текст вопроса (вернется, только если это текстовый вопрос)*
3. `QuestionContentType Type` - *Тип вопроса*

----
### `QuestionSelectedPosition`
Позиция выбранного вопроса
#### Поля
1. `int ThemeNumber` - *индекс темы ( начинается с нуля (какая неожиданность))*
2. `int QuestionNumber` - *индекс вопроса (ну ты понял)*

----
### `Answer`
Ответ
#### Поля
1. `QuestionContentType Type` - *тип ответа*
2. `string Text` - *текстовый ответ*
3. `string Url`  - *ссылка на медиа ответ (может быть пустым, если это не медиа ответ, лол)*
4. `string? Wrong`  - *текстовый плохой ответ (што???) ну типа неправильный ответ*

----
#### `QuestionContentType`
 тип вопроса или ответа (ЭТО ЕНАМ, ТЕБЕ ПРИДЕТСЯ ТОЛЬКО INT)

#### Значения

0. `Unknown` = 0, - неизвестный
1. `Text` = 1,
2. `Audio` = 2,
3. `Image` = 3,
4. `Video` = 4,
5. `Say` = 5

----

### `QuestionPackInfo`
Информация о выбранном вопросе (если это особенный вопрос)
#### Поля
1. `QuestionPackType Type` - *тип выбранного вопроса (это енам, тебе приедет инт)*
2. `CatInfo? CatInfo` - *Инфа о коте в мешке (если это кот в мешке)*
3. `SuperCatInfo?` SuperCatInfo - *инфа о супер коте в мешке (если это супер кот в мешке)*

----

#### `QuestionPackType`
Это тип вопроса (кот в мешке или нет или хз)

#### Значения

0. `Simple` = 0, - простой вопрос
1. `Cat` = 1 - кот в мешке,
2. `SuperCat` = 2 - супер кот в мешке,
3. `FreeQuestion` = 3 - вопрос без риска,
4. `Auction` = 4 - аукцион,
5. `Other` = 5 - другой (я не знаю что это из 1000 паков, ни разу не появлялся, но документации такое есть)
6. `Final` = 6 - Финальный вопрос

----


### `CatInfo`
Инфа о коте в мешке
#### Поля
1. `string? Theme` - *Тема кота в мешке*
2. `CatInfo? CatInfo` - *Инфа о коте в мешке (если это кот в мешке)*

----