using System;
using System.Collections.Generic;
using System.Data.SQLite;
using BankCore.Models;
using BankCore.Infrastructure;

namespace BankCore.Repositories
{
    public class CreditCardRepository : ICreditCardRepository
    {
        public CreditCard GetById(int id)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM CreditCards WHERE ID = @ID";
                cmd.Parameters.AddWithValue("@ID", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapCreditCard(reader);
                    }
                }
            }
            return null;
        }

        public CreditCard GetByCustomerId(int customerId)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM CreditCards WHERE CustomerID = @CustomerID";
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapCreditCard(reader);
                    }
                }
            }
            return null;
        }

        public List<CreditCard> GetAll()
        {
            var list = new List<CreditCard>();
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM CreditCards";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapCreditCard(reader));
                    }
                }
            }
            return list;
        }

        public void Insert(CreditCard entity)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO CreditCards (CustomerID, CashLimit, CurrentDebt, IssuedDate, Status)
                    VALUES (@CustomerID, @CashLimit, @CurrentDebt, @IssuedDate, @Status);
                    SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@CustomerID", entity.CustomerID);
                cmd.Parameters.AddWithValue("@CashLimit", (double)entity.CashLimit);
                cmd.Parameters.AddWithValue("@CurrentDebt", (double)entity.CurrentDebt);
                cmd.Parameters.AddWithValue("@IssuedDate", entity.IssuedDate.ToString("o"));
                cmd.Parameters.AddWithValue("@Status", entity.Status.ToString());

                entity.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Update(CreditCard entity)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE CreditCards
                    SET CashLimit = @CashLimit, CurrentDebt = @CurrentDebt, Status = @Status
                    WHERE ID = @ID";

                cmd.Parameters.AddWithValue("@ID", entity.Id);
                cmd.Parameters.AddWithValue("@CashLimit", (double)entity.CashLimit);
                cmd.Parameters.AddWithValue("@CurrentDebt", (double)entity.CurrentDebt);
                cmd.Parameters.AddWithValue("@Status", entity.Status.ToString());

                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM CreditCards WHERE ID = @ID";
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }
        }

        private CreditCard MapCreditCard(SQLiteDataReader reader)
        {
            return new CreditCard
            {
                Id = Convert.ToInt32(reader["ID"]),
                CustomerID = Convert.ToInt32(reader["CustomerID"]),
                CashLimit = Convert.ToDecimal(reader["CashLimit"]),
                CurrentDebt = Convert.ToDecimal(reader["CurrentDebt"]),
                IssuedDate = DateTime.Parse(Convert.ToString(reader["IssuedDate"])),
                Status = (CardStatus)Enum.Parse(typeof(CardStatus), Convert.ToString(reader["Status"]))
            };
        }
    }
}
