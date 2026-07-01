namespace StockManagement.Domain.Enums;

/// <summary>Canonical role names used across RBAC. Stored as string in Identity.</summary>
public static class Roles
{
    public const string SuperAdmin = "Super Admin";
    public const string OrgAdmin = "Organization Admin";
    public const string ShopAdmin = "Shop Admin";
    public const string Manager = "Manager";
    public const string InventoryManager = "Inventory Manager";
    public const string Cashier = "Cashier";
    public const string SalesExecutive = "Sales Executive";
    public const string PurchaseExecutive = "Purchase Executive";
    public const string Accountant = "Accountant";
    public const string Customer = "Customer";

    public static readonly string[] All =
    {
        SuperAdmin, OrgAdmin, ShopAdmin, Manager, InventoryManager,
        Cashier, SalesExecutive, PurchaseExecutive, Accountant, Customer
    };
}
