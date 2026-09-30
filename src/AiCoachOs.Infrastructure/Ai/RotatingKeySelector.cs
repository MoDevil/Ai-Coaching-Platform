namespace AiCoachOs.Infrastructure.Ai;

public class RotatingKeySelector
{
    private readonly string _envVarName;
    private readonly object _lock = new();
    private int _currentIndex = 0;
    private DateTime? _unavailableUntilUtc;

    public string EnvVarName => _envVarName;

    public RotatingKeySelector(string envVarName)
    {
        _envVarName = envVarName ?? string.Empty;
    }

    public bool IsAvailable()
    {
        lock (_lock)
        {
            if (_unavailableUntilUtc.HasValue)
            {
                if (DateTime.UtcNow < _unavailableUntilUtc.Value)
                {
                    return false;
                }

                _unavailableUntilUtc = null;
            }

            return true;
        }
    }

    public DateTime? UnavailableUntilUtc
    {
        get
        {
            lock (_lock)
            {
                return _unavailableUntilUtc;
            }
        }
    }

    public IReadOnlyList<string> GetKeys()
    {
        if (string.IsNullOrWhiteSpace(_envVarName))
            return Array.Empty<string>();

        var raw = Environment.GetEnvironmentVariable(_envVarName);
        if (string.IsNullOrWhiteSpace(raw))
            return Array.Empty<string>();

        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                  .Where(k => !string.IsNullOrWhiteSpace(k))
                  .ToList();
    }

    public string? GetCurrentOrNextKey()
    {
        var keys = GetKeys();
        if (keys.Count == 0)
            return null;

        lock (_lock)
        {
            if (!IsAvailable())
                return null;

            var key = keys[_currentIndex % keys.Count];
            _currentIndex = (_currentIndex + 1) % keys.Count;
            return key;
        }
    }

    public string? RotateToNextKey()
    {
        return GetCurrentOrNextKey();
    }

    public void MarkTemporarilyUnavailable(TimeSpan? duration = null)
    {
        lock (_lock)
        {
            _unavailableUntilUtc = DateTime.UtcNow.Add(duration ?? TimeSpan.FromSeconds(60));
        }
    }

    public void ResetAvailability()
    {
        lock (_lock)
        {
            _unavailableUntilUtc = null;
            _currentIndex = 0;
        }
    }
}
