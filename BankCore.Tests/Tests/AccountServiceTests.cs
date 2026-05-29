using System;
using System.Collections.Generic;
using BankCore.Models;
using BankCore.Services;
using BankCore.Tests.Framework;
using BankCore.Infrastructure;

namespace BankCore.Tests.Tests
{
    public class AccountServiceTests
    {
        private int _customerId;

        public void Setup()
        {
            DatabaseManager.Instance.ClearDatabase();
            var customerService = new CustomerService();
            var customer = customerService.RegisterCustomer("Ahmed Ali", 30, Gender.Male, "Cairo", "12345");
            _customerId = customer.Id;
        }

        public void TestOpenAccount_Success()
        {
            Setup();
            var service = new AccountService();
            var account = service.OpenAccount(_customerId, AccountType.Saving, 1000m);

            SimpleAssert.IsNotNull(account);
            SimpleAssert.AreEqual(AccountType.Saving, account.Type);
            SimpleAssert.AreEqual(1000m, account.Balance);
            SimpleAssert.AreEqual(_customerId, account.CustomerID);
            SimpleAssert.IsTrue(account.Id > 0);
        }

        public void TestDeposit_Success()
        {
            Setup();
            var service = new AccountService();
            var account = service.OpenAccount(_customerId, AccountType.Saving, 1000m);

            service.Deposit(account.Id, 500m, "Salary Bonus");

            var updated = service.GetAccountsByCustomerId(_customerId)[0];
            SimpleAssert.AreEqual(1500m, updated.Balance);
        }

        public void TestWithdraw_Success()
        {
            Setup();
            var service = new AccountService();
            var account = service.OpenAccount(_customerId, AccountType.Saving, 1000m);

            service.Withdraw(account.Id, 400m, "Atm Cash");

            var updated = service.GetAccountsByCustomerId(_customerId)[0];
            SimpleAssert.AreEqual(600m, updated.Balance);
        }

        public void TestWithdraw_InsufficientFunds_Throws()
        {
            Setup();
            var service = new AccountService();
            var account = service.OpenAccount(_customerId, AccountType.Saving, 100m);

            SimpleAssert.Throws<InvalidOperationException>(() =>
            {
                service.Withdraw(account.Id, 150m, "Atm Cash Overdraft");
            });
        }

        public void TestTransfer_Success()
        {
            Setup();
            var service = new AccountService();
            var source = service.OpenAccount(_customerId, AccountType.Saving, 1000m);
            var dest   = service.OpenAccount(_customerId, AccountType.Salary, 500m);

            service.Transfer(source.Id, dest.Id, 300m, "Pocket money");

            var accounts    = service.GetAccountsByCustomerId(_customerId);
            var updatedSource = accounts.Find(a => a.Id == source.Id);
            var updatedDest   = accounts.Find(a => a.Id == dest.Id);

            SimpleAssert.AreEqual(700m, updatedSource.Balance);
            SimpleAssert.AreEqual(800m, updatedDest.Balance);
        }

        public void TestCloseAccount_PaysOutRemaining_Success()
        {
            Setup();
            var service = new AccountService();
            var account = service.OpenAccount(_customerId, AccountType.Saving, 1000m);

            service.CloseAccount(account.Id);

            var accounts      = service.GetAccountsByCustomerId(_customerId);
            var closedAccount = accounts.Find(a => a.Id == account.Id);

            SimpleAssert.IsTrue(closedAccount.IsClosed);
            SimpleAssert.AreEqual(0m, closedAccount.Balance);
        }

        public void TestOpenSalaryAccount_DuplicateActive_Throws()
        {
            // A customer must NOT have more than one ACTIVE salary account.
            Setup();
            var service = new AccountService();
            service.OpenAccount(_customerId, AccountType.Salary, 5000m); // first salary account — OK

            SimpleAssert.Throws<InvalidOperationException>(() =>
            {
                service.OpenAccount(_customerId, AccountType.Salary, 3000m); // second — must throw
            });
        }

        public void TestOpenSalaryAccount_AfterClosed_Succeeds()
        {
            // Once the original salary account is CLOSED, the customer may open a new one.
            Setup();
            var service = new AccountService();
            var first = service.OpenAccount(_customerId, AccountType.Salary, 5000m);
            service.CloseAccount(first.Id); // close it

            // Should now succeed — no active salary account remains
            var second = service.OpenAccount(_customerId, AccountType.Salary, 3000m);
            SimpleAssert.IsNotNull(second);
            SimpleAssert.AreEqual(AccountType.Salary, second.Type);
            SimpleAssert.IsFalse(second.IsClosed);
        }
    }
}
