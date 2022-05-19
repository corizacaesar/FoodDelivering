namespace FoodService.Models
{
    public partial class FoodData
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Stock { get; set; }
        public double Price { get; set; }
    }
}
