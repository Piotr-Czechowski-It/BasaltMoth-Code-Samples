namespace BasaltMoth.CodeSamples.Production;

public sealed record CuttingOptimizationSettings
{
    public CuttingOptimizationSettings(decimal stockLength, decimal sawKerf)
    {
        if (stockLength <= 0m)
            throw new ArgumentOutOfRangeException(nameof(stockLength), "Stock length must be positive.");

        if (sawKerf < 0m)
            throw new ArgumentOutOfRangeException(nameof(sawKerf), "Saw kerf cannot be negative.");

        StockLength = stockLength;
        SawKerf = sawKerf;
    }

    public decimal StockLength { get; }
    public decimal SawKerf { get; }
}
