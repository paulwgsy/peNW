using System;
namespace NoahWeb_Private_Asset_Module.Models
{
    public class Portfolio
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<PortfolioValuation> Valuations { get; set; }
        // ... other properties like Owner, CreatedDate, etc.
    }

    public class PortfolioValuation
    {
        public int Id { get; set; }
        public decimal Value { get; set; }
        public string Currency { get; set; }
        public DateTime Date { get; set; }
        public int InvestmentId { get; set; }
        public Investment Investment { get; set; }
    }

    public class Investment
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public int InstrumentId { get; set; }
        public Instrument Instrument { get; set; }
        public decimal Nominal { get; set; }
        // ... other properties like ShareCount, CostBasis, etc.
    }

    public class Instrument
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InstrumentComposition> Compositions { get; set; }
        // ... other properties ...
    }

    public class InstrumentComposition
    {
        public int Id { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public List<Override> Overrides { get; set; }
        public List<InstrumentWeight> Weights { get; set; }
        // ... other properties ...
    }

    public class Override
    {
        public int Id { get; set; }
        public string Descriptor { get; set; }
        public string NewValue { get; set; }
        // ... other properties ...
    }

    public class InstrumentWeight
    {
        public int Id { get; set; }
        public int ElementId { get; set; }
        public Element Element { get; set; }
        public decimal Weight { get; set; }
        // ... other properties ...
    }

    public class Element
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Currency Currency { get; set; }
        // No longer holds Region directly, we'll use Country and Region relationship
        public ICollection<CountryRegion> CountryRegions { get; set; }
        public AssetClass AssetClass { get; set; }
        public Liquidity Liquidity { get; set; }
        public Maturity Maturity { get; set; }
        public Strategy Strategy { get; set; }
        public Sector Sector { get; set; }
        // ... other properties ...
    }

    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g., "United States", "United Kingdom"
        public ICollection<CountryRegion> CountryRegions { get; set; }
    }

    public class Region
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g., "Developed World", "Emerging Markets"
        public ICollection<CountryRegion> CountryRegions { get; set; }
    }

    // Join table for many-to-many relationship between Country and Region
    public class CountryRegion
    {
        public int CountryId { get; set; }
        public Country Country { get; set; }
        public int RegionId { get; set; }
        public Region Region { get; set; }
    }

    public class Currency
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g., "United States", "United Kingdom"
    }

    public class AssetClass
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g., "United States", "United Kingdom"

    }

    public class Liquidity
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g., "United States", "United Kingdom"
    }

    public class Maturity
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g., "United States", "United Kingdom"
    }

    public class Strategy
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g., "United States", "United Kingdom"
    }

    public class Sector
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g., "United States", "United Kingdom"
    }

    public class PortfolioGridModel
    {
        public Portfolio Portfolio { get; set; }
        public List<Investment> Investments { get; set; }
        public Dictionary<DateTime, List<PortfolioValuation>> ValuationsByDate { get; set; }
    }



}

