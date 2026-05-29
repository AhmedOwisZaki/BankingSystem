using System;
using System.Collections.Generic;
using System.Data.SQLite;
using BankCore.Models;
using BankCore.Infrastructure;

namespace BankCore.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        public Transaction GetById(int id)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Transactions WHERE ID = @ID";
                cmd.Parameters.AddWithValue("@ID", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapTransaction(reader);
                    }
                }
            }
            return null;
        }

        public List<Transaction> GetByAccountId(int accountId)
        {
            var list = new List<Transaction>();
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Transactions WHERE AccountID = @AccountID ORDER BY Timestamp DESC";
                cmd.Parameters.AddWithValue("@AccountID", accountId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapTransaction(reader));
                    }
                }
            }
            return list;
        }

        public List<Transaction> GetByCustomerId(int customerId)
        {
            var list = new List<Transaction>();
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT t.* FROM Transactions t
                    JOIN Accounts a ON t.AccountID = a.ID
                    WHERE a.CustomerID = @CustomerID
                    ORDER BY t.Timestamp DESC";
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapTransaction(reader));
                    }
                }
            }
            return list;
        }

        public List<Transaction> GetAll()
        {
            var list = new List<Transaction>();
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Transactions ORDER BY Timestamp DESC";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapTransaction(reader));
                    }
                }
            }
            return list;
        }

        public void Insert(Transaction entity)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Transactions (AccountID, Type, Amount, Description, Timestamp)
                    VALUES (@AccountID, @Type, @Amount, @Description, @Timestamp);
                    SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@AccountID", entity.AccountID);
                cmd.Parameters.AddWithValue("@Type", entity.Type.ToString());
                cmd.Parameters.AddWithValue("@Amount", (double)entity.Amount);
                cmd.Parameters.AddWithValue("@Description", entity.Description);
                cmd.Parameters.AddWithValue("@Timestamp", entity.Timestamp.ToString("o"));

                entity.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Update(Transaction entity)
        {
            // Transactions are usually append-only, but let's implement Update to conform to basic patterns
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE Transactions
                    SET AccountID = @AccountID, Type = @Type, Amount = @Amount, Description = @Description, Timestamp = @Timestamp
                    WHERE ID = @ID";

                cmd.Parameters.AddWithValue("@ID", entity.Id);
                cmd.Parameters.AddWithValue("@AccountID", entity.AccountID);
                cmd.Parameters.AddWithValue("@Type", entity.Type.ToString());
                cmd.Parameters.AddWithValue("@Amount", (double)entity.Amount);
                cmd.Parameters.AddWithValue("@Description", entity.Description);
                cmd.Parameters.AddWithValue("@Timestamp", entity.Timestamp.ToString("o"));

                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Transactions WHERE ID = @ID";
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }
        }

        private Transaction MapTransaction(SQLiteDataReader reader)
        {
            return new Transaction
            {
                Id = Convert.ToInt32(reader["ID"]),
                AccountID = Convert.ToInt32(reader["AccountID"]),
                Type = (TransactionType)Enum.Parse(typeof(TransactionType), Convert.ToString(reader["Type"])),
                Amount = Convert.ToDecimal(reader["Amount"]),
                Description = Convert.ToString(reader["Description"]),
                Timestamp = DateTime.Parse(Convert.ToString(reader["Timestamp"]))
            };
        }
    }
}
