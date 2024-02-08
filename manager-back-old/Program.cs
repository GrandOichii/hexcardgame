using core.cards;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Data.Common;
using manager_back;

// TODO remake everything

public class ManagerContext : DbContext
{
    #region DB Sets

    public DbSet<CardData> Cards { get; set; }
    public DbSet<DeckData> Decks { get; set; }
    public DbSet<ExpansionData> Expansions { get; set; }
    public DbSet<ExpansionCardData> ExpansionCards { get; set; }
    public DbSet<DeckCardData> DeckCards { get; set; }
    public DbSet<MatchConfigData> MatchConfigs { get; set; }

    #endregion

    public ManagerContext()
    {
        Database.EnsureCreated();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(Program.ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ExpansionCardData>()
            .Property(d => d.ID)
            .ValueGeneratedOnAdd();
    }
}

public class Program 
{    
    public static string ConnectionString { get; private set;  }
    public static void Main(string[] args) {

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        ConfigureDB(args[0]);
        ConfigureSources();

        app.Run();
    }

    private static void ConfigureDB(string password)
    {
       ConnectionString = "Host=localhost;Username=postgres;Password=" + password + ";Database=testdb";
    }

    private static void ConfigureSources() {
        var cm = new DBCardMaster();
        Global.CMaster = cm;

        var dm = new DeckManager("../decks");
        Global.DManager = dm;

        var configM = new ConfigsManager("../configs");
        Global.CManager = configM;
    }   
}