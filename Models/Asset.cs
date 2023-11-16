using System.ComponentModel.DataAnnotations.Schema;
using NoahWeb_Private_Asset_Module.enums;

namespace NoahWeb_Private_Asset_Module.Models;

public class Asset
{
    public int AssetID { get; set; }
    public AssetType Type { get; set; } // a model, fund or investment
    public string FundName { get; set; }
    public string Currency { get; set; }  // ISO Code
    public FundStrategy Strategy { get; set; }  // Enum
    public FundAssetClass AssetClass { get; set; }  // Enum
    public int ManagerID { get; set; }
    public int Vintage { get; set; }
    public int? AssociatedFundID { get; set; }
    [ForeignKey("AssociatedFundID")]
    public virtual Asset AssociatedFund { get; set; }

    // Navigation property
    public Manager Manager { get; set; }
}

public class AssetAggregate
{
    public int AssetAggregateID { get; set; }
    public string FundName { get; set; }
    public string Currency { get; set; }  // ISO Code

    public List<Asset> Assets { get; set; }


}

public class PrivateAssetPortfolio
{
    public int PrivateAssetPortfolioID { get; set; }
    public List<int> AssetIDs { get; set; } // List of Asset IDs
    public List<Asset> Assets { get; set; } // List to hold detailed Asset objects
    public string PortfolioName { get; set; }
    // Other properties...
}

public class PrivateAssetPortfolioSummary
{
    public DateTime ValuationDate { get; set; }
    public decimal TotalCommitment { get; set; }
    public decimal TotalContribution { get; set; }
    public decimal TotalDistribution { get; set; }
    public decimal TotalNetCashFlow { get; set; }
    public decimal TotalCarryValue { get; set; }
    public decimal PortfolioDPI { get; set; }
    public decimal PortfolioRVPI { get; set; }
    public decimal PortfolioTVPI { get; set; }

    // Additional fields as needed
}


public class GlobalConstants
{
    public static HashSet<string> ValidISOCurrencyCodes { get; } = new HashSet<string>
    {
        "USD", "EUR", "GBP", "JPY", "CAD", "CNY", "CHF",
    };

    // ... other global constants or static methods ...
}
