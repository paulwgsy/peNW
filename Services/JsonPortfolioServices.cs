using System.IO;
using System.Collections.Generic;
using NoahWeb_Private_Asset_Module.Models;
using Newtonsoft.Json;

namespace NoahWeb_Private_Asset_Module.Services
{
    public static class JsonPortfolioService
    {
        public static List<Portfolio> GetPortfolios(string filePath)
        {
            var jsonString = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<Portfolio>>(jsonString) ?? new List<Portfolio>();
        }

        public static Portfolio GetPortfolioWithValuations(string filePath, int portfolioId)
        {
            var portfolios = GetPortfolios(filePath);
            return portfolios.Find(p => p.Id == portfolioId);
        }

        public static List<Investment> GetInvestments(string filePath)
        {
            // Read the JSON file
            string json = File.ReadAllText(filePath);
            // Deserialize the JSON to a List of Investments
            var investments = JsonConvert.DeserializeObject<List<Investment>>(json);
            return investments ?? new List<Investment>();
        }

        public static Investment GetInvestmentsbyId(string filePath, int investmentId)
        {
            // Read the JSON file
            string json = File.ReadAllText(filePath);
            // Deserialize the JSON to a List of Investments
            var investments = JsonConvert.DeserializeObject<List<Investment>>(json);
            // Find the specific investment by ID
            var investment = investments.FirstOrDefault(i => i.Id == investmentId);
            return investment;
        }
    }
}

