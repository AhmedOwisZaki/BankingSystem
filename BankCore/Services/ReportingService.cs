using System;
using System.Collections.Generic;
using System.Linq;
using BankCore.Models;
using BankCore.Repositories;

namespace BankCore.Services
{
    public class ReportingService
    {
        private readonly ICustomerRepository _customerRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly ICertificateRepository _certificateRepo;
        private readonly ICreditCardRepository _creditCardRepo;
        private readonly IServiceActivityRepository _activityRepo;

        public ReportingService()
        {
            _customerRepo = new CustomerRepository();
            _accountRepo = new AccountRepository();
            _transactionRepo = new TransactionRepository();
            _certificateRepo = new CertificateRepository();
            _creditCardRepo = new CreditCardRepository();
            _activityRepo = new ServiceActivityRepository();
        }

        public ReportingService(
            ICustomerRepository customerRepo,
            IAccountRepository accountRepo,
            ITransactionRepository transactionRepo,
            ICertificateRepository certificateRepo,
            ICreditCardRepository creditCardRepo,
            IServiceActivityRepository activityRepo)
        {
            _customerRepo = customerRepo;
            _accountRepo = accountRepo;
            _transactionRepo = transactionRepo;
            _certificateRepo = certificateRepo;
            _creditCardRepo = creditCardRepo;
            _activityRepo = activityRepo;
        }

        public BankReport GenerateCustomerReport(int customerId)
        {
            var customer = _customerRepo.GetById(customerId);
            if (customer == null)
                throw new KeyNotFoundException("Customer not found.");

            var accounts = _accountRepo.GetByCustomerId(customerId);
            var transactions = _transactionRepo.GetByCustomerId(customerId);
            var certificates = _certificateRepo.GetByCustomerId(customerId);
            var creditCard = _creditCardRepo.GetByCustomerId(customerId);
            var activities = _activityRepo.GetByCustomerId(customerId);

            return new BankReport
            {
                Customer = customer,
                Accounts = accounts,
                Transactions = transactions,
                Certificates = certificates,
                CreditCard = creditCard,
                ServiceActivities = activities
            };
        }

        public BankStatistics GenerateBankStatistics()
        {
            var customers = _customerRepo.GetAll();
            var accounts = _accountRepo.GetAll();
            var certificates = _certificateRepo.GetAll();
            var cards = _creditCardRepo.GetAll();

            decimal totalAccountAssets = accounts.Where(a => !a.IsClosed).Sum(a => a.Balance);
            decimal totalCertificateAssets = certificates.Sum(c => c.Price);

            return new BankStatistics
            {
                TotalCustomers = customers.Count,
                TotalAccountAssets = totalAccountAssets,
                TotalCertificateAssets = totalCertificateAssets,
                TotalCertificates = certificates.Count,
                TotalCreditCards = cards.Count
            };
        }
    }
}
