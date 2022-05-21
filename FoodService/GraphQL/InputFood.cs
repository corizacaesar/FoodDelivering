namespace FoodService.GraphQL
{
    public record InputFood
    (
        int? Id,
        string Name,
        int Stock,
        double Price
    );
}
