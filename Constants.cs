public static class Constants
{
    public const byte MinCodeLength = 7;
    public const byte MaxCodeLength = 8;
    public const int FixedCodeLength = 8;

    public const ushort MaxCodePerRequest = 2000;
    public const string CodeChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public const byte GenerateCodeKey = 1;
    public const byte UseCodeKey = 2;

    public const byte CodeUseSuccessKey = 0;
    public const byte CodeNotFoundKey = 1;
    public const byte CodeAlreadyUsedKey = 2;
    public const byte CodeInvalidKey = 3;

    public const int ServerWaitingTimeMs = 50;
    public const int ConsoleWaitingTimeMs = 100;
}