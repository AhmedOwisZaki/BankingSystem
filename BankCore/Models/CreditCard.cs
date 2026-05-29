using System;

namespace BankCore.Models
{
    public class CreditCard : BankEntity
    {
        public int CustomerID { get; set; }
        private decimal _cashLimit;

        public decimal CashLimit
        {
            get => _cashLimit;
            set
            {
                if (value < 50000 || value > 250000)
                    throw new ArgumentException("Credit card limit must be between 50,000 L.E and 250,000 L.E.");
                _cashLimit = value;
            }
        }

        public decimal CurrentDebt { get; set; } = 0m;
        public DateTime IssuedDate { get; set; } = DateTime.Now;
        public DateTime ExpiryDate => IssuedDate.AddYears(10);
        public CardStatus Status { get; set; } = CardStatus.Active;

        public CreditCard() { }

        public CreditCard(int customerId, decimal cashLimit)
        {
            CustomerID = customerId;
            CashLimit = cashLimit; // validates
        }

        public decimal AvailableLimit => CashLimit - CurrentDebt;

        public void Charge(decimal amount)
        {
            if (Status != CardStatus.Active)
                throw new InvalidOperationException("Card is not active.");
            if (amount <= 0)
                throw new ArgumentException("Charge amount must be positive.");
            if (amount > AvailableLimit)
                throw new InvalidOperationException("Transaction exceeds available cash limit.");

            CurrentDebt += amount;
        }

        public void Repay(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Repayment amount must be positive.");
            if (amount > CurrentDebt)
                throw new ArgumentException("Repayment amount exceeds current debt.");

            CurrentDebt -= amount;
        }

        public override string ToString()
        {
            return $"Credit Card: Limit {CashLimit:N0} L.E. (Available: {AvailableLimit:N2} L.E.)";
        }
    }
}
