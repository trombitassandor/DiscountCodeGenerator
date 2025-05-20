using System.Collections.Concurrent;
using System.Text.Json;
using DiscountCode;
using static Constants;
using System.Threading.Channels;
using System.Threading;

public class Service : IService, IAsyncDisposable
{
    private readonly string _filePath;
    private readonly ConcurrentDictionary<string, bool> _codeUsage;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly Channel<bool> _saveChannel;
    private readonly CancellationTokenSource _cts;
    private readonly Task _backgroundSaveTask;
    private readonly ILogger _logger;
    
    private bool _isDisposed;

    public Service(string filePath, ILogger logger)
    {
        _codeUsage = new ConcurrentDictionary<string, bool>();
        _filePath = filePath;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        EnsureFileExists();
        LoadCodes();

        _saveChannel = Channel.CreateUnbounded<bool>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
        _cts = new CancellationTokenSource();
        _backgroundSaveTask = Task.Run(() => ProcessSaveQueueAsync(_cts.Token));
    }

    public Task<bool> GenerateCode(ushort count, byte length)
    {
        if (count > MaxCodePerRequest || !IsValidCodeLength(length))
        {
            return Task.FromResult(false);
        }

        var random = Random.Shared;
        var generated = 0;

        while (generated < count)
        {
            var code = GenerateCode(random, length);
            if (_codeUsage.TryAdd(code, false))
            {
                generated++;
            }
        }

        _saveChannel.Writer.TryWrite(true);

        return Task.FromResult(true);
    }

    public Task<byte> UseCode(string code)
    {
        if (!IsValidCodeLength(code.Length))
        {
            return Task.FromResult(CodeInvalidKey);
        }

        if (!_codeUsage.TryGetValue(code, out var used))
        {
            return Task.FromResult(CodeNotFoundKey);
        }

        if (used)
        {
            return Task.FromResult(CodeAlreadyUsedKey);
        }

        _codeUsage[code] = true;

        _saveChannel.Writer.TryWrite(true);

        return Task.FromResult(CodeUseSuccessKey);
    }

    private static string GenerateCode(Random random, byte length)
    {
        return new string(Enumerable.Repeat(CodeChars, length)
            .Select(s => s[random.Next(s.Length)])
            .ToArray());
    }

    private static bool IsValidCodeLength(int length)
    {
        return length >= MinCodeLength &&
               length <= MaxCodeLength;
    }

    private void EnsureFileExists()
    {
        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }
    }

    private void LoadCodes()
    {
        var fileContent = File.ReadAllText(_filePath);

        var entries =
            JsonSerializer.Deserialize<List<DiscountCodeEntry>>(fileContent)
            ?? new List<DiscountCodeEntry>();

        foreach (var entry in entries)
        {
            _codeUsage.TryAdd(entry.Code, entry.Used);
        }

        LogCodeUsage();
    }

    private async Task ProcessSaveQueueAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var _ in _saveChannel.Reader.ReadAllAsync(cancellationToken))
            {
                await SaveCodesAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on Dispose
        }
    }

    private async Task SaveCodesAsync()
    {
        var entries = _codeUsage.Select(kvp => new DiscountCodeEntry
        {
            Code = kvp.Key,
            Used = kvp.Value
        }).ToList();

        var jsonString = JsonSerializer.Serialize(entries, _jsonOptions);

        await File.WriteAllTextAsync(_filePath, jsonString);

        LogCodeUsage();
    }

    private void LogCodeUsage()
    {
        Log("LogCodeUsage");

        foreach (var codeUsage in _codeUsage)
        {
            var used = codeUsage.Value ? "used" : "unused";
            Log($"{used} - {codeUsage.Key}");
        }
    }
    
    private void Log(string message)
    {
        _logger.WriteLine("SERVICE", message);
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        _cts.Cancel();
        _saveChannel.Writer.Complete();
        
        try
        {
            await _backgroundSaveTask;
        } 
        catch (OperationCanceledException) {}
        
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
}
