namespace Nenia.SGP.Models;

public sealed class RegionalPrice
{
    public int PriceID { get; set; }
    public int ProductID { get; set; }
    public int RegionID { get; set; }
    public double? RegionalPriceValue { get; set; }
    public double DiscountRate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
