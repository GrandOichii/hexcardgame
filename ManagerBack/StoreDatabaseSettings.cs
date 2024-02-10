namespace ManagerBack;

public class StoreDatabaseSettings {
    public required string ConnectionString { get; set; }
    public required string DatabaseName { get; set; }
    public required string CardCollectionName { get; set; }
    public required string UserCollectionName { get; set; }
}