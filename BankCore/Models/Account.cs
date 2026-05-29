using System;

namespace BankCore.Models
{
    public abstract class Account : BankEntity
    {
        public int CustomerID { get; set; }
        public decimal Balance { get; protected set; }
        public AccountType Type { get; protected set; }
        public DateTime OpenedAt { get; set; } = DateTime.Now;
        public bool IsClosed { get; set; } = false;

        protected Account(int customerId, decimal initialBalance, AccountType type)
        {
            CustomerID = customerId;
            Balance = initialBalance;
            Type = type;
        }

        public virtual void Deposit(decimal amount)
        {
            if (IsClosed)
                throw new InvalidOperationException("Cannot deposit into a closed account.");
            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be positive.", nameof(amount));

            Balance += amount;
        }

        public virtual void Withdraw(decimal amount)
        {
            if (IsClosed)
                throw new InvalidOperationException("Cannot withdraw from a closed account.");
            if (amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive.", nameof(amount));
            if (amount > Balance)
                throw new InvalidOperationException("Insufficient funds.");

            Balance -= amount;
        }

        public void SetBalanceDirect(decimal balance)
        {
            Balance = balance;
        }

        public override string ToString()
        {
            return $"{Type} Account - Balance: {Balance:N2} L.E.";
        }
    }
}
