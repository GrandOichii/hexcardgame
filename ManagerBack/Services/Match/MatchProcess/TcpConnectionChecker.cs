using System.Net.Sockets;
using Shared;

namespace ManagerBack.Services;


public class TcpConnectionChecker : IConnectionChecker {
    private readonly TcpClient _socket;

    public TcpConnectionChecker(TcpClient socket)
    {
        _socket = socket;
    }

    public async Task<bool> Check()
    {
        try {
            NetUtil.Write(_socket.GetStream(), "ping");
            _socket.ReceiveTimeout = 5000;
            var resp = await Read();
            _socket.ReceiveTimeout = 0;
            return resp == "pong";
        } catch {
            // TODO bee more specific with exception types
            return false;
        }
    }

    public Task<string> Read()
    {
        return Task.FromResult(
            NetUtil.Read(_socket.GetStream())
        );
    }
}
