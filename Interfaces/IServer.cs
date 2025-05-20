namespace DiscountCode;

public interface IServer
{
    public Task StartAsync(CancellationToken cancellationToken = default);
}