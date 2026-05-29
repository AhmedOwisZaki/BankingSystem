using System;

namespace BankCore.Models
{
    public class Certificate : BankEntity
    {
        public int CustomerID { get; set; }
        private decimal _price;
        private int _period; // in years

        public decimal Price
        {
            get => _price;
            set
            {
                if (value < 1000)
                    throw new ArgumentException("Price must be at least 1000 L.E.");
                if (value % 1000 != 0)
                    throw new ArgumentException("Price must be a multiple of 1000 L.E.");
                _price = value;
            }
        }

        public int Period
        {
            get => _period;
            set
            {
                if (value != 1 && value != 3 && value != 5)
                    throw new ArgumentException("Period must be 1, 3, or 5 years.");
                _period = value;
                // Automatically set interest rate based on period
                InterestRate = GetInterestRateForPeriod(value);
            }
        }

        public decimal InterestRate { get; private set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        public DateTime ExpiryDate => PurchaseDate.AddYears(Period);

        public Certificate() { }

        public Certificate(int customerId, decimal price, int period)
        {
            CustomerID = customerId;
            Price = price; // will validate
            Period = period; // will validate and set interest rate
        }

        public static decimal GetInterestRateForPeriod(int period)
        {
            switch (period)
            {
                case 1: return 0.10m;
                case 3: return 0.15m;
                case 5: return 0.20m;
                default:
                    throw new ArgumentException("Invalid certificate period.");
            }
        }

        public decimal CalculateInterestPayout()
        {
            return Price * InterestRate * Period;
        }

        public override string ToString()
        {
            return $"Certificate: {Price:N0} L.E., {Period} Yr @ {InterestRate:P0}";
        }
    }
}
