namespace Nenia.SGP.Models;

public sealed class Product
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = "";
    public string? Capacity { get; set; }
    public string? MainImagePath { get; set; }
    public string? ThumbnailImagePath { get; set; }
    public int? PageNumber { get; set; }
    public int? OrderInPage { get; set; }
    public double StandardPrice { get; set; }
    public double? PricePerUnit { get; set; }
    public string? StorageType { get; set; }
    public string? Certification { get; set; }
    public string? Tags { get; set; }
    public string? Ingredients { get; set; }
    public string? AllergyInfo { get; set; }
    public string? Description { get; set; }
    public string? OriginalImagePath { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
