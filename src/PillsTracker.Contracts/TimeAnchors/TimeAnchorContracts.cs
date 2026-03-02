namespace PillsTracker.Contracts.TimeAnchors;

public sealed record TimeAnchorDto(Guid Id, string Key, string Time, bool IsSystem);
public sealed record UpsertTimeAnchorRequest(string Key, string Time);
