namespace BasaltMoth.CodeSamples.Orders;

/// <summary>A small domain model that owns the allowed order status transitions.</summary>
public sealed class Order
{
    private Order(Guid id, Guid companyId, string orderNumber, Guid createdByUserId, DateTimeOffset now)
    {
        Id = id;
        CompanyId = companyId;
        OrderNumber = orderNumber;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = now;
        Status = OrderStatus.New;
    }

    public Guid Id { get; }
    public Guid CompanyId { get; }
    public string OrderNumber { get; }
    public Guid CreatedByUserId { get; }
    public DateTimeOffset CreatedAtUtc { get; }
    public OrderStatus Status { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static Order Create( Guid companyId, string orderNumber, Guid createdByUserId, TimeProvider? timeProvider = null)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("Company id is required.", nameof(companyId));

        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number is required.", nameof(orderNumber));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Creator id is required.", nameof(createdByUserId));

        return new Order(Guid.NewGuid(), companyId, orderNumber.Trim(), createdByUserId,
            (timeProvider ?? TimeProvider.System).GetUtcNow());
    }

    public void StartProduction(Guid userId, TimeProvider? timeProvider = null)
    {
        EnsureStatus(OrderStatus.New, "Only new orders can enter production.");
        TransitionTo(OrderStatus.InProduction, userId, timeProvider);
    }

    public void Complete(Guid userId, TimeProvider? timeProvider = null)
    {
        EnsureStatus(OrderStatus.InProduction, "Only orders in production can be completed.");
        TransitionTo(OrderStatus.Completed, userId, timeProvider);
    }

    public void Cancel(Guid userId, TimeProvider? timeProvider = null)
    {
        if (Status is not (OrderStatus.New or OrderStatus.InProduction or OrderStatus.ReturnedForCorrection))
            throw new InvalidOperationException("Only active orders can be cancelled.");
        TransitionTo(OrderStatus.Cancelled, userId, timeProvider);
    }

    public void ReturnForCorrection(Guid userId, TimeProvider? timeProvider = null)
    {
        EnsureStatus(OrderStatus.New, "Only new orders can be returned for correction.");
        TransitionTo(OrderStatus.ReturnedForCorrection, userId, timeProvider);
    }

    public void ApplyCorrection(Guid userId, TimeProvider? timeProvider = null)
    {
        EnsureStatus(OrderStatus.ReturnedForCorrection, "This order is not waiting for correction.");
        TransitionTo(OrderStatus.New, userId, timeProvider);
    }

    private void TransitionTo(OrderStatus nextStatus, Guid userId, TimeProvider? timeProvider)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User id is required.", nameof(userId));

        Status = nextStatus;
        UpdatedByUserId = userId;
        UpdatedAtUtc = (timeProvider ?? TimeProvider.System).GetUtcNow();
    }

    private void EnsureStatus(OrderStatus expected, string message)
    {
        if (Status != expected)
            throw new InvalidOperationException(message);
    }
}
