namespace BasaltMoth.CodeSamples.Production;

public sealed record CuttingPiece
{
    public CuttingPiece(string id, decimal length)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Piece id is required.", nameof(id));

        if (length <= 0m)
            throw new ArgumentOutOfRangeException(nameof(length), "Piece length must be positive.");

        Id = id;
        Length = length;
    }

    public string Id { get; }
    public decimal Length { get; }
}
