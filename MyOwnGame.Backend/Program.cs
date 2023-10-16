using MyOwnGame.Backend.Helpers;
using MyOwnGame.Backend.Hubs;
using MyOwnGame.Backend.Managers;
using MyOwnGame.Backend.Parsers;
using MyOwnGame.Backend.Parsers.QuestionInfo;
using MyOwnGame.Backend.Parsers.QuestionInfo.Answer;
using MyOwnGame.Backend.Parsers.QuestionInfo.QuestionParsers;
using MyOwnGame.Backend.Services;

namespace MyOwnGame.Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
            builder.Services.AddTransient<QuestionParser>();
            
            builder.Services.AddTransient<SessionService>();
            builder.Services.AddTransient<FilesService>();
            
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("any", builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();
            
            app.MapControllers();

            app.MapHub<SessionHub>("hubs/session");

            app.UseCors("any");

            app.Run("http://0.0.0.0:3000");
        }
    }
}