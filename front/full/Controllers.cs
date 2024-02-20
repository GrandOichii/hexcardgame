
using System.Net.Sockets;
using Shared;

public interface IIOController {
	public string Read();
	public void Write(string message);
}

public class TcpController : IIOController {
    public TcpClient Client { get; }
    public TcpController(TcpClient client)
    {
        Client = client;
    }

    public string Read()
    {
        return NetUtil.Read(Client.GetStream());
    }

    public void Write(string message)
    {
        NetUtil.Write(Client.GetStream(), message);
    }
}

// TODO
public class WebSocketController : IIOController
{
    public string Read()
    {
        throw new System.NotImplementedException();
    }

    public void Write(string message)
    {
        throw new System.NotImplementedException();
    }
}