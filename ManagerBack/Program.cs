using System.Text;
using ManagerBack.Hubs;
using ManagerBack.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Filters;

namespace ManagerBack;

public class Program {
    public static void Main(string[] args){
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // builder.Services.AddSignalR();
        // Add backgrond tasks
        builder.Services.AddHostedService<ClearMatchesTask>();
        
        // Add service layer
        builder.Services.AddScoped<ICardService, CardService>();
        builder.Services.AddScoped<IExpansionService, ExpansionService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IDeckService, DeckService>();
        builder.Services.AddScoped<IMatchConfigService, MatchConfigService>();
        builder.Services.AddSingleton<IMatchService, MatchService>();

        // Add data layer
        // ? should be singletons or scoped
        builder.Services.AddSingleton<ICardRepository, CardRepository>();
        builder.Services.AddSingleton<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IDeckRepository, DeckRepository>();
        builder.Services.AddSingleton<IMatchScriptsRepository, MatchScriptsRepository>();
        builder.Services.AddSingleton<IMatchConfigRepository, MatchConfigRepository>();

        builder.Services.AddSingleton<ICachedCardRepository, CachedCardRepository>();
        builder.Services.AddSingleton<ICachedMatchConfigRepository, CachedMatchConfigRepository>();

        // Add validators
        builder.Services.AddSingleton<IValidator<DeckTemplate>, DeckTemplateValidator>();
        builder.Services.AddSingleton<IValidator<PostUserDto>, PostUserDtoValidator>();
        builder.Services.AddSingleton<IValidator<ExpansionCard>, ExpansionCardValidator>();
        builder.Services.AddSingleton<IValidator<MatchConfig>, MatchConfigValidator>();
        builder.Services.AddSingleton<IValidator<string>, CIDValidator>();

        builder.Services.AddSignalR();
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add database configuration
        builder.Services.Configure<StoreDatabaseSettings>(
            builder.Configuration.GetSection("Database")
        );
        builder.Services.Configure<UserValidationSettings>(
            builder.Configuration.GetSection("UserValidation")
        );
        builder.Services.Configure<DeckRestrictionSettings>(
            builder.Configuration.GetSection("DeckRestrictions")
        );
        builder.Services.Configure<ClearMatchesSettings>(
            builder.Configuration.GetSection("MatchClearing")
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

        // Add logger
        // Initial bootstrap logger, will be replaced by the AddSerilog logger upon setup
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger()
        ;
        builder.Logging.AddSerilog(logger);
        builder.Host.UseSerilog();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        } else {
            app.UseHttpsRedirection();
        }

        app.UseSerilogRequestLogging();


        app.UseAuthorization();
        
        // SignalR hubs
        app.MapHub<MatchLiveHub>("/api/v1/match/live");
        app.MapHub<MatchViewHub>("/api/v1/match/watch");
        app.MapHub<MatchProcessHub>("/api/v1/match/view");

        // Add WebSockets
        app.UseWebSockets(new() {
            KeepAliveInterval = TimeSpan.FromMinutes(10)
        });

        app.MapControllers();
        app.Run();
    }
}