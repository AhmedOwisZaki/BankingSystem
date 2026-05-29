using System;

namespace BankCore.Models
{
    public class SalaryAccount : Account
    {
        public string EmployerName { get; set; }

        public SalaryAccount(int customerId, decimal initialBalance)
            : base(customerId, initialBalance, AccountType.Salary)
        {
        }
    }
}
