namespace Nenia.SGP.Models;

public sealed class Region
{
    public int RegionID { get; set; }
    public string RegionName { get; set; } = "";
    public string? DefaultMainType { get; set; }
    public DateTime CreatedDate { get; set; }
}
