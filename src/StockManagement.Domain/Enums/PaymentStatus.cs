namespace StockManagement.Domain.Enums;

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    PartiallyPaid = 2,
    Refunded = 3,
    Failed = 4
}
