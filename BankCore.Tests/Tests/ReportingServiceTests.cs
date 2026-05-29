using System;
using System.Collections.Generic;
using BankCore.Models;
using BankCore.Services;
using BankCore.Tests.Framework;
using BankCore.Infrastructure;

namespace BankCore.Tests.Tests
{
    public class ReportingServiceTests
    {
        private int _customer1Id;
        private int _customer2Id;

        public void Setup()
        {
            DatabaseManager.Instance.ClearDatabase();
            
            var customerService = new CustomerService();
            var accountService = new AccountService();
            var certService = new CertificateService();
            var cardService = new CreditCardService();

            // Customer 1 setup
            var customer1 = customerService.RegisterCustomer("Ahmed Ali", 30, Gender.Male, "Cairo", "111");
            _customer1Id = customer1.Id;
            var acc1 = accountService.OpenAccount(_customer1Id, AccountType.Saving, 5000m);
            accountService.Deposit(acc1.Id, 2000m, "Deposit bonus");
            certService.BuyCertificate(_customer1Id, 10000m, 1);
            var card1 = cardService.IssueCreditCard(_customer1Id, 100000m);
            cardService.ChargeCreditCard(card1.Id, 5000m, "Purchase");

            // Customer 2 setup
            var customer2 = customerService.RegisterCustomer("Sara Aly", 25, Gender.Female, "Giza", "222");
            _customer2Id = customer2.Id;
            accountService.OpenAccount(_customer2Id, AccountType.Salary, 8000m);
        }

        public void TestGenerateCustomerReport_Success()
        {
            Setup();
            var service = new ReportingService();
            var report = service.GenerateCustomerReport(_customer1Id);

            SimpleAssert.IsNotNull(report);
            SimpleAssert.AreEqual("Ahmed Ali", report.Customer.Name);
            SimpleAssert.AreEqual(1, report.Accounts.Count);
            SimpleAssert.AreEqual(7000m, report.TotalAccountBalance); // 5000 + 2000
            SimpleAssert.AreEqual(1, report.Certificates.Count);
            SimpleAssert.AreEqual(10000m, report.TotalCertificateValue);
            SimpleAssert.IsNotNull(report.CreditCard);
            SimpleAssert.AreEqual(5000m, report.CreditCardDebt);
            SimpleAssert.AreEqual(12000m, report.NetWorth); // 7000 + 10000 - 5000
        }

        public void TestGenerateBankStatistics_Success()
        {
            Setup();
            var service = new ReportingService();
            var stats = service.GenerateBankStatistics();

            SimpleAssert.IsNotNull(stats);
            SimpleAssert.AreEqual(2, stats.TotalCustomers);
            SimpleAssert.AreEqual(15000m, stats.TotalAccountAssets); // 7000 + 8000
            SimpleAssert.AreEqual(10000m, stats.TotalCertificateAssets); // 10000
            SimpleAssert.AreEqual(25000m, stats.TotalAssets); // 15000 + 10000
            SimpleAssert.AreEqual(1, stats.TotalCertificates);
            SimpleAssert.AreEqual(1, stats.TotalCreditCards);
        }
    }
}
