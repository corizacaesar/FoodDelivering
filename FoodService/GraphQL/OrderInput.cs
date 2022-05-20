namespace FoodService.GraphQL
{
    public record OrderInput
    (
        string Product,
        int Quantity
    );
}