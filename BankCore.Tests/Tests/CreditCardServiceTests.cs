using System;
using System.Collections.Generic;
using BankCore.Models;
using BankCore.Services;
using BankCore.Tests.Framework;
using BankCore.Infrastructure;

namespace BankCore.Tests.Tests
{
    public class CreditCardServiceTests
    {
        private int _customerId;

        public void Setup()
        {
            DatabaseManager.Instance.ClearDatabase();
            var customerService = new CustomerService();
            var customer = customerService.RegisterCustomer("Ahmed Ali", 30, Gender.Male, "Cairo", "12345");
            _customerId = customer.Id;
        }

        public void TestIssueCreditCard_Success()
        {
            Setup();
            var service = new CreditCardService();
            var card = service.IssueCreditCard(_customerId, 100000m); // 100k Limit

            SimpleAssert.IsNotNull(card);
            SimpleAssert.AreEqual(100000m, card.CashLimit);
            SimpleAssert.AreEqual(0m, card.CurrentDebt);
            SimpleAssert.AreEqual(100000m, card.AvailableLimit);
            SimpleAssert.AreEqual(_customerId, card.CustomerID);
            SimpleAssert.IsTrue(card.Id > 0);
        }

        public void TestIssueCreditCard_LimitOutOfBounds_Throws()
        {
            Setup();
            var service = new CreditCardService();

            SimpleAssert.Throws<ArgumentException>(() =>
            {
                service.IssueCreditCard(_customerId, 40000m); // Below 50k
            });

            SimpleAssert.Throws<ArgumentException>(() =>
            {
                service.IssueCreditCard(_customerId, 300000m); // Above 250k
            });
        }

        public void TestIssueCreditCard_DuplicateCard_Throws()
        {
            Setup();
            var service = new CreditCardService();
            service.IssueCreditCard(_customerId, 100000m);

            SimpleAssert.Throws<InvalidOperationException>(() =>
            {
                service.IssueCreditCard(_customerId, 150000m); // Duplicate card
            });
        }

        public void TestChargeAndRepay_Success()
        {
            Setup();
            var service = new CreditCardService();
            var card = service.IssueCreditCard(_customerId, 100000m);

            service.ChargeCreditCard(card.Id, 30000m, "Laptop Purchase");
            
            var updated = service.GetCreditCardByCustomerId(_customerId);
            SimpleAssert.AreEqual(30000m, updated.CurrentDebt);
            SimpleAssert.AreEqual(70000m, updated.AvailableLimit);

            service.RepayCreditCard(card.Id, 10000m, "Partial Repayment");
            updated = service.GetCreditCardByCustomerId(_customerId);
            SimpleAssert.AreEqual(20000m, updated.CurrentDebt);
            SimpleAssert.AreEqual(80000m, updated.AvailableLimit);
        }

        public void TestCharge_ExceedsLimit_Throws()
        {
            Setup();
            var service = new CreditCardService();
            var card = service.IssueCreditCard(_customerId, 50000m);

            SimpleAssert.Throws<InvalidOperationException>(() =>
            {
                service.ChargeCreditCard(card.Id, 60000m, "Buying something expensive");
            });
        }

        public void TestDeleteCreditCard_Throws()
        {
            Setup();
            var service = new CreditCardService();
            var card = service.IssueCreditCard(_customerId, 100000m);

            SimpleAssert.Throws<InvalidOperationException>(() =>
            {
                service.DeleteCreditCard(card.Id);
            });
        }
    }
}
