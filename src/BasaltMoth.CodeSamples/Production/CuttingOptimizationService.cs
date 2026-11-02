namespace BasaltMoth.CodeSamples.Production;

/// <summary>
/// Packs longest pieces first and chooses the fitting bar with least remaining space.
/// This best-fit decreasing heuristic is deterministic but does not guarantee a global optimum.
/// </summary>
public sealed class CuttingOptimizationService
{
    public CuttingPlan Optimize(IEnumerable<CuttingPiece> pieces, CuttingOptimizationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(pieces);
        ArgumentNullException.ThrowIfNull(settings);

        var input = pieces.ToArray();

        Validate(input, settings);

        if (input.Length == 0)
            return new CuttingPlan([]);

        var bars = new List<BarAccumulator>();

        foreach (var piece in input.OrderByDescending(piece => piece.Length).ThenBy(piece => piece.Id, StringComparer.Ordinal))
        {
            BarAccumulator? bestFit = null;

            var leastRemaining = decimal.MaxValue;

            foreach (var bar in bars)
            {
                var kerf = bar.Placements.Count == 0 ? 0m : settings.SawKerf;
                var usedAfterPlacement = bar.UsedLength + kerf + piece.Length;

                if (usedAfterPlacement > settings.StockLength)
                    continue;

                var remaining = settings.StockLength - usedAfterPlacement;

                if (remaining < leastRemaining)
                {
                    leastRemaining = remaining;
                    bestFit = bar;
                }
            }

            bestFit ??= AddBar(bars, settings.StockLength);

            bestFit.Add(piece, settings.SawKerf);
        }

        return new CuttingPlan(bars.Select(bar => bar.ToPlan()).ToArray());
    }

    private static void Validate(IReadOnlyCollection<CuttingPiece> pieces, CuttingOptimizationSettings settings)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);

        foreach (var piece in pieces)
        {
            if (piece is null)
                throw new ArgumentException("Pieces cannot contain null values.", nameof(pieces));

            if (!ids.Add(piece.Id))
                throw new ArgumentException($"Duplicate piece id '{piece.Id}'.", nameof(pieces));

            if (piece.Length > settings.StockLength)
                throw new ArgumentException($"Piece '{piece.Id}' exceeds stock length.", nameof(pieces));
        }
    }

    private static BarAccumulator AddBar(List<BarAccumulator> bars, decimal stockLength)
    {
        var bar = new BarAccumulator(bars.Count + 1, stockLength);

        bars.Add(bar);

        return bar;
    }

    private sealed class BarAccumulator(int index, decimal stockLength)
    {
        public int Index { get; } = index;
        public decimal StockLength { get; } = stockLength;
        public List<PiecePlacement> Placements { get; } = [];
        public decimal UsedLength { get; private set; }

        public void Add(CuttingPiece piece, decimal sawKerf)
        {
            var start = Placements.Count == 0 ? 0m : UsedLength + sawKerf;
            var end = start + piece.Length;

            Placements.Add(new PiecePlacement(piece.Id, start, end));
            UsedLength = end;
        }

        public StockBarPlan ToPlan() => new( Index, UsedLength, StockLength - UsedLength, Placements.ToArray());
    }
}
