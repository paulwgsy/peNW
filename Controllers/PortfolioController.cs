using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using NoahWeb_Private_Asset_Module.Models;
using NoahWeb_Private_Asset_Module.enums;
using CsvHelper;
using System.Globalization;
using NoahWeb_Private_Asset_Module.Services;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NoahWeb_Private_Asset_Module.Controllers
{
    public class PortfolioController : Controller
    {
        // INDEX PAGES



        private const string JsonPortfolioPath = "Portfolios.json"; // Update with the actual path
        private const string JsonInvestmentPath = "Investments.json"; // Update with the actual path

        public IActionResult Index()
        {
            var portfolios = JsonPortfolioService.GetPortfolios(JsonPortfolioPath);
            return View(portfolios);
        }

        /*public IActionResult ValuationView(int id)
        {
            var portfolio = JsonPortfolioService.GetPortfolioWithValuations(JsonPortfolioPath, id);
            if (portfolio == null)
            {
                return NotFound();
            }
            return View(portfolio);
        }*/

        public IActionResult ValuationView(int portfolioId)
        {
            // Load the portfolio and investments. This example assumes you're loading from JSON.
            var portfolio = JsonPortfolioService.GetPortfolioWithValuations(JsonPortfolioPath, portfolioId);
            List<Investment> investments = JsonPortfolioService.GetInvestments(JsonInvestmentPath);

            // Create a dictionary to organize valuations by date for easier access in the view.
            var valuationsByDate = new Dictionary<DateTime, List<PortfolioValuation>>();

            foreach (var valuation in portfolio.Valuations)
            {
                if (!valuationsByDate.ContainsKey(valuation.Date))
                {
                    valuationsByDate[valuation.Date] = new List<PortfolioValuation>();
                }
                valuationsByDate[valuation.Date].Add(valuation);
            }

            var model = new PortfolioGridModel
            {
                Portfolio = portfolio,
                Investments = investments,
                ValuationsByDate = valuationsByDate
            };

            return View(model);
        }

    }

}

