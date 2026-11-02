using BasaltMoth.CodeSamples.Production;

namespace BasaltMoth.CodeSamples.Tests.Production;

public sealed class CuttingOptimizationServiceTests
{
    private readonly CuttingOptimizationService _service = new();

    [Fact]
    public void Optimize_EmptyInputReturnsNoBars()
    {
        var plan = _service.Optimize([], new CuttingOptimizationSettings(6m, 0.002m));

        Assert.Empty(plan.Bars);
    }

    [Fact]
    public void Optimize_PacksPiecesAndAppliesKerfBetweenCuts()
    {
        var plan = _service.Optimize(
            [new CuttingPiece("part-b", 1.5m), new CuttingPiece("part-a", 2m)],
            new CuttingOptimizationSettings(4m, 0.002m));

        var bar = Assert.Single(plan.Bars);

        Assert.Equal(new[] { "part-a", "part-b" }, bar.Pieces.Select(piece => piece.PieceId));
        Assert.Equal(2.002m, bar.Pieces[1].Start);
        Assert.Equal(3.502m, bar.UsedLength);
        Assert.Equal(0.498m, bar.WasteLength);
    }

    [Fact]
    public void Optimize_BestFitUsesTheBarWithLeastRemainingSpace()
    {
        var plan = _service.Optimize(
            [new CuttingPiece("six", 6m), new CuttingPiece("five", 5m), new CuttingPiece("four", 4m)],
            new CuttingOptimizationSettings(10m, 0m));

        Assert.Equal(2, plan.Bars.Count);
        Assert.Equal(new[] { "six", "four" }, plan.Bars[0].Pieces.Select(piece => piece.PieceId));
        Assert.Equal(0m, plan.Bars[0].WasteLength);
    }

    [Fact]
    public void Optimize_IsDeterministicForEqualLengths()
    {
        var pieces = new[]
        {
            new CuttingPiece("z", 3m), new CuttingPiece("b", 3m),
            new CuttingPiece("a", 3m), new CuttingPiece("c", 1m)
        };
        var settings = new CuttingOptimizationSettings(6m, 0.01m);

        var first = _service.Optimize(pieces, settings);
        var second = _service.Optimize(pieces, settings);

        Assert.Equal(Describe(first), Describe(second));
        Assert.Equal(new[] { "a", "c", "b", "z" }, first.Bars.SelectMany(bar => bar.Pieces).Select(piece => piece.PieceId));
    }

    [Fact]
    public void Optimize_RejectsPieceLongerThanStock()
    {
        Assert.Throws<ArgumentException>(() => _service.Optimize(
            [new CuttingPiece("too-long", 6.01m)], new CuttingOptimizationSettings(6m, 0m)));
    }

    [Fact]
    public void Optimize_RejectsDuplicateIds()
    {
        Assert.Throws<ArgumentException>(() => _service.Optimize(
            [new CuttingPiece("duplicate", 1m), new CuttingPiece("duplicate", 2m)],
            new CuttingOptimizationSettings(6m, 0m)));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, 0)]
    [InlineData(1, -0.1)]
    public void Settings_RejectInvalidLengths(double stock, double kerf)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CuttingOptimizationSettings((decimal)stock, (decimal)kerf));
    }

    private static string[] Describe(CuttingPlan plan) => plan.Bars
        .SelectMany(bar => bar.Pieces.Select(piece =>
            $"{bar.Index}:{piece.PieceId}:{piece.Start}:{piece.End}:{bar.WasteLength}"))
        .ToArray();
}
