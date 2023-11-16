using NoahWeb_Private_Asset_Module.enums;

namespace NoahWeb_Private_Asset_Module.Models;

public class Transaction
{
    public int TransactionID { get; set; }
    public int AssetID { get; set; }
    public decimal Value { get; set; }
    public DateTime Date { get; set; }
    public string Currency { get; set; }  // ISO Code, default from Asset if not provided
    public bool Recallable { get; set; }  // default is False
    public TransactionType Type { get; set; }  // Enum: Commitment, CarryValue, Fee, CapitalCall, Distribution
    public CalculationType Calculation { get; set; }  // Enum: Add, Subtract, Set

    // Navigation properties
    public Asset Asset { get; set; }
}