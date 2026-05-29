using System;
using System.Collections.Generic;
using System.Linq;

namespace BankCore.Models
{
    public class BankReport
    {
        public Customer Customer { get; set; }
        public List<Account> Accounts { get; set; } = new List<Account>();
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
        public List<Certificate> Certificates { get; set; } = new List<Certificate>();
        public CreditCard CreditCard { get; set; }
        public List<ServiceActivity> ServiceActivities { get; set; } = new List<ServiceActivity>();

        public decimal TotalAccountBalance => Accounts.Where(a => !a.IsClosed).Sum(a => a.Balance);
        public decimal TotalCertificateValue => Certificates.Sum(c => c.Price);
        public decimal CreditCardDebt => CreditCard != null ? CreditCard.CurrentDebt : 0m;
        public decimal NetWorth => TotalAccountBalance + TotalCertificateValue - CreditCardDebt;

        public override string ToString()
        {
            return $"Report for {Customer.Name} - Accounts: {Accounts.Count}, Certificates: {Certificates.Count}, Net Worth: {NetWorth:N2} L.E.";
        }
    }
}
