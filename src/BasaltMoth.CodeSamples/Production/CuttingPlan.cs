namespace BasaltMoth.CodeSamples.Production;

public sealed record PiecePlacement(string PieceId, decimal Start, decimal End);
public sealed record StockBarPlan(int Index, decimal UsedLength, decimal WasteLength, IReadOnlyList<PiecePlacement> Pieces);
public sealed record CuttingPlan(IReadOnlyList<StockBarPlan> Bars);
