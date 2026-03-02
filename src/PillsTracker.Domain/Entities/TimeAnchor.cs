namespace PillsTracker.Domain.Entities;

public sealed class TimeAnchor
{
    public Guid Id { get; private set; }
    public string Key { get; private set; }
    public TimeOnly Time { get; private set; }
    public Guid? OwnerUserId { get; private set; }

    public TimeAnchor(Guid id, string key, TimeOnly time, Guid? ownerUserId = null)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Key = EnsureRequired(key, nameof(key));
        Time = time;
        OwnerUserId = ownerUserId;
    }

    public void Update(string key, TimeOnly time)
    {
        Key = EnsureRequired(key, nameof(key));
        Time = time;
    }

    private static string EnsureRequired(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
        }

        return value.Trim();
    }
}
