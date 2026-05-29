using System;
using System.Collections.Generic;
using BankCore.Models;
using BankCore.Services;
using BankCore.Tests.Framework;
using BankCore.Infrastructure;

namespace BankCore.Tests.Tests
{
    public class CustomerServiceTests
    {
        public void Setup()
        {
            DatabaseManager.Instance.ClearDatabase();
        }

        public void TestRegisterCustomer_Success()
        {
            Setup();
            var service = new CustomerService();
            var customer = service.RegisterCustomer("Ahmed Ali", 30, Gender.Male, "Cairo, Egypt", "29501011234567");

            SimpleAssert.IsNotNull(customer);
            SimpleAssert.AreEqual("Ahmed Ali", customer.Name);
            SimpleAssert.AreEqual(30, customer.Age);
            SimpleAssert.AreEqual(Gender.Male, customer.Gender);
            SimpleAssert.AreEqual("Cairo, Egypt", customer.Address);
            SimpleAssert.AreEqual("29501011234567", customer.NationalID);
            SimpleAssert.IsTrue(customer.Id > 0);
        }

        public void TestRegisterCustomer_DuplicateNationalID_Throws()
        {
            Setup();
            var service = new CustomerService();
            service.RegisterCustomer("Ahmed Ali", 30, Gender.Male, "Cairo, Egypt", "29501011234567");

            SimpleAssert.Throws<InvalidOperationException>(() =>
            {
                service.RegisterCustomer("Mona Ali", 25, Gender.Female, "Alex, Egypt", "29501011234567");
            });
        }

        public void TestRegisterCustomer_InvalidAge_Throws()
        {
            Setup();
            var service = new CustomerService();

            SimpleAssert.Throws<ArgumentException>(() =>
            {
                service.RegisterCustomer("Ahmed Ali", -5, Gender.Male, "Cairo", "12345");
            });
        }

        public void TestUpdateCustomer_Success()
        {
            Setup();
            var service = new CustomerService();
            var customer = service.RegisterCustomer("Ahmed Ali", 30, Gender.Male, "Cairo", "12345");

            service.UpdateCustomer(customer.Id, "Ahmed Aly", 31, Gender.Male, "Giza", "12345");

            var updated = service.GetCustomerById(customer.Id);
            SimpleAssert.AreEqual("Ahmed Aly", updated.Name);
            SimpleAssert.AreEqual(31, updated.Age);
            SimpleAssert.AreEqual("Giza", updated.Address);
        }

        public void TestDeleteCustomer_WithActiveAccountBalance_Throws()
        {
            Setup();
            var customerService = new CustomerService();
            var accountService = new AccountService();

            var customer = customerService.RegisterCustomer("Ahmed Ali", 30, Gender.Male, "Cairo", "12345");
            var account = accountService.OpenAccount(customer.Id, AccountType.Saving, 500m);

            SimpleAssert.Throws<InvalidOperationException>(() =>
            {
                customerService.DeleteCustomer(customer.Id);
            });
        }
    }
}
