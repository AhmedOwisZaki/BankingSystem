using System;

namespace BankCore.Models
{
    public class SavingAccount : Account
    {
        public decimal InterestRate { get; set; } = 0.05m; // 5% default annual interest

        public SavingAccount(int customerId, decimal initialBalance)
            : base(customerId, initialBalance, AccountType.Saving)
        {
        }

        public void ApplyInterest()
        {
            if (IsClosed) return;
            decimal interest = Balance * InterestRate;
            Deposit(interest);
        }
    }
}
