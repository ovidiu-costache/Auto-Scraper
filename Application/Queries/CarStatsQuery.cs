namespace Application.Queries
{
    public sealed record CarStatsQuery(
        string Make,
        string Model,
        int MinYear,
        int MaxYear,
        string Currency)
    {
        public string Make { get; init; } = Make.Trim();
        public string Model { get; init; } = Model.Trim();
        public string Currency { get; init; } = Currency.Trim().ToUpperInvariant();

        public Dictionary<string, string[]> GetValidationErrors()
        {
            var errors = new Dictionary<string, string[]>();

            if (Make.Length == 0 || Make.Length > 50)
            {
                errors["make"] = ["Make must contain between 1 and 50 characters after trimming."];
            }

            if (Model.Length == 0 || Model.Length > 50)
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

            if (MinYear > MaxYear)
            {
                errors["minYear"] = ["MinYear must not exceed MaxYear."];
            }

            if (Currency is not ("EUR" or "RON"))
            {
                errors["currency"] = ["Currency must be EUR or RON."];
            }

            return errors;
        }
    }
}
