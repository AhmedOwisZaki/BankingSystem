using System;
using System.Collections.Generic;
using System.Data.SQLite;
using BankCore.Models;
using BankCore.Infrastructure;

namespace BankCore.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        public Account GetById(int id)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Accounts WHERE ID = @ID";
                cmd.Parameters.AddWithValue("@ID", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapAccount(reader);
                    }
                }
            }
            return null;
        }

        public List<Account> GetByCustomerId(int customerId)
        {
            var list = new List<Account>();
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Accounts WHERE CustomerID = @CustomerID";
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapAccount(reader));
                    }
                }
            }
            return list;
        }

        public List<Account> GetAll()
        {
            var list = new List<Account>();
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Accounts";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapAccount(reader));
                    }
                }
            }
            return list;
        }

        public void Insert(Account entity)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Accounts (CustomerID, Balance, Type, OpenedAt, IsClosed)
                    VALUES (@CustomerID, @Balance, @Type, @OpenedAt, @IsClosed);
                    SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@CustomerID", entity.CustomerID);
                cmd.Parameters.AddWithValue("@Balance", (double)entity.Balance);
                cmd.Parameters.AddWithValue("@Type", entity.Type.ToString());
                cmd.Parameters.AddWithValue("@OpenedAt", entity.OpenedAt.ToString("o"));
                cmd.Parameters.AddWithValue("@IsClosed", entity.IsClosed ? 1 : 0);

                entity.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Update(Account entity)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE Accounts
                    SET Balance = @Balance, IsClosed = @IsClosed
                    WHERE ID = @ID";

                cmd.Parameters.AddWithValue("@ID", entity.Id);
                cmd.Parameters.AddWithValue("@Balance", (double)entity.Balance);
                cmd.Parameters.AddWithValue("@IsClosed", entity.IsClosed ? 1 : 0);

                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Accounts WHERE ID = @ID";
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }
        }

        private Account MapAccount(SQLiteDataReader reader)
        {
            var typeStr = Convert.ToString(reader["Type"]);
            var type = (AccountType)Enum.Parse(typeof(AccountType), typeStr);
            int customerId = Convert.ToInt32(reader["CustomerID"]);
            decimal balance = Convert.ToDecimal(reader["Balance"]);
            int id = Convert.ToInt32(reader["ID"]);
            DateTime openedAt = DateTime.Parse(Convert.ToString(reader["OpenedAt"]));
            bool isClosed = Convert.ToInt32(reader["IsClosed"]) == 1;

            Account account;
            if (type == AccountType.Saving)
            {
                account = new SavingAccount(customerId, balance) { Id = id, OpenedAt = openedAt, IsClosed = isClosed };
            }
            else
            {
                account = new SalaryAccount(customerId, balance) { Id = id, OpenedAt = openedAt, IsClosed = isClosed };
            }

            return account;
        }
    }
}
