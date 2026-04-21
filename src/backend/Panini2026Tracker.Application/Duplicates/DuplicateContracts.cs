namespace Panini2026Tracker.Application.Duplicates;

public sealed record DuplicateItemDto(
    Guid StickerId,
    string StickerCode,
    string DisplayName,
    string CountryCode,
    string CountryName,
    int Quantity,
    bool IsOwned);

public sealed record DuplicateFilter(string? Search, string? CountryCode);

public sealed record UpdateDuplicateCommand(int Quantity);

public interface IDuplicateService
{
    Task<IReadOnlyCollection<DuplicateItemDto>> GetAllAsync(DuplicateFilter filter, CancellationToken cancellationToken);
    Task<DuplicateItemDto> UpdateAsync(Guid stickerId, UpdateDuplicateCommand command, CancellationToken cancellationToken);
    Task DeleteAsync(Guid stickerId, CancellationToken cancellationToken);
}
