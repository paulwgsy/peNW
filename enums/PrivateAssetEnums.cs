using System;
namespace NoahWeb_Private_Asset_Module.enums
{
    public enum FundStrategy
    {
        Buyout,
        Growth,
        Venture,
        // ... other strategies
    }

    public enum FundAssetClass
    {
        PrivateEquity,
        RealEstate,
        Debt,
        Infrastructure,
        // ... other asset classes
    }

    public enum TransactionType
    {
        Commitment,
        CarryValue,
        Fee,
        CapitalCall,
        Distribution
    }

    public enum CalculationType
    {
        Add,
        Subtract,
        Set
    }

    public enum AssetType
    {
        Model,
        Fund,
        Investment
    }
}

