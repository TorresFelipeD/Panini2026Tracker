namespace Panini2026Tracker.Api.Contracts;

public sealed record UpdateStickerStateRequest(bool IsOwned, int DuplicateCount, string? Notes);

public sealed record UpdateDuplicateRequest(int Quantity);

public sealed record DeleteLogsRequest(string? Category, string? Level, string? Search);
