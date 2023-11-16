using NoahWeb_Private_Asset_Module.enums;

namespace NoahWeb_Private_Asset_Module.Models;

public class AssetView
{
    public DateTime ValuationDate { get; set; }
    public int AssetID { get; set; }
    public string FundName { get; set; }
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

public class PortfolioAssetView
{
    public DateTime ValuationDate { get; set; }
    public List<AssetView> AssetViews { get; set; }

    // Additional properties for portfolio-level aggregates
}
