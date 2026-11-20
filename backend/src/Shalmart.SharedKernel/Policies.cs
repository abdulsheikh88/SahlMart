namespace Shalmart.SharedKernel;

public static class AuthSchemes
{
    public const string Customer = "Customer";
    public const string Admin = "Admin";
}

public static class PolicyNames
{
    public const string CatalogEdit = "catalog.edit";
    public const string InventoryWrite = "inventory.write";
    public const string OrdersRead = "orders.read";
    public const string OrdersRefund = "orders.refund";
}
