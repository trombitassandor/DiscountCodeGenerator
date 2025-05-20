using System.Net;
using DiscountCode;
using static Constants;

class Program
{
    static async Task Main(string[] args)
    {
        var address = IPAddress.Loopback;
        var port = 5000;
        var codesFilePath = "discount_codes.json";
        
        var logger = new ConsoleLogger();
        var service = new Service(codesFilePath, logger);
        var server = new TcpServer(address, port, service, logger);
        var cts = new CancellationTokenSource();
        
        server.StartAsync(cts.Token);
        await Task.Delay(ConsoleWaitingTimeMs);

        var consoleClient = new ConsoleClient(address.ToString(), port, logger);
        await consoleClient.StartAsync();
        
        await cts.CancelAsync();
    }
}
