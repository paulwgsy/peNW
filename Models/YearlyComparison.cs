using System;
namespace NoahWeb_Private_Asset_Module.Models
{
    public class YearlyComparison
    {
        public int Year { get; set; }
        public decimal FundPeriodCapitalCall { get; set; }
        public decimal FundPeriodDistribution { get; set; }
        public decimal FundPeriodNCF { get; set; }
        public decimal FundCarryValue { get; set; }
        public decimal FundTotalCapitalCall { get; set; }
        public decimal FundTotalDistribution { get; set; }
        public decimal FundTotalNCF { get; set; }
        public decimal FundDPI { get; set; }
        public decimal FundRVPI { get; set; }
        public decimal FundTVPI { get; set; }

        public decimal ModelPeriodCapitalCall { get; set; }
        public decimal ModelPeriodDistribution { get; set; }
        public decimal ModelPeriodNCF { get; set; }
        public decimal ModelCarryValue { get; set; }
        public decimal ModelTotalCapitalCall { get; set; }
        public decimal ModelTotalDistribution { get; set; }
        public decimal ModelTotalNCF { get; set; }
        public decimal ModelDPI { get; set; }
        public decimal ModelRVPI { get; set; }
        public decimal ModelTVPI { get; set; }
    }
}

