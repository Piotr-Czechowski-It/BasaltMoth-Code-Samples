using BasaltMoth.CodeSamples.Orders;

namespace BasaltMoth.CodeSamples.Tests.Orders;

public sealed class OrderLifecycleTests
{
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Create_TrimsNumberAndStartsInNewState()
    {
        var clock = new FixedTimeProvider(DateTimeOffset.Parse("2026-01-02T03:04:05Z"));

        var order = Order.Create(CompanyId, "  DEMO-001  ", UserId, clock);

        Assert.Equal("DEMO-001", order.OrderNumber);
        Assert.Equal(OrderStatus.New, order.Status);
        Assert.Equal(clock.GetUtcNow(), order.CreatedAtUtc);
        Assert.Null(order.UpdatedAtUtc);
    }

    [Fact]
    public void StartProductionAndComplete_RequireSequentialTransitions()
    {
        var clock = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var order = CreateOrder(clock);

        order.StartProduction(UserId, clock);
        Assert.Equal(OrderStatus.InProduction, order.Status);
        Assert.Equal(UserId, order.UpdatedByUserId);

        order.Complete(UserId, clock);
        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Equal(clock.GetUtcNow(), order.UpdatedAtUtc);
    }

    [Fact]
    public void Complete_RejectsOrderThatHasNotEnteredProduction()
    {
        var order = CreateOrder();

        Assert.Throws<InvalidOperationException>(() => order.Complete(UserId));
        Assert.Equal(OrderStatus.New, order.Status);
    }

    [Theory]
    [InlineData(OrderStatus.New)]
    [InlineData(OrderStatus.InProduction)]
    [InlineData(OrderStatus.ReturnedForCorrection)]
    public void Cancel_AllowsActiveStatuses(OrderStatus initialStatus)
    {
        var order = CreateOrder();

        if (initialStatus == OrderStatus.InProduction)
            order.StartProduction(UserId);

        if (initialStatus == OrderStatus.ReturnedForCorrection)
            order.ReturnForCorrection(UserId);

        order.Cancel(UserId);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Correction_ReturnsOrderToNewAndPreservesIdentity()
    {
        var order = CreateOrder();

        var originalId = order.Id;

        order.ReturnForCorrection(UserId);

        order.ApplyCorrection(UserId);

        Assert.Equal(originalId, order.Id);
        Assert.Equal(OrderStatus.New, order.Status);
    }

    [Fact]
    public void CompletedOrderCannotBeCancelled()
    {
        var order = CreateOrder();

        order.StartProduction(UserId);

        order.Complete(UserId);

        Assert.Throws<InvalidOperationException>(() => order.Cancel(UserId));
        Assert.Equal(OrderStatus.Completed, order.Status);
    }

    [Fact]
    public void Create_RejectsMissingRequiredValues()
    {
        Assert.Throws<ArgumentException>(() => Order.Create(Guid.Empty, "DEMO-001", UserId));
        Assert.Throws<ArgumentException>(() => Order.Create(CompanyId, " ", UserId));
        Assert.Throws<ArgumentException>(() => Order.Create(CompanyId, "DEMO-001", Guid.Empty));
    }

    private static Order CreateOrder(TimeProvider? clock = null) => Order.Create(CompanyId, "DEMO-001", UserId, clock);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
