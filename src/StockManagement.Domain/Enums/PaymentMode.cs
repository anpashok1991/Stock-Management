namespace StockManagement.Domain.Enums;

[Flags]
public enum PaymentMode
{
    Cash = 1,
    Card = 2,
    Upi = 4,
    NetBanking = 8,
    Wallet = 16,
    Credit = 32
}
