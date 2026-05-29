using System;

namespace BankCore.Models
{
    public class Transaction : BankEntity
    {
        public int AccountID { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public Transaction() { }

        public Transaction(int accountId, TransactionType type, decimal amount, string description)
        {
            AccountID = accountId;
            Type = type;
            Amount = amount;
            Description = description;
        }

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Type}: {Amount:N2} L.E. - {Description}";
        }
    }
}
