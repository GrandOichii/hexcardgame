namespace ManagerBack.Services;

public class WatcherConnection {
    public string ConnectionId { get; set; }
    public string UserId { get; set; }

    public WatcherConnection(string connectionId, string userId)
    {
        ConnectionId = connectionId;
        UserId = userId;
    }

}