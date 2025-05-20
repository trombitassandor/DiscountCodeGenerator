using System.Net.Sockets;
using static Constants;

namespace DiscountCode;

public class ConsoleClient : IClient
{
    private readonly string _address;
    private readonly int _port;
    private readonly ILogger _logger;

    public ConsoleClient(string address, int port, ILogger logger)
    {
        _address = address;
        _port = port;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        using var client = new TcpClient();
        await client.ConnectAsync(_address, _port);
        await Task.Delay(ConsoleWaitingTimeMs);
        
        using var stream = client.GetStream();
        using var reader = new BinaryReader(stream);
        using var writer = new BinaryWriter(stream);

        WriteLine("Connected to server as client. Enter commands:");

        while (true)
        {
            WriteLine("\nCommands: (1) Generate Codes, (2) Use Code, (any other key) Quit \n> ");
            Write("> ");
            
            var input = Console.ReadLine()?.Trim();

            if (!byte.TryParse(input, out var command))
            {
                break;
            }

            try
            {
                switch (command)
                {
                    case GenerateCodeKey:
                        await GenerateCodes(writer, reader);
                        break;
                    case UseCodeKey:
                    {
                        await UseCode(writer, reader);
                        break;
                    }
                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
                WriteLine($"Error: {ex.Message}");
            }
        }

        WriteLine("Client disconnected.");
    }

    private async Task GenerateCodes(BinaryWriter writer, BinaryReader reader)
    {
        Write("Count: ");
        var count = ushort.Parse(Console.ReadLine());

        Write("Length: ");
        var length = byte.Parse(Console.ReadLine());

        writer.Write(GenerateCodeKey);
        writer.Write(count);
        writer.Write(length);
        writer.Flush();
        
        await Task.Delay(ConsoleWaitingTimeMs);

        var genResult = reader.ReadBoolean();
        WriteLine($"Generate result: {genResult}");
    }
    
    private async Task UseCode(BinaryWriter writer, BinaryReader reader)
    {
        Write("Code: ");
        var code = Console.ReadLine()?.PadRight(FixedCodeLength);
        writer.Write(UseCodeKey);
        writer.Write(code.ToCharArray());
        writer.Flush();
        
        await Task.Delay(ConsoleWaitingTimeMs);

        var useResult = reader.ReadByte();
        var msg = useResult switch
        {
            CodeUseSuccessKey => nameof(CodeUseSuccessKey),
            CodeNotFoundKey => nameof(CodeNotFoundKey),
            CodeAlreadyUsedKey => nameof(CodeAlreadyUsedKey),
            CodeInvalidKey => nameof(CodeInvalidKey),
            _ => "Unknown result"
        };
        WriteLine($"UseCode result: {msg}");
    }
    
    private void WriteLine(string message)
    {
        _logger.WriteLine("CLIENT", message);
    }
    
    private void Write(string message)
    {
        _logger.Write("CLIENT", message);
    }
}