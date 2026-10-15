namespace Application.Queries
{
    public sealed record CarListingsQuery(
        string? Make = null,
        string? Model = null,
        int? MinYear = null,
        int? MaxYear = null,
        string? Currency = null,
        int Page = 1,
        int PageSize = 20)
    {
        public const int MaxPageSize = 100;

        public string? Make { get; init; } = Make?.Trim();
        public string? Model { get; init; } = Model?.Trim();
        public string? Currency { get; init; } = Currency?.Trim().ToUpperInvariant();

        public Dictionary<string, string[]> GetValidationErrors()
        {
            var errors = new Dictionary<string, string[]>();

            if (Make is not null && (Make.Length == 0 || Make.Length > 50))
            {
                errors["make"] = ["Make must contain between 1 and 50 characters after trimming."];
            }

            if (Model is not null && (Model.Length == 0 || Model.Length > 50))
            {
                errors["model"] = ["Model must contain between 1 and 50 characters after trimming."];
            }

            if (MinYear is < 1886 or > 9999)
            {
                errors["minYear"] = ["MinYear must be between 1886 and 9999."];
            }

            if (MaxYear is < 1886 or > 9999)
            {
                errors["maxYear"] = ["MaxYear must be between 1886 and 9999."];
            }

            if (MinYear.HasValue && MaxYear.HasValue && MinYear > MaxYear)
            {
                errors["minYear"] = ["MinYear must not exceed MaxYear."];
            }

            if (Currency is not null and not ("EUR" or "RON"))
            {
                errors["currency"] = ["Currency must be EUR or RON."];
            }

            if (Page < 1)
            {
                errors["page"] = ["Page must be at least 1."];
            }
            else if (PageSize > 0 && ((long)Page - 1) * PageSize > int.MaxValue)
            {
                errors["page"] = ["The requested page offset must not exceed 2147483647."];
            }

            if (PageSize is < 1 or > MaxPageSize)
            {
                errors["pageSize"] = ["Page size must be between 1 and 100."];
            }

            return errors;
        }
    }
}
