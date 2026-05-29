using System;
using System.Collections.Generic;
using BankCore.Models;
using BankCore.Repositories;
using BankCore.Infrastructure;

namespace BankCore.Services
{
    public class AccountService
    {
        private readonly IAccountRepository _accountRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly ICustomerRepository _customerRepo;

        public AccountService()
        {
            _accountRepo = new AccountRepository();
            _transactionRepo = new TransactionRepository();
            _customerRepo = new CustomerRepository();
        }

        public AccountService(IAccountRepository accountRepo, ITransactionRepository transactionRepo, ICustomerRepository customerRepo)
        {
            _accountRepo = accountRepo;
            _transactionRepo = transactionRepo;
            _customerRepo = customerRepo;
        }

        public Account OpenAccount(int customerId, AccountType type, decimal initialBalance)
        {
            var customer = _customerRepo.GetById(customerId);
            if (customer == null)
                throw new KeyNotFoundException("Customer not found.");

            if (initialBalance < 0)
                throw new ArgumentException("Initial balance cannot be negative.");

            // Enforce: only one active Salary account per customer
            if (type == AccountType.Salary)
            {
                var existing = _accountRepo.GetByCustomerId(customerId);
                bool hasActiveSalary = existing.Any(a => a.Type == AccountType.Salary && !a.IsClosed);
                if (hasActiveSalary)
                    throw new InvalidOperationException(
                        "This customer already has an active Salary account. " +
                        "Only one active Salary account is allowed per customer.");
            }

            Account account;
            if (type == AccountType.Saving)
            {
                account = new SavingAccount(customerId, initialBalance);
            }
            else
            {
                account = new SalaryAccount(customerId, initialBalance);
            }

            try
            {
                _accountRepo.Insert(account);
                BankLogger.Instance.LogInfo($"Opened new {type} account for Customer ID {customerId}. Account ID: {account.Id}, Initial Balance: {initialBalance:N2} L.E.", "AccountService");

                if (initialBalance > 0)
                {
                    var transaction = new Transaction(account.Id, TransactionType.Deposit, initialBalance, "Initial Deposit");
                    _transactionRepo.Insert(transaction);
                }

                return account;
            }
            catch (Exception ex)
            {
                BankLogger.Instance.LogError($"Failed to open account for Customer ID {customerId}", ex, "AccountService");
                throw;
            }
        }

        public void Deposit(int accountId, decimal amount, string description)
        {
            var account = _accountRepo.GetById(accountId);
            if (account == null)
                throw new KeyNotFoundException("Account not found.");

            account.Deposit(amount);

            try
            {
                _accountRepo.Update(account);
                var transaction = new Transaction(accountId, TransactionType.Deposit, amount, description);
                _transactionRepo.Insert(transaction);

                BankLogger.Instance.LogInfo($"Deposited {amount:N2} L.E. to Account ID {accountId}. New Balance: {account.Balance:N2} L.E.", "AccountService");
            }
            catch (Exception ex)
            {
                BankLogger.Instance.LogError($"Deposit failed for Account ID {accountId}", ex, "AccountService");
                throw;
            }
        }

        public void Withdraw(int accountId, decimal amount, string description)
        {
            var account = _accountRepo.GetById(accountId);
            if (account == null)
                throw new KeyNotFoundException("Account not found.");

            account.Withdraw(amount);

            try
            {
                _accountRepo.Update(account);
                var transaction = new Transaction(accountId, TransactionType.Withdrawal, amount, description);
                _transactionRepo.Insert(transaction);

                BankLogger.Instance.LogInfo($"Withdrew {amount:N2} L.E. from Account ID {accountId}. New Balance: {account.Balance:N2} L.E.", "AccountService");
            }
            catch (Exception ex)
            {
                BankLogger.Instance.LogError($"Withdrawal failed for Account ID {accountId}", ex, "AccountService");
                throw;
            }
        }

        public void Transfer(int sourceAccountId, int destAccountId, decimal amount, string description)
        {
            if (sourceAccountId == destAccountId)
                throw new ArgumentException("Source and destination accounts must be different.");

            var source = _accountRepo.GetById(sourceAccountId);
            if (source == null)
                throw new KeyNotFoundException("Source account not found.");

            var dest = _accountRepo.GetById(destAccountId);
            if (dest == null)
                throw new KeyNotFoundException("Destination account not found.");

            source.Withdraw(amount);
            dest.Deposit(amount);

            try
            {
                _accountRepo.Update(source);
                _accountRepo.Update(dest);

                var sourceTrans = new Transaction(sourceAccountId, TransactionType.Withdrawal, amount, $"Transfer to Account {destAccountId}: {description}");
                _transactionRepo.Insert(sourceTrans);

                var destTrans = new Transaction(destAccountId, TransactionType.Deposit, amount, $"Transfer from Account {sourceAccountId}: {description}");
                _transactionRepo.Insert(destTrans);

                BankLogger.Instance.LogInfo($"Transferred {amount:N2} L.E. from Account ID {sourceAccountId} to Account ID {destAccountId}.", "AccountService");
            }
            catch (Exception ex)
            {
                BankLogger.Instance.LogError($"Transfer from {sourceAccountId} to {destAccountId} failed", ex, "AccountService");
                throw;
            }
        }

        public void CloseAccount(int accountId)
        {
            var account = _accountRepo.GetById(accountId);
            if (account == null)
                throw new KeyNotFoundException("Account not found.");

            if (account.IsClosed)
                throw new InvalidOperationException("Account is already closed.");

            // Before closing, we can pay out/empty the balance if positive, or require it to be zero
            decimal remainingBalance = account.Balance;
            if (remainingBalance > 0)
            {
                account.Withdraw(remainingBalance);
                _accountRepo.Update(account);

                var transaction = new Transaction(accountId, TransactionType.Withdrawal, remainingBalance, "Payout upon account closure");
                _transactionRepo.Insert(transaction);
            }

            account.IsClosed = true;

            try
            {
                _accountRepo.Update(account);
                BankLogger.Instance.LogInfo($"Closed Account ID {accountId}. Paid out: {remainingBalance:N2} L.E.", "AccountService");
            }
            catch (Exception ex)
            {
                BankLogger.Instance.LogError($"Failed to close Account ID {accountId}", ex, "AccountService");
                throw;
            }
        }

        public List<Account> GetAccountsByCustomerId(int customerId)
        {
            return _accountRepo.GetByCustomerId(customerId);
        }
    }
}
