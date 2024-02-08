namespace ManagerBack;

public class Program {
    public static void Main(string[] args){
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        // Add service layer
        builder.Services.AddScoped<ICardService, CardService>();
        builder.Services.AddScoped<IExpansionService, ExpansionService>();

        // Add data layer
        builder.Services.AddSingleton<ICardRepository, CardRepository>();

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add database configuration
        builder.Services.Configure<StoreDatabaseSettings>(
            builder.Configuration.GetSection("Database")
        );

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

        app.MapControllers();

        app.Run();
    }
}