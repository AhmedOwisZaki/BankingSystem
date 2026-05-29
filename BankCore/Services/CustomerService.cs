using System;
using System.Collections.Generic;
using BankCore.Models;
using BankCore.Repositories;
using BankCore.Infrastructure;

namespace BankCore.Services
{
    public class CustomerService
    {
        private readonly ICustomerRepository _customerRepo;
        private readonly IAccountRepository _accountRepo;

        public CustomerService()
        {
            _customerRepo = new CustomerRepository();
            _accountRepo = new AccountRepository();
        }

        // For unit testing/dependency injection
        public CustomerService(ICustomerRepository customerRepo, IAccountRepository accountRepo)
        {
            _customerRepo = customerRepo;
            _accountRepo = accountRepo;
        }

        public Customer RegisterCustomer(string name, int age, Gender gender, string address, string nationalId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.");
            if (age < 0 || age > 150)
                throw new ArgumentException("Please enter a valid age.");
            if (string.IsNullOrWhiteSpace(nationalId))
                throw new ArgumentException("National ID cannot be empty.");

            // Check if National ID already exists
            var existing = _customerRepo.GetByNationalID(nationalId);
            if (existing != null)
                throw new InvalidOperationException($"A customer with National ID '{nationalId}' already exists.");

            var customer = new Customer
            {
                Name = name.Trim(),
                Age = age,
                Gender = gender,
                Address = address?.Trim() ?? string.Empty,
                NationalID = nationalId.Trim(),
                CreatedAt = DateTime.Now
            };

            try
            {
                _customerRepo.Insert(customer);
                BankLogger.Instance.LogInfo($"Successfully registered new customer: {customer.Name} (ID: {customer.Id})", "CustomerService");
                return customer;
            }
            catch (Exception ex)
            {
                BankLogger.Instance.LogError($"Failed to register customer: {name}", ex, "CustomerService");
                throw;
            }
        }

        public void UpdateCustomer(int id, string name, int age, Gender gender, string address, string nationalId)
        {
            var customer = _customerRepo.GetById(id);
            if (customer == null)
                throw new KeyNotFoundException("Customer not found.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.");
            if (age < 0 || age > 150)
                throw new ArgumentException("Please enter a valid age.");
            if (string.IsNullOrWhiteSpace(nationalId))
                throw new ArgumentException("National ID cannot be empty.");

            // Check unique constraint if national ID is changing
            if (!customer.NationalID.Equals(nationalId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var existing = _customerRepo.GetByNationalID(nationalId.Trim());
                if (existing != null)
                    throw new InvalidOperationException($"A customer with National ID '{nationalId}' already exists.");
            }

            customer.Name = name.Trim();
            customer.Age = age;
            customer.Gender = gender;
            customer.Address = address?.Trim() ?? string.Empty;
            customer.NationalID = nationalId.Trim();

            try
            {
                _customerRepo.Update(customer);
                BankLogger.Instance.LogInfo($"Successfully updated customer details: {customer.Name} (ID: {customer.Id})", "CustomerService");
            }
            catch (Exception ex)
            {
                BankLogger.Instance.LogError($"Failed to update customer: {id}", ex, "CustomerService");
                throw;
            }
        }

        public Customer GetCustomerById(int id)
        {
            var customer = _customerRepo.GetById(id);
            if (customer != null)
            {
                customer.Accounts = _accountRepo.GetByCustomerId(customer.Id);
            }
            return customer;
        }

        public List<Customer> GetAllCustomers()
        {
            var customers = _customerRepo.GetAll();
            foreach (var customer in customers)
            {
                customer.Accounts = _accountRepo.GetByCustomerId(customer.Id);
            }
            return customers;
        }

        public void DeleteCustomer(int id)
        {
            var customer = _customerRepo.GetById(id);
            if (customer == null)
                throw new KeyNotFoundException("Customer not found.");

            // Check if customer has accounts with balance
            var accounts = _accountRepo.GetByCustomerId(id);
            foreach (var account in accounts)
            {
                if (!account.IsClosed && account.Balance > 0)
                    throw new InvalidOperationException($"Cannot delete customer '{customer.Name}' because they have an active account '{account.Id}' with a positive balance of {account.Balance:N2} L.E.");
            }

            try
            {
                _customerRepo.Delete(id);
                BankLogger.Instance.LogInfo($"Successfully deleted customer: {customer.Name} (ID: {id})", "CustomerService");
            }
            catch (Exception ex)
            {
                BankLogger.Instance.LogError($"Failed to delete customer: {id}", ex, "CustomerService");
                throw;
            }
        }
    }
}
