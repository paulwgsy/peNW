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
using System.Runtime.Intrinsics.Arm;
using Microsoft.AspNetCore.Mvc.Rendering;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NoahWeb_Private_Asset_Module.Controllers
{
    public class PrivateAssetModuleController : Controller
    {
        // INDEX PAGES
        public IActionResult Index()
        {
            return View();
        }

        // CREATE ASSET - Allows user to create a new asset and then displays the AssetSummary Page
        // First Page - FORM
        [HttpGet]
        public IActionResult CreateAsset()
        {
            ViewBag.Assets = LoadAssetsFromTextFile();
            return View();
        }

        // This page receives the FORM and then sends the user to the AssetSummary
        [HttpPost]
        public IActionResult CreateAsset(Asset asset)
        {
            // Get the next ID and assign it to the asset
            asset.AssetID = GetNextAssetId("Assets.txt");


            // For now, save to a text file
            // This needs to be modified to write to the database
            string filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets.txt");
            System.IO.File.AppendAllText(filePath, JsonConvert.SerializeObject(asset) + Environment.NewLine);

            return RedirectToAction("AssetSummary", new { assetId = asset.AssetID });  // Redirect back to landing page or a confirmation page
        }

        public IActionResult CreatePrivateAssetPortfolio()
        {
            ViewBag.Assets = LoadAssetsFromTextFile();
            return View();
        }

        [HttpPost]
        public IActionResult CreatePrivateAssetPortfolio(PrivateAssetPortfolio portfolio)
        {
            portfolio.PrivateAssetPortfolioID = GetNextAssetPortfolioId("AssetPortfolios.txt");

            // Save to a text file
            string filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "AssetPortfolios.txt");
            System.IO.File.AppendAllText(filePath, JsonConvert.SerializeObject(portfolio) + Environment.NewLine);

            return RedirectToAction("PrivateAssetPortfolioDetails", new { portfolioId = portfolio.PrivateAssetPortfolioID });
        }


        // Once a new Asset has been created, the user is sent here
        [HttpGet]
        public IActionResult AssetSummary(int assetId)
        {
            // Retrieve the asset details using the asset ID.
            // For the demonstration purpose, I'll just read from the Assets.txt
            var assets = System.IO.File.ReadAllLines(Path.Combine(_hostingEnvironment.ContentRootPath, "Assets.txt"))
                            .Select(line => JsonConvert.DeserializeObject<Asset>(line))
                            .ToList();

            var asset = assets.FirstOrDefault(a => a.AssetID == assetId);
            if (asset == null)
            {
                return NotFound("Asset not found");
            }

            return View(asset);
        }

        public IActionResult PrivateAssetPortfolioDetails(int portfolioId)
        {
            var portfolioFilePath = Path.Combine(_hostingEnvironment.ContentRootPath, "AssetPortfolios.txt");
            var portfolioList = System.IO.File.ReadAllLines(portfolioFilePath)
                                               .Select(line => JsonConvert.DeserializeObject<PrivateAssetPortfolio>(line))
                                               .ToList();

            var portfolio = portfolioList.FirstOrDefault(p => p.PrivateAssetPortfolioID == portfolioId);

            if (portfolio == null)
            {
                return NotFound(); // Or some other appropriate action
            }

            // Load assets from Assets.txt
            var assetsFilePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets.txt");
            var allAssets = System.IO.File.ReadAllLines(assetsFilePath)
                                           .Select(line => JsonConvert.DeserializeObject<Asset>(line))
                                           .ToList();

            var portfolioAssetsDetails = allAssets.Where(asset => portfolio.AssetIDs.Contains(asset.AssetID)).ToList();
            portfolio.Assets = portfolioAssetsDetails; // Assuming Assets is now a list of Asset objects

            return View(portfolio);
        }

        public IActionResult PrivateAssetPortfolioValuation()
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "AssetPortfolios.txt");
            var portfolios = System.IO.File.ReadAllLines(filePath)
                                            .Select(line => JsonConvert.DeserializeObject<PrivateAssetPortfolio>(line))
                                            .ToList();

            ViewBag.Portfolios = new SelectList(portfolios, "PrivateAssetPortfolioID", "PortfolioName");
            return View();
        }

        [HttpPost]
        public IActionResult PrivateAssetPortfolioValuation(int privateAssetPortfolioId, DateTime endDate)
        {
            DateTime startDate = new DateTime(1990, 1, 1);

            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "AssetPortfolios.txt");
            var allPortfolios = System.IO.File.ReadAllLines(filePath)
                                               .Select(line => JsonConvert.DeserializeObject<PrivateAssetPortfolio>(line))
                                               .ToList();

            ViewBag.portfolios = new SelectList(allPortfolios, "PrivateAssetPortfolioID", "PortfolioName");

            var portfolio = allPortfolios.FirstOrDefault(p => p.PrivateAssetPortfolioID == privateAssetPortfolioId);

            if (portfolio == null)
            {
                // Handle the case where the portfolio is not found
                return NotFound();
            }

            var assetsFilePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets.txt");
            var allAssets = System.IO.File.ReadAllLines(assetsFilePath)
                                           .Select(line => JsonConvert.DeserializeObject<Asset>(line))
                                           .ToList();

            // Populate the Assets property based on AssetIDs
            portfolio.Assets = allAssets.Where(asset => portfolio.AssetIDs.Contains(asset.AssetID)).ToList();

            var combinedAssetViews = new List<AssetView>();

            foreach (var asset in portfolio.Assets)
            {
                // Call PrivateAssetViewBuild for each asset
                var assetResult = PrivateAssetViewBuild(asset.AssetID, startDate, endDate) as ViewResult;
                var assetViews = assetResult?.Model as List<AssetView>;
                if (assetViews != null)
                {
                    combinedAssetViews.AddRange(assetViews);
                }
            }

            return View(combinedAssetViews);
        }



        // A page to display the list of all assets. This would ideally be controlled by the client or
        // if at an admin level, the user may be able to see all assets in the system
        // models may be restricted to certain levels of users

        public IActionResult AssetOverview()
        {
            var assets = ViewBag.Assets = LoadAssetsFromTextFile();

            return View(assets); // Pass the list of assets to the view.
        }

        [HttpGet]
        public IActionResult EditAsset(int id)
        {
            var assets = ViewBag.Assets = LoadAssetsFromTextFile();
            var asset = LoadAssetByIdFromTextFile(id); // Replace with your database retrieval method.

            if (asset == null)
            {
                // Handle this gracefully - e.g., display an error message or redirect.
                return NotFound();
            }

            return View(asset);
        }

        //EDIT the Asset

        [HttpPost]
        public IActionResult EditAsset(Asset asset)
        {

                DeleteAssetFromTextFile(asset.AssetID); // Remove the old record.
                AppendAssetToTextFile(asset);           // Add the updated one.

                return RedirectToAction("AssetOverview");
        }

        //DELETE the Asset
        [HttpPost]
        public IActionResult DeleteAsset(int id)
        {
            try
            {
                DeleteAssetFromTextFile(id);
                DeleteRelatedTransactions(id);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        //Page to add a new transaction
        public IActionResult AddTransaction()
        {
            ViewBag.Assets = LoadAssetsFromTextFile(); // Implement this method to read assets from the text file.
            return View();
        }

        [HttpPost]
        public IActionResult AddTransaction(Transaction transaction)
        {
            // Assign a TransactionID (similar to how we assigned AssetID previously)
            transaction.TransactionID = GetNextTransactionId("Transactions.txt");

            // Save to text file
            string filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Transactions.txt");
            System.IO.File.AppendAllText(filePath, JsonConvert.SerializeObject(transaction) + Environment.NewLine);

            // Redirect to a confirmation page showing all transactions for the specific asset in date order
            return RedirectToAction("TransactionSummary", new { assetId = transaction.AssetID });
        }

        // here is a method that adds multiple transactions at once by leveraging the above code. 
        [HttpPost]
        public void AddTransactions(List<Transaction> transactions)
        {
            // Save to text file
            string filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Transactions.txt");

            // Loop through each transaction and append to the file
            foreach (var transaction in transactions)
            {
                transaction.TransactionID = GetNextTransactionId("Transactions.txt");
                System.IO.File.AppendAllText(filePath, JsonConvert.SerializeObject(transaction) + Environment.NewLine);
            }
        }


        //Load the CSV File from the AddTransactionsFile
        // We may want to amend this so that we can load files for other areas.

        public IActionResult AddTransactionsFile()
        {
            ViewBag.Assets = LoadAssetsFromTextFile();  // Implement this function to load all assets from your storage
            return View();
        }


        [HttpPost]
        public IActionResult UploadCsv(IFormFile file, int assetId)
        {
            // Check if the file is present
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No file uploaded or file is empty" });
            }

            // Check the file size (e.g., 10 MB limit)
            long maxSize = 10 * 1024 * 1024;
            if (file.Length > maxSize)
            {
                return BadRequest(new { error = "File size exceeds limit" });
            }

            // Check the file extension
            var allowedExtensions = new[] { ".csv" };
            var extension = Path.GetExtension(file.FileName);
            if (!allowedExtensions.Contains(extension.ToLower()))
            {
                return BadRequest(new { error = "Invalid file format" });
            }

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                // Read all records (you might limit this to a specific number for the sake of a preview)
                var records = csv.GetRecords<dynamic>().ToList();

                // Return the first few rows for preview (e.g., 5 rows)
                return Json(records.Take(5));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error processing CSV file", details = ex.Message });
            }
        }

        //Then we need to map the data to the relevant columns in the data model
        //in this instance we're mapping for transactions

        [HttpPost]
        public IActionResult MapAndSaveData(int AssetID, IFormFile file, Dictionary<string, string> mappings)
        {
            //Check the file is uplaoded properly
            if (file == null)
            {
                // Handle error - maybe return an error response or throw a specific exception
                throw new Exception("No file uploaded.");
            }

            // Load the CSV data from the uploaded file
            using var reader = new StreamReader(file.OpenReadStream());
            var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csvReader.GetRecords<dynamic>().ToList();

            // Create a list to hold our transactions
            List<Transaction> transactions = new List<Transaction>();


            List<string> errorMessages = new List<string>();

            int rowIndex = 1;
            // For each record in the CSV
            foreach (var record in records)
            {
                Transaction transaction = new Transaction
                {
                    AssetID = AssetID
                };

                foreach (var key in mappings.Keys)
                {
                    // Skip the column if it's set to "discard"
                    if (mappings[key] == "Discard" || mappings[key] == "")
                        continue;

                    var recordDictionary = record as IDictionary<string, object>;
                    if (recordDictionary == null || !recordDictionary.ContainsKey(key))
                        continue; // if key doesn't exist in the record, skip this iteration

                    var valueofitem = recordDictionary[key];

                    switch (mappings[key])
                    {
                        case "Value":
                            if (decimal.TryParse(valueofitem.ToString(), out decimal parsedValue))
                            {
                                transaction.Value = parsedValue;
                            }
                            else
                            {
                                errorMessages.Add($"Invalid value for 'Value' in row {rowIndex} and column {key}");
                            }
                            break;
                        case "Date":
                            if (DateTime.TryParseExact(valueofitem.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                            {
                                transaction.Date = parsedDate;
                            }
                            else
                            {
                                errorMessages.Add($"Invalid date format in row {rowIndex} and column {key}");
                            }
                            break;
                        case "Currency":
                            string currencyCode = valueofitem.ToString().ToUpper();
                            if (currencyCode.Length == 3 && GlobalConstants.ValidISOCurrencyCodes.Contains(currencyCode))
                            {
                                transaction.Currency = currencyCode;
                            }
                            else
                            {
                                errorMessages.Add($"Invalid currency code '{currencyCode}' in row {rowIndex} and column {key}. Must be a 3-letter ISO code.");
                            }
                            break;
                        case "Type":
                            if (Enum.TryParse(valueofitem.ToString(), out TransactionType parsedTransactionType))
                            {
                                transaction.Type = parsedTransactionType;
                            }
                            else
                            {
                                errorMessages.Add($"Invalid Transaction Type in row {rowIndex} and column {key}. Must be Commitment, CapitalCall, Distribution or CarryValue.");
                            }
                            break;
                        case "Calculation":
                            if (Enum.TryParse(valueofitem.ToString(), out CalculationType parsedCalculationType))
                            {
                                transaction.Calculation = parsedCalculationType;
                            }
                            else
                            {
                                errorMessages.Add($"Invalid Calculation Type in row {rowIndex} and column {key}. Must be Set, Add or Subtract.");
                            }
                            break;
                            // Add other cases as necessary
                    }
                }

                rowIndex++;
                transactions.Add(transaction);
            }

            if (errorMessages.Any())
            {
                TempData["ErrorMessages"] = string.Join("<br>", errorMessages);
                return RedirectToAction("TransactionFileError");
            }

            // Use the AddTransactions method to save all transactions to the file
            AddTransactions(transactions);

            // Redirect to a confirmation page showing all transactions for the specific asset in date order
            return RedirectToAction("TransactionSummary", new { assetId = AssetID });
        }

        public IActionResult TransactionFileError()
        {
            return View();
        }


        public IActionResult TransactionSummary(int assetId)
        {
            var transactions = LoadTransactionsForAsset(assetId); // Implement this to load transactions from text file
            return View(transactions);
        }

        public IActionResult AssetViewIndex()
        {
            // Load the list of assets (e.g., from a database, service, or file)
            List<Asset> assets = LoadAssetsFromTextFile();

            return View(assets);
        }

        // AssetView shows the total summary for an asset between two given dates
        public IActionResult AssetView(int AssetID)
        {
            // Get transactions for the asset
            var transactions = LoadTransactionsForAsset(AssetID);

            // If there are transactions, set the StartDate as the date of the first transaction. 
            // Otherwise, default to "2000-01-01".
            DateTime startDate = transactions.Any()
                ? transactions.Min(t => t.Date)
                : new DateTime(2000, 1, 1);

            // Set the end date as "2023-06-30".
            DateTime endDate = new DateTime(2023, 6, 30);

            // Use PrivateAssetViewBuild method to generate the view model
            return PrivateAssetViewBuild(AssetID, startDate, endDate);
        }


        // Read in the assets from the text file
        // this will need to be replaced with access to the database when moved over
        private Asset LoadAssetByIdFromTextFile(int assetId)
        {
            var allAssets = LoadAssetsFromTextFile();

            return allAssets.FirstOrDefault(asset => asset.AssetID == assetId);
        }

        private List<Asset> LoadAssetsFromTextFile()
        {
            string filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets.txt");

            if (!System.IO.File.Exists(filePath))
                return new List<Asset>(); // return empty list if file does not exist

            var assetLines = System.IO.File.ReadAllLines(filePath);
            var assets = assetLines.Select(line => JsonConvert.DeserializeObject<Asset>(line)).ToList();

            return assets;
        }

        // This will load all the transactions for a given asset from the transactions file
        // replace with database call when needed

        private List<Transaction> LoadTransactionsForAsset(int assetId)
        {
            string filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Transactions.txt");

            if (!System.IO.File.Exists(filePath))
                return new List<Transaction>(); // return empty list if file does not exist

            var transactionLines = System.IO.File.ReadAllLines(filePath);
            var transactions = transactionLines.Select(line => JsonConvert.DeserializeObject<Transaction>(line)).ToList();

            // Filter transactions for the specified asset
            return transactions.Where(t => t.AssetID == assetId).ToList();
        }


        [HttpGet]
        public IActionResult ValuationSummary(DateTime date)
        {
            DateTime minDate = new DateTime(1990, 1, 1);

            // If a date is not provided or is less than the minimum, use the minimum date
            DateTime effectiveDate = date;
            if (effectiveDate < minDate)
            {
                effectiveDate = minDate;
            }

            ViewBag.SelectedDate = effectiveDate;

            List<Asset> assets = LoadAssetsFromTextFile();

            var viewModelList = new List<AssetView>();
            foreach (var asset in assets)
            {
                var valuation = AssessTransactionsForAsset(asset.AssetID, effectiveDate);

                // Assuming valuation has properties that match AssetSummary.
                var assetSummary = ViewData["AssetSummary"] as AssetSummary;

                var viewModel = new AssetView
                {
                    ValuationDate = effectiveDate,
                    AssetID = asset.AssetID,
                    FundName = asset.FundName,
                    SpecifiedDate = assetSummary.SpecifiedDate,
                    ContributionTotal = assetSummary.ContributionTotal,
                    DistributionTotal = assetSummary.DistributionTotal,
                    NetCashFlowTotal = assetSummary.NetCashFlowTotal,
                    CarryValue = assetSummary.CarryValue,
                    DPI = assetSummary.DPI,
                    RVPI = assetSummary.RVPI,
                    TVPI = assetSummary.TVPI
                };
                viewModelList.Add(viewModel);
            }

            return View(viewModelList);
        }

        //ASSET VIEW Models in USE HERE

        //AssessTransactionsForAsset runs through all the transactions for an asset up to a specified date to build
        //the state of the asset at that point in time

        public IActionResult AssessTransactionsForAsset(int assetID, DateTime specifiedDate)
        {
            //get the asset details:
            var assetdetails = LoadAssetByIdFromTextFile(assetID);

            var transactionTypeOrder = new Dictionary<TransactionType, int>
            {
                { TransactionType.Commitment, 1 },
                { TransactionType.CapitalCall, 2 },
                { TransactionType.Distribution, 3 },
                { TransactionType.CarryValue, 4 },

            };

            var transactionsForAsset = LoadTransactionsForAsset(assetID)
                .Where(t => t.AssetID == assetID && t.Date <= specifiedDate)
                .OrderBy(t => t.Date)
                .ThenBy(t => transactionTypeOrder[t.Type])
                .ToList();

            // Initialize variables for tracking totals
            decimal commitment = 0;
            decimal contributionTotal = 0;
            decimal distributionTotal = 0;
            decimal carryvalueTotal = 0;

            foreach (var transaction in transactionsForAsset)
            {
                // Determine the type of transaction (e.g., 'add', 'subtract', 'set')
                switch (transaction.Type)
                {
                    case TransactionType.Commitment:
                        if (transaction.Calculation == CalculationType.Add)
                        {
                            commitment += transaction.Value;
                        }
                        else if (transaction.Calculation == CalculationType.Subtract)
                        {
                            commitment -= transaction.Value;
                        }
                        else if (transaction.Calculation == CalculationType.Set)
                        {
                            commitment = transaction.Value;
                        }
                        break;
                    case TransactionType.CapitalCall:
                        if (transaction.Calculation == CalculationType.Add)
                        {
                            contributionTotal += transaction.Value;
                            carryvalueTotal += transaction.Value;
                        }
                        else if (transaction.Calculation == CalculationType.Subtract)
                        {
                            contributionTotal -= transaction.Value;
                            carryvalueTotal -= transaction.Value;
                        }
                        else if (transaction.Calculation == CalculationType.Set)
                        {
                            carryvalueTotal += transaction.Value - contributionTotal;
                            contributionTotal = transaction.Value;
                        }
                        break;
                    case TransactionType.Distribution:
                        if (transaction.Calculation == CalculationType.Add)
                        {
                            distributionTotal += transaction.Value;
                            carryvalueTotal -= transaction.Value;
                        }
                        else if (transaction.Calculation == CalculationType.Subtract)
                        {
                            distributionTotal -= transaction.Value;
                            carryvalueTotal += transaction.Value;
                        }
                        else if (transaction.Calculation == CalculationType.Set)
                        {
                            carryvalueTotal -= transaction.Value - distributionTotal;
                            distributionTotal = transaction.Value;
                        }
                        break;
                    // You can handle other types of transactions here
                    case TransactionType.CarryValue:
                        if (transaction.Calculation == CalculationType.Add)
                            carryvalueTotal += transaction.Value;
                        else if (transaction.Calculation == CalculationType.Subtract)
                            carryvalueTotal -= transaction.Value;
                        else if (transaction.Calculation == CalculationType.Set)
                            carryvalueTotal = transaction.Value;
                        break;
                }
            }

            // Calculate the current total value for the specified asset
            decimal NetCashFlow = distributionTotal - contributionTotal;

            // set the denominator to 1 to avoid errors if there are no contributions
            decimal denom = 0;
            if (contributionTotal == 0)
                denom = 1;
            else
                denom = contributionTotal;


            AssetSummary assetSummary = new AssetSummary
            {
                AssetID = assetID,
                FundName = assetdetails.FundName,
                Vintage = assetdetails.Vintage,
                SpecifiedDate = specifiedDate,
                Commitment = commitment,
                ContributionTotal = contributionTotal,
                DistributionTotal = distributionTotal,
                NetCashFlowTotal = NetCashFlow,
                CarryValue = carryvalueTotal,
                DPI = distributionTotal / denom,
                RVPI = carryvalueTotal / denom,
                TVPI = (distributionTotal + carryvalueTotal) / denom

            };

            ViewData["AssetSummary"] = assetSummary;

            return View(transactionsForAsset);
        }

        public IActionResult PrivateAssetViewBuild(int AssetID, DateTime startDate, DateTime endDate)
        {
            var viewModelList = new List<AssetView>();

            for (var month = startDate; month <= endDate; month = month.AddMonths(1))
            {
                var transactionsForMonth = AssessTransactionsForAsset(AssetID, new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month)));
                var assetSummary = ViewData["AssetSummary"] as AssetSummary;

                var viewModel = new AssetView
                {
                    ValuationDate = month,
                    AssetID = assetSummary.AssetID,
                    FundName = assetSummary.FundName,
                    Vintage = assetSummary.Vintage,
                    SpecifiedDate = assetSummary.SpecifiedDate,
                    Commitment = assetSummary.Commitment,
                    ContributionTotal = assetSummary.ContributionTotal,
                    DistributionTotal = assetSummary.DistributionTotal,
                    NetCashFlowTotal = assetSummary.NetCashFlowTotal,
                    CarryValue = assetSummary.CarryValue,
                    DPI = assetSummary.DPI,
                    RVPI = assetSummary.RVPI,
                    TVPI = assetSummary.TVPI
                };
                viewModelList.Add(viewModel);
            }

            return View(viewModelList);
        }

        public IActionResult ComparePEModelToFund()
        {
            var assets = LoadAssetsFromTextFile();

            // Fetch lists of funds and PEModels.
            var funds = assets.Where(a => a.Type == AssetType.Fund).ToList();
            var peModels = assets.Where(a => a.Type == AssetType.Model).ToList();

            ViewBag.Funds = funds;
            ViewBag.PEModels = peModels;

            return View();
        }

        [HttpPost]
        public IActionResult GenerateComparison(int fundId, int peModelId)
        {
            var assets = LoadAssetsFromTextFile();

            var fund = assets.FirstOrDefault(a => a.AssetID == fundId);
            var peModel = assets.FirstOrDefault(a => a.AssetID == peModelId);

            if (fund == null || peModel == null)
            {
                // Handle this error gracefully. Maybe redirect with an error message.
                return RedirectToAction("ComparePEModelToFund");
            }

            var fundTransactions = LoadTransactionsForAsset(fundId);
            var earliestCommitment = fundTransactions
                                         .Where(t => t.Type == TransactionType.Commitment)
                                         .OrderBy(t => t.Date)
                                         .FirstOrDefault();

            if (earliestCommitment == null)
            {
                // Handle this error. No commitments found for the fund.
                return RedirectToAction("ComparePEModelToFund");
            }

            DateTime fundStartDate = earliestCommitment.Date;

            var modelTransactions = AdjustAndScalePEModelTransactions(fund, fundStartDate, peModelId);

            // This will contain the adjusted and scaled transactions. 
            // Pass it to the view for rendering in the table.

            var viewModel = new FundAndModelComparisonViewModel
            {
                FundTransactions = fundTransactions,
                ModelTransactions = modelTransactions
            };

            var years = DateTime.Now.Year - fundStartDate.Year;
            List<YearlyComparison> comparisons = new List<YearlyComparison>();

            decimal comm = earliestCommitment.Value;
            decimal fCapitalCall = 0;
            decimal mCapitalCall = 0;
            decimal fDistribution = 0;
            decimal mDistribution = 0;
            decimal fpCapitalCall = 0;
            decimal mpCapitalCall = 0;
            decimal fpDistribution = 0;
            decimal mpDistribution = 0;
            decimal fcarryvalue = 0;
            decimal mcarryvalue = 0;

            for (int i = 0; i <= years; i++)
            {
                var startYearDate = fundStartDate.AddYears(i);
                var endYearDate = fundStartDate.AddYears(i + 1);

                

                fpCapitalCall = SumForPeriod(fundTransactions, startYearDate, endYearDate, TransactionType.CapitalCall);
                fCapitalCall = fCapitalCall + fpCapitalCall;
                fpDistribution = SumForPeriod(fundTransactions, startYearDate, endYearDate, TransactionType.Distribution);
                fDistribution = fDistribution + fpDistribution;

                fcarryvalue = ComputeModelCarryValueForPeriod(fundTransactions, startYearDate, endYearDate, fcarryvalue);

                mpCapitalCall = SumForPeriod(modelTransactions, startYearDate, endYearDate, TransactionType.CapitalCall);
                mCapitalCall = mCapitalCall + mpCapitalCall;
                mpDistribution = SumForPeriod(modelTransactions, startYearDate, endYearDate, TransactionType.Distribution);
                mDistribution = mDistribution + mpDistribution;

                mcarryvalue = ComputeModelCarryValueForPeriod(modelTransactions, startYearDate, endYearDate, mcarryvalue);

                decimal fdenom = 0;
                if (fCapitalCall == 0)
                    fdenom = 1;
                else
                    fdenom = fCapitalCall;

                decimal mdenom = 0;
                if (mCapitalCall == 0)
                    mdenom = 1;
                else
                    mdenom = mCapitalCall;

                var yearlyComparison = new YearlyComparison
                {
                    Year = startYearDate.Year,
                    FundPeriodCapitalCall = fpCapitalCall,
                    FundPeriodDistribution = fpDistribution,
                    FundCarryValue = fcarryvalue,
                    FundPeriodNCF = fpCapitalCall - fpDistribution,
                    FundTotalCapitalCall = fCapitalCall,
                    FundTotalDistribution = fDistribution,
                    FundTotalNCF = fCapitalCall - fDistribution,
                    FundDPI = fDistribution / fdenom,
                    FundRVPI = fcarryvalue / fdenom,
                    FundTVPI = (fcarryvalue + fDistribution) / fdenom,

                    // ... [Repeat for other columns]
                    ModelPeriodCapitalCall = mpCapitalCall,
                    ModelPeriodDistribution = mpDistribution,
                    ModelCarryValue = mcarryvalue,
                    ModelPeriodNCF = mpCapitalCall - mpDistribution,
                    ModelTotalCapitalCall = mCapitalCall,
                    ModelTotalDistribution = mDistribution,
                    ModelTotalNCF = mCapitalCall - mDistribution,
                    ModelDPI = mDistribution / mdenom,
                    ModelRVPI = mcarryvalue / mdenom,
                    ModelTVPI = (mcarryvalue + mDistribution) / mdenom
                    // ... [Repeat for other columns]
                };

                comparisons.Add(yearlyComparison);
            }

            return View("ComparisonView", comparisons);
        }



        private List<Transaction> AdjustAndScalePEModelTransactions(Asset fund, DateTime fundStartDate, int peModel)
        {
            // Calculate the full years difference, rounding down
            //int differenceInYears = (fundStartDate.Year - new DateTime(1990, 12, 31).Year)-1;

            DateTime referenceDate = new DateTime(1990, 12, 31);

            // Calculate the difference in years and months
            int years = fundStartDate.Year - referenceDate.Year;
            int months = fundStartDate.Month - referenceDate.Month;

            // Combine the year and month differences to get total difference in months
            int differenceInMonths = (years * 12) + months;

            var adjustedTransactions = new List<Transaction>();

            var modelTransactions = LoadTransactionsForAsset(peModel);
            var fundTransactions = LoadTransactionsForAsset(fund.AssetID);

            // Identify the highest commitment for each asset
            decimal modelCommitment = modelTransactions
                .Where(t => t.Type == TransactionType.Commitment)
                .Max(t => t.Value);

            decimal fundCommitment = fundTransactions
                .Where(t => t.Type == TransactionType.Commitment)
                .Max(t => t.Value);

            foreach (var transaction in modelTransactions)
            {
                var adjustedTransaction = new Transaction
                {
                    // Add the full years difference to the transaction date
                    Date = transaction.Date.AddMonths(differenceInMonths),
                    Value = (transaction.Value / modelCommitment) * fundCommitment,
                    Type = transaction.Type, // Including the type in case you need it in the future.
                    Calculation = transaction.Calculation,
                    Recallable = transaction.Recallable,
                    Currency = transaction.Currency

                };

                adjustedTransactions.Add(adjustedTransaction);
            }

            return adjustedTransactions;
        }



        // Controls used on the pages above

        private readonly IWebHostEnvironment _hostingEnvironment;

        public PrivateAssetModuleController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        //ASSET RELATED CONTROLS

        // Deletes the asset with the given ID from the text file.
        private void DeleteAssetFromTextFile(int assetId)
        {
            DeleteLinesFromFile("Assets.txt", assetId);
        }

        private void DeleteRelatedTransactions(int assetId)
        {
            // Assuming that transactions.txt follows a similar JSON structure to assets.txt
            DeleteLinesFromFile("Transactions.txt", assetId, isTransactionFile: true);
        }

        private void DeleteLinesFromFile(string filePath, int assetId, bool isTransactionFile = false)
        {
            var allLines = System.IO.File.ReadAllLines(filePath).ToList();
            var linesToRemove = allLines.Where(line =>
            {
                try
                {
                    var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(line);
                    // Check if the line contains the AssetID or Transaction's AssetID
                    int id = isTransactionFile ? jsonObj["AssetID"].ToObject<int>() : jsonObj["AssetID"].ToObject<int>();
                    return id == assetId;
                }
                catch (Exception ex)
                {
                    // Handle or log the exception if needed
                    Console.WriteLine($"Error parsing line as JSON: {ex.Message}");
                    return false;
                }
            }).ToList();

            foreach (var lineToRemove in linesToRemove)
            {
                allLines.Remove(lineToRemove);
            }

            System.IO.File.WriteAllLines(filePath, allLines);
        }

        /*private void DeleteAssetFromTextFile(int assetId)
        {
            var filePath = "Assets.txt";
            var allLines = System.IO.File.ReadAllLines(filePath).ToList();

            var linesToRemove = allLines.Where(line =>
            {
                // Try to parse the line as JSON
                try
                {
                    var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(line);
                    return jsonObj["AssetID"] != null && jsonObj["AssetID"].ToObject<int>() == assetId;
                }
                catch (Exception ex)
                {
                    // Handle or log the exception if needed
                    Console.WriteLine($"Error parsing line as JSON: {ex.Message}");
                    return false;
                }
            }).ToList();

            // Remove the lines that match the assetId
            foreach (var lineToRemove in linesToRemove)
            {
                allLines.Remove(lineToRemove);
            }

            // Write the remaining lines back to the file
            System.IO.File.WriteAllLines(filePath, allLines);
        }*/

        // Appends the provided asset to the text file.
        private void AppendAssetToTextFile(Asset asset)
        {
            string filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets.txt");
            System.IO.File.AppendAllText(filePath, JsonConvert.SerializeObject(asset) + Environment.NewLine);
        }

        private int GetNextAssetId(string fileName)
        {
            string filePath = Path.Combine(_hostingEnvironment.ContentRootPath, fileName);

            // Check if file exists; if not, return 1 as the first ID.
            if (!System.IO.File.Exists(filePath))
            {
                return 1;
            }

            // Read all lines from the file.
            var allLines = System.IO.File.ReadAllLines(filePath);

            // If the file is empty, return 1.
            if (allLines.Length == 0)
            {
                return 1;
            }

            // Retrieve the AssetID of the last line (assuming the last line has the highest ID).
            int highestId = 0;

            // Iterate through each line to find the highest AssetID.
            foreach (var line in allLines)
            {
                var asset = JsonConvert.DeserializeObject<Asset>(line);
                if (asset.AssetID > highestId)
                {
                    highestId = asset.AssetID;
                }
            }

            // Return the next available ID.
            return highestId + 1;
        }

        private int GetNextAssetPortfolioId(string fileName)
        {
            string filePath = Path.Combine(_hostingEnvironment.ContentRootPath, fileName);

            // Check if file exists; if not, return 1 as the first ID.
            if (!System.IO.File.Exists(filePath))
            {
                return 1;
            }

            // Read all lines from the file.
            var allLines = System.IO.File.ReadAllLines(filePath);

            // If the file is empty, return 1.
            if (allLines.Length == 0)
            {
                return 1;
            }

            // Retrieve the AssetID of the last line (assuming the last line has the highest ID).
            int highestId = 0;

            // Iterate through each line to find the highest AssetID.
            foreach (var line in allLines)
            {
                var privateAssetPortfolio = JsonConvert.DeserializeObject<PrivateAssetPortfolio>(line);
                if (privateAssetPortfolio.PrivateAssetPortfolioID > highestId)
                {
                    highestId = privateAssetPortfolio.PrivateAssetPortfolioID;
                }
            }

            // Return the next available ID.
            return highestId + 1;
        }

        private int GetNextTransactionId(string fileName)
        {
            string filePath = Path.Combine(_hostingEnvironment.ContentRootPath, fileName);

            // Check if file exists; if not, return 1 as the first ID.
            if (!System.IO.File.Exists(filePath))
            {
                return 1;
            }

            // Read all lines from the file.
            var allLines = System.IO.File.ReadAllLines(filePath);

            // If the file is empty, return 1.
            if (allLines.Length == 0)
            {
                return 1;
            }

            // Retrieve the TransactionID of the last line (assuming the last line has the highest ID).
            int highestId = 0;

            // Iterate through each line to find the highest TransactionID.
            foreach (var line in allLines)
            {
                var transaction = JsonConvert.DeserializeObject<Transaction>(line);
                if (transaction.TransactionID > highestId)
                {
                    highestId = transaction.TransactionID;
                }
            }

            // Return the next available ID.
            return highestId + 1;
        }


        //Helper method to calculate the CarryValue at any given time. This is needed when not doing a rollup claculation in the
        //AssessTransactionsForAsset
        private decimal ComputeModelCarryValueForPeriod(List<Transaction> transactions, DateTime start, DateTime end, decimal previousCarryValue)
        {
            var periodTransactions = transactions
                .Where(t => t.Date >= start && t.Date <= end)
                .OrderBy(t => t.Date)
                .ToList();


            // First, check if there's a 'Set' transaction at the end of the period.
            var lastSetTransaction = periodTransactions
                .LastOrDefault(t => t.Type == TransactionType.CarryValue && t.Calculation == CalculationType.Set);

            decimal carryValue;

            // If a 'Set' transaction exists, return its value immediately, overriding the calculated carry value.
            if (lastSetTransaction != null)
            {
                carryValue = lastSetTransaction.Value;
                // Get all transactions after the last 'Set' transaction to adjust the carry value accordingly.
                periodTransactions = periodTransactions
                    .Where(t => t.Date > lastSetTransaction.Date)
                    .ToList();
            }
            else
            {
                carryValue = previousCarryValue;
            }

            // Additions based on Type.Add CapitalCall and Type.Subtract Distribution
            carryValue += periodTransactions
                .Where(t => t.Type == TransactionType.CapitalCall && t.Calculation == CalculationType.Add)
                .Sum(t => t.Value);

            carryValue += periodTransactions
                .Where(t => t.Type == TransactionType.Distribution && t.Calculation == CalculationType.Subtract)
                .Sum(t => t.Value);

            // Subtractions based on Type.Subtract CapitalCall and Type.Add Distribution
            carryValue -= periodTransactions
                .Where(t => t.Type == TransactionType.CapitalCall && t.Calculation == CalculationType.Subtract)
                .Sum(t => t.Value);

            carryValue -= periodTransactions
                .Where(t => t.Type == TransactionType.Distribution && t.Calculation == CalculationType.Add)
                .Sum(t => t.Value);

            return carryValue;
        }

        private decimal SumForPeriod(List<Transaction> transactions, DateTime start, DateTime end, TransactionType type)
        {
            var periodTransactions = transactions
                .Where(t => t.Type == type && t.Date > start && t.Date <= end)
                .ToList();

            decimal totalValue = 0;

            foreach (var transaction in periodTransactions)
            {
                if (transaction.Calculation == CalculationType.Add)
                {
                    totalValue += transaction.Value;
                }
                else if (transaction.Calculation == CalculationType.Subtract)
                {
                    totalValue -= transaction.Value;
                }
                // Optionally handle other types if you expand the Calculation enum in the future.
            }

            return totalValue;
        }

        private DateTime AdjustDateForLeapYears(DateTime originalDate, int yearAdjustment)
        {
            int year = originalDate.Year + yearAdjustment;
            int month = originalDate.Month;
            int day = originalDate.Day;

            // If the original date was Feb 29th and the new year is not a leap year, adjust to Feb 28.
            if (month == 2 && day == 29 && !DateTime.IsLeapYear(year))
            {
                day = 28;
            }

            // Construct the new date, it will automatically adjust for months with less than 31 days.
            DateTime adjustedDate = new DateTime(year, month, day);

            // Additional logic can be added here if necessary to handle other edge cases.

            return adjustedDate;
        }

    }
}

