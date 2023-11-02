using MyOwnGame.Backend.BackgroundTasks;
using MyOwnGame.Backend.Helpers;
using MyOwnGame.Backend.Hubs;
using MyOwnGame.Backend.Managers;
using MyOwnGame.Backend.Parsers;
using MyOwnGame.Backend.Parsers.QuestionInfo;
using MyOwnGame.Backend.Parsers.QuestionInfo.Answer;
using MyOwnGame.Backend.Parsers.QuestionInfo.PackQuestion;
using MyOwnGame.Backend.Parsers.QuestionInfo.QuestionParsers;
using MyOwnGame.Backend.Services;
using NLog.Web;

namespace MyOwnGame.Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

#if DEBUG
            builder.WebHost.UseKestrel(serverOptions =>
            {
                serverOptions.ListenAnyIP(3000);
            });
#endif
            
#if RELEASE
            builder.WebHost.UseKestrel(serverOptions =>
            {
                serverOptions.ListenAnyIP(3000, options =>
                {
                    options.UseHttps("fooxboy.ru.pfx", "fooxboy");
                });
            });
#endif
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSignalR();
            builder.Services.AddMemoryCache();
            
            builder.Services.AddSingleton<SessionsManager>();
            
            builder.Services.AddTransient<SiqPackageParser>();
            
            builder.Services.AddTransient<UsersManager>();
            builder.Services.AddTransient<UsersService>();

            builder.Services.AddTransient<IAnswerParser, TextAnswerParser>();
            builder.Services.AddTransient<IAnswerParser, MediaAnswerParser>();
            builder.Services.AddTransient<IQuestionParser, MediaQuestionParser>();    
            builder.Services.AddTransient<IQuestionParser, TextQuestionParser>();       
            builder.Services.AddTransient<IQuestionParser, MultipleQuestionParser>();

            builder.Services.AddTransient<IPackQuestionParser, AuctionQuestionParser>();
            builder.Services.AddTransient<IPackQuestionParser, CatQuestionParser>();
            builder.Services.AddTransient<IPackQuestionParser, SuperCatQuestionParser>();
            builder.Services.AddTransient<IPackQuestionParser, FreeQuestionParser>();
            builder.Services.AddTransient<IPackQuestionParser, OtherQuestionParser>();
            builder.Services.AddTransient<IPackQuestionParser, SimpleQuestionPackParser>();

            builder.Services.AddTransient<QuestionParser>();
            
            builder.Services.AddTransient<SessionService>();
            builder.Services.AddTransient<FilesService>();           
            builder.Services.AddTransient<SessionCallbackService>();

            //Регистрация фоновых задач
            builder.Services.AddSingleton<IBackgroundTask, SessionCleaner>();
            builder.Services.AddSingleton<BackgroundTaskRunner>();

            
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("any", builder =>
                {
                    builder.SetIsOriginAllowed(origin => true)
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            //builder.Services.AddLettuceEncrypt();
            builder.Logging.ClearProviders();
            builder.Host.UseNLog();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();
            
            app.MapControllers();

            app.MapHub<SessionHub>("hubs/session");

            app.UseCors("any");

            var backgroundTaskRunner = app.Services.GetRequiredService<BackgroundTaskRunner>();

            backgroundTaskRunner.Run();

            app.Run();
        }
    }
}