using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace ManagerBack;

public class Program {
    public static void Main(string[] args){
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // builder.Services.AddSignalR();
        
        // Add service layer
        builder.Services.AddScoped<ICardService, CardService>();
        builder.Services.AddScoped<IExpansionService, ExpansionService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddSingleton<IMatchService, MatchService>();
        builder.Services.AddSingleton<IDeckService, DeckService>();

        // Add data layer
        // ? should be singletons or scoped
        builder.Services.AddSingleton<ICardRepository, CardRepository>();
        builder.Services.AddSingleton<ICachedCardRepository, CachedCardRepository>();
        builder.Services.AddSingleton<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IDeckRepository, DeckRepository>();

        // Add validators
        builder.Services.AddSingleton<IValidator<PostDeckDto>, DeckValidator>();
        builder.Services.AddSingleton<IValidator<string>, CIDValidator>();

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add database configuration
        builder.Services.Configure<StoreDatabaseSettings>(
            builder.Configuration.GetSection("Database")
        );

        // Add cache
        builder.Services.AddStackExchangeRedisCache(options => {
            var conn = builder.Configuration.GetConnectionString("Redis");
            options.Configuration = conn;
        });

        // Add authentication
        // Add auth to app
        builder.Services.AddSwaggerGen(options => {
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            options.OperationFilter<SecurityRequirementsOperationFilter>();
        });
        // Add auth to swagger
        builder.Services.AddAuthentication().AddJwtBearer(options => {
            options.TokenValidationParameters = new TokenValidationParameters{
                ValidateIssuerSigningKey = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!))
            };
        });

        // Add mapping profiles
        builder.Services.AddAutoMapper(typeof(Program).Assembly);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        // app.MapHub<MatchHub>("match-hub");
        app.UseWebSockets(new() {
            KeepAliveInterval = TimeSpan.FromMinutes(10)
        });

        app.MapControllers();
        app.Run();
    }
}