namespace Lab1.Models
{
    public class FilteredProduct
    {
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string? Link { get; set; }
        public string? Resolution { get; set; }
        public decimal TotalPriceFilteredProducts { get; set; }
        public DateTime Date { get; set; }
    }
}