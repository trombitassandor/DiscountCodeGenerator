namespace DiscountCode;

public interface IService
{
    public Task<bool> GenerateCode(ushort count, byte length);
    public Task<byte> UseCode(string code);
}