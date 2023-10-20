// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.SignalR.Client;
using MyOwnGame.Backend.Domain;
using MyOwnGame.Backend.Models;
using MyOwnGame.Backend.Models.Dtos;
using MyOwnGame.Backend.Models.Questions;
using Newtonsoft.Json.Linq;

Console.WriteLine("Hello, World!");

const string TARGET_REST_URL = "http://localhost:3000/";

const string TARGET_HUB_URL = "http://localhost:3000/hubs/session";

//Загружаем siq пакет


using var httpClient = new HttpClient();
using var content = new MultipartFormDataContent();
var fileBytes = File.ReadAllBytes("C:\\Users\\Fooxb\\Desktop\\siq\\123.siq");
        
content.Add(new ByteArrayContent(fileBytes, 0, fileBytes.Length), "package", "123.siq");
var response = await httpClient.PostAsync(TARGET_REST_URL + "sessions/createSession?number=1234", content);
        
response.EnsureSuccessStatusCode();

var jsonSession = await response.Content.ReadAsStringAsync();

Console.WriteLine($"Создана сессия, а вот инфа: {jsonSession}");

var sessionId = JObject.Parse(jsonSession)["sessionId"].ToObject<long>();

var adminUserId = 0;
var playerUserId = 0;
//Регаем аккаунт лол
{
    using var contentImage = new MultipartFormDataContent();
    var fileImageBytes = File.ReadAllBytes("D:\\Pictures\\3.png");

    contentImage.Add(new ByteArrayContent(fileImageBytes, 0, fileImageBytes.Length), "image", "3.png");

    var registerResponse = await httpClient.PostAsync(TARGET_REST_URL + "users/create?name=Славк", contentImage);

    registerResponse.EnsureSuccessStatusCode();

    var jsonRegister = await registerResponse.Content.ReadAsStringAsync();
    Console.WriteLine($"Создан пользователь, а вот инфа: {jsonRegister}");
    adminUserId = JObject.Parse(jsonRegister)["id"].ToObject<int>();
}

//Регаем аккаунт лол
{
    using var contentImage = new MultipartFormDataContent();
    var fileImageBytes = File.ReadAllBytes("D:\\Pictures\\3.png");

    contentImage.Add(new ByteArrayContent(fileImageBytes, 0, fileImageBytes.Length), "image", "3.png");

    var registerResponse = await httpClient.PostAsync(TARGET_REST_URL + "users/create?name=Игрок ебать", contentImage);

    registerResponse.EnsureSuccessStatusCode();

    var jsonRegister = await registerResponse.Content.ReadAsStringAsync();
    Console.WriteLine($"Создан пользователь, а вот инфа: {jsonRegister}");
    playerUserId = JObject.Parse(jsonRegister)["id"].ToObject<int>();
}

//подключаемся к хабу

var adminConnection = new HubConnectionBuilder()
    .WithUrl(TARGET_HUB_URL)
    .Build();


adminConnection.ServerTimeout = TimeSpan.FromDays(1);
//подписываемся на события
adminConnection.On<PlayerDto>("PlayerConnectedToSession", (player) =>
{
    Console.WriteLine($"[admin] PlayerConnectedToSession: {player.ConnectionId} | {player.SessionId} | {player.Score} | {player.Id}");
});

adminConnection.On<RoundInfo>("RoundChanged", (roundInfo) =>
{
    Console.WriteLine($"[admin] RoundChanged: {roundInfo.Name}");
});

adminConnection.On<SessionDto>("GamePaused", (session) =>
{
    Console.WriteLine($"[admin] GamePaused: {session.State}");
});

adminConnection.On<SessionDto>("GameResumed", (session) =>
{
    Console.WriteLine($"[admin] GameResumed: {session.State}");
});

adminConnection.On<string>("QuestionSelected", (json) =>
{
    Console.WriteLine($"[admin] QuestionSelected: {json}");
});

adminConnection.On<string>("QuestionSelectedAdmin", (json) =>
{
    Console.WriteLine($"[admin] QuestionSelectedAdmin: {json}");
});

adminConnection.On<PlayerDto>("PlayerAnswer", (player) =>
{
    Console.WriteLine($"[admin] PlayerAnswer: {player} | {player.ConnectionId} | {player.Id}");
});


adminConnection.On<PlayerDto>("PlayerTryedAnswer", (player) =>
{
    Console.WriteLine($"[admin] PlayerTryedAnswer: {player.ConnectionId} | {player.Id}");
});

adminConnection.On<PlayerDto, int, string>("AcceptAnswer", (player, newScore, jsonAnswer) =>
{
    Console.WriteLine($"[admin] AcceptAnswer: {player.ConnectionId} | {player.Id} | {newScore} | {jsonAnswer}");
});

adminConnection.On<PlayerDto, int>("RejectAnswer", (player, newScore) =>
{
    Console.WriteLine($"[admin] RejectAnswer: {player.ConnectionId} | {player.Id} | {newScore}");
});

adminConnection.On<string>("SkipQuestion", (jsonAnswer) =>
{
    Console.WriteLine($"[admin] SkipQuestion: {jsonAnswer}");
});

await adminConnection.StartAsync();

//подключаемся с пользователя
var userConnection = new HubConnectionBuilder()
    .WithUrl(TARGET_HUB_URL)
    .Build();

//подписываемся на события
userConnection.On<PlayerDto>("PlayerConnectedToSession", (player) =>
{
    Console.WriteLine($"[user] PlayerConnectedToSession: {player.ConnectionId} | {player.SessionId} | {player.Score} | {player.Id}");
});

userConnection.On<RoundInfo>("RoundChanged", (roundInfo) =>
{
    Console.WriteLine($"[user] RoundChanged: {roundInfo.Name}");
});

userConnection.On<SessionDto>("GamePaused", (session) =>
{
    Console.WriteLine($"[user] GamePaused: {session.State}");
});

userConnection.On<SessionDto>("GameResumed", (session) =>
{
    Console.WriteLine($"[user] GameResumed: {session.State}");
});

userConnection.On<string>("QuestionSelected", (json) =>
{
    Console.WriteLine($"[user] QuestionSelected: {json}");
});

userConnection.On<string>("QuestionSelectedAdmin", (json) =>
{
    Console.WriteLine($"[user] QuestionSelectedAdmin: {json}");
});

userConnection.On<PlayerDto>("PlayerAnswer", (player) =>
{
    Console.WriteLine($"[user] PlayerAnswer: {player} | {player.ConnectionId} | {player.Id}");
});


userConnection.On<PlayerDto>("PlayerTryedAnswer", (player) =>
{
    Console.WriteLine($"[user] PlayerTryedAnswer: {player.ConnectionId} | {player.Id}");
});

userConnection.On<PlayerDto, int, string>("AcceptAnswer", (player, newScore, jsonAnswer) =>
{
    Console.WriteLine($"[user] AcceptAnswer: {player.ConnectionId} | {player.Id} | {newScore} | {jsonAnswer}");
});

userConnection.On<PlayerDto, int>("RejectAnswer", (player, newScore) =>
{
    Console.WriteLine($"[user] RejectAnswer: {player.ConnectionId} | {player.Id} | {newScore}");
});

userConnection.On<string>("SkipQuestion", (jsonAnswer) =>
{
    Console.WriteLine($"[user] SkipQuestion: {jsonAnswer}");
});


await userConnection.StartAsync();

userConnection.ServerTimeout = TimeSpan.FromDays(1);

//входим ккак админ
var resultAdmin = await adminConnection.InvokeAsync<SessionDto>("ConnectToSession", sessionId, adminUserId);


var resultPlayer = await userConnection.InvokeAsync<SessionDto>("ConnectToSession", sessionId, playerUserId);


await adminConnection.DisposeAsync();

//await adminConnection.InvokeAsync("DisconnectFromSession");



Console.WriteLine("лол што");


await adminConnection.InvokeAsync("ChangeRound", 0);



await userConnection.InvokeAsync("SelectQuestion", 3, 4);

//ждать!!!!!!!!!!
await userConnection.InvokeAsync("ReadyToAnswer", DateTime.UtcNow);


await adminConnection.InvokeAsync("AcceptAnswer");


while (true)
{
    Thread.Sleep(500);
    Console.WriteLine("лол што");
    Console.ReadLine();
}






