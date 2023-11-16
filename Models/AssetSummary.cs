using NoahWeb_Private_Asset_Module.enums;

namespace NoahWeb_Private_Asset_Module.Models;

public class AssetSummary
{
    public int AssetID { get; set; }
    public String FundName { get; set; }
    public int Vintage { get; set; }
    public DateTime SpecifiedDate { get; set; }
    public decimal Commitment { get; set; }
    public decimal ContributionTotal { get; set; }
    public decimal DistributionTotal { get; set; }
    public decimal NetCashFlowTotal { get; set; }
    public decimal CarryValue { get; set; }
    public decimal DPI { get; set; }
    public decimal RVPI { get; set; }
    public decimal TVPI { get; set; }
}