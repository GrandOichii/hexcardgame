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
            var resp = await Read();
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

