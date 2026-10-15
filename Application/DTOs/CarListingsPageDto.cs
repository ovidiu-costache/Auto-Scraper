namespace Application.DTOs
{
    public sealed record CarListingsPageDto(
        IReadOnlyList<CarListingListItemDto> Items,
        int Page,
        int PageSize,
        bool HasMore);
}
