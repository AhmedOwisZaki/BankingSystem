using System.Collections.Generic;
using BankCore.Models;

namespace BankCore.Repositories
{
    public interface IRepository<T> where T : BankEntity
    {
        T GetById(int id);
        List<T> GetAll();
        void Insert(T entity);
        void Update(T entity);
        void Delete(int id);
    }

    public interface ICustomerRepository : IRepository<Customer>
    {
        Customer GetByNationalID(string nationalId);
    }

    public interface IAccountRepository : IRepository<Account>
    {
        List<Account> GetByCustomerId(int customerId);
    }

    public interface ITransactionRepository : IRepository<Transaction>
    {
        List<Transaction> GetByAccountId(int accountId);
        List<Transaction> GetByCustomerId(int customerId);
    }

    public interface ICertificateRepository : IRepository<Certificate>
    {
        List<Certificate> GetByCustomerId(int customerId);
    }

    public interface ICreditCardRepository : IRepository<CreditCard>
    {
        CreditCard GetByCustomerId(int customerId);
    }

    public interface IServiceActivityRepository : IRepository<ServiceActivity>
    {
        List<ServiceActivity> GetByCustomerId(int customerId);
    }
}
