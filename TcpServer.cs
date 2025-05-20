using System.Net;
using System.Net.Sockets;
using DiscountCode;
using static Constants;

public class TcpServer : IServer
{
    private readonly TcpListener _listener;
    private readonly IService _service;
    private readonly ILogger _logger;

    public TcpServer(IPAddress address, int port, IService service, ILogger logger)
    {
        _listener = new TcpListener(address, port);
        _service = service;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _listener.Start();
        Log("Server started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync(cancellationToken);
            _ = HandleClientAsync(client, cancellationToken);
        }
        
        Log("Server shutting down.");
        _listener.Stop();
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken = default)
    {
        Log("Client connected.");
        
        using var stream = client.GetStream();
        using var reader = new BinaryReader(stream);
        using var writer = new BinaryWriter(stream);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!reader.BaseStream.CanRead)
                {
                    break;
                }

                if (!stream.DataAvailable)
                {
                    await Task.Delay(ServerWaitingTimeMs, cancellationToken);
                    continue;
                }

                var opcode = reader.ReadByte();
                switch (opcode)
                {
                    case GenerateCodeKey:
                    {
                        var count = reader.ReadUInt16();
                        var length = reader.ReadByte();
                        var result = await _service.GenerateCode(count, length);
                        writer.Write(result);
                        break;
                    }
                    case UseCodeKey:
                    {
                        var code = new string(reader.ReadChars(FixedCodeLength)).Trim();
                        var result = await _service.UseCode(code);
                        writer.Write(result);
                        break;
                    }
                    default:
                    {
                        Log($"Not implemented opcode: {opcode}");
                        break;
                    }
                }
            }
        }
        catch (EndOfStreamException)
        {
            Log("Client disconnected.");
        }
        catch (OperationCanceledException)
        {
            Log("Client handler canceled due to server shutdown.");
        }
        catch (Exception ex)
        {
            Log($"Error handling client: {ex.Message}");
        }
        finally
        {
            client.Close();
            client.Dispose();
            Log("Client connection closed and resources disposed.");
        }
    }

    private void Log(string message)
    {
        _logger.WriteLine("SERVER", message);
    }
}