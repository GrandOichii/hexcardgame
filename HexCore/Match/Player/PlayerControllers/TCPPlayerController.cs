using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Shared;

namespace HexCore.GameMatch.Players;

/// <summary>
/// Player controller, controlled by a TCP socket
/// </summary>
public class TCPPlayerController : IOPlayerController
{
    private readonly TcpClient _handler;

    public TCPPlayerController(TcpClient handler, Match match) {
        _handler = handler;        
    }

    public override Task<string> Read()
    {
        var stream = _handler.GetStream();
        var result = NetUtil.Read(stream);
        return Task.FromResult(result);
    }

    public override Task Write(string message)
    {
        var handler = _handler.GetStream();
        NetUtil.Write(handler, message);   
        return Task.CompletedTask;
    }

    public override Task CleanUp()
    {
        _handler.Close();
        return Task.CompletedTask;
    }
}