using System;
using System.Collections.Generic;
using System.Data.SQLite;
using BankCore.Models;
using BankCore.Infrastructure;

namespace BankCore.Repositories
{
    public class CertificateRepository : ICertificateRepository
    {
        public Certificate GetById(int id)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Certificates WHERE ID = @ID";
                cmd.Parameters.AddWithValue("@ID", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapCertificate(reader);
                    }
                }
            }
            return null;
        }

        public List<Certificate> GetByCustomerId(int customerId)
        {
            var list = new List<Certificate>();
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Certificates WHERE CustomerID = @CustomerID ORDER BY PurchaseDate DESC";
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapCertificate(reader));
                    }
                }
            }
            return list;
        }

        public List<Certificate> GetAll()
        {
            var list = new List<Certificate>();
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Certificates ORDER BY PurchaseDate DESC";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapCertificate(reader));
                    }
                }
            }
            return list;
        }

        public void Insert(Certificate entity)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Certificates (CustomerID, Price, Period, InterestRate, PurchaseDate)
                    VALUES (@CustomerID, @Price, @Period, @InterestRate, @PurchaseDate);
                    SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@CustomerID", entity.CustomerID);
                cmd.Parameters.AddWithValue("@Price", (double)entity.Price);
                cmd.Parameters.AddWithValue("@Period", entity.Period);
                cmd.Parameters.AddWithValue("@InterestRate", (double)entity.InterestRate);
                cmd.Parameters.AddWithValue("@PurchaseDate", entity.PurchaseDate.ToString("o"));

                entity.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Update(Certificate entity)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE Certificates
                    SET Price = @Price, Period = @Period, InterestRate = @InterestRate
                    WHERE ID = @ID";

                cmd.Parameters.AddWithValue("@ID", entity.Id);
                cmd.Parameters.AddWithValue("@Price", (double)entity.Price);
                cmd.Parameters.AddWithValue("@Period", entity.Period);
                cmd.Parameters.AddWithValue("@InterestRate", (double)entity.InterestRate);

                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Certificates WHERE ID = @ID";
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }
        }

        private Certificate MapCertificate(SQLiteDataReader reader)
        {
            return new Certificate
            {
                Id = Convert.ToInt32(reader["ID"]),
                CustomerID = Convert.ToInt32(reader["CustomerID"]),
                Price = Convert.ToDecimal(reader["Price"]),
                Period = Convert.ToInt32(reader["Period"]),
                PurchaseDate = DateTime.Parse(Convert.ToString(reader["PurchaseDate"]))
            };
        }
    }
}
