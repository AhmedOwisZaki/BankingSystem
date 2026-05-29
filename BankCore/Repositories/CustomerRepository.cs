using System;
using System.Collections.Generic;
using System.Data.SQLite;
using BankCore.Models;
using BankCore.Infrastructure;

namespace BankCore.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        public Customer GetById(int id)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Customers WHERE ID = @ID";
                cmd.Parameters.AddWithValue("@ID", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapCustomer(reader);
                    }
                }
            }
            return null;
        }

        public Customer GetByNationalID(string nationalId)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Customers WHERE NationalID = @NationalID";
                cmd.Parameters.AddWithValue("@NationalID", nationalId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapCustomer(reader);
                    }
                }
            }
            return null;
        }

        public List<Customer> GetAll()
        {
            var list = new List<Customer>();
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Customers ORDER BY Name ASC";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapCustomer(reader));
                    }
                }
            }
            return list;
        }

        public void Insert(Customer entity)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Customers (Name, Age, Gender, Address, NationalID, CreatedAt)
                    VALUES (@Name, @Age, @Gender, @Address, @NationalID, @CreatedAt);
                    SELECT last_insert_rowid();";
                
                cmd.Parameters.AddWithValue("@Name", entity.Name);
                cmd.Parameters.AddWithValue("@Age", entity.Age);
                cmd.Parameters.AddWithValue("@Gender", entity.Gender.ToString());
                cmd.Parameters.AddWithValue("@Address", entity.Address);
                cmd.Parameters.AddWithValue("@NationalID", entity.NationalID);
                cmd.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt.ToString("o"));

                entity.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Update(Customer entity)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE Customers
                    SET Name = @Name, Age = @Age, Gender = @Gender, Address = @Address, NationalID = @NationalID
                    WHERE ID = @ID";

                cmd.Parameters.AddWithValue("@ID", entity.Id);
                cmd.Parameters.AddWithValue("@Name", entity.Name);
                cmd.Parameters.AddWithValue("@Age", entity.Age);
                cmd.Parameters.AddWithValue("@Gender", entity.Gender.ToString());
                cmd.Parameters.AddWithValue("@Address", entity.Address);
                cmd.Parameters.AddWithValue("@NationalID", entity.NationalID);

                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Customers WHERE ID = @ID";
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }
        }

        private Customer MapCustomer(SQLiteDataReader reader)
        {
            return new Customer
            {
                Id = Convert.ToInt32(reader["ID"]),
                Name = Convert.ToString(reader["Name"]),
                Age = Convert.ToInt32(reader["Age"]),
                Gender = (Gender)Enum.Parse(typeof(Gender), Convert.ToString(reader["Gender"])),
                Address = Convert.ToString(reader["Address"]),
                NationalID = Convert.ToString(reader["NationalID"]),
                CreatedAt = DateTime.Parse(Convert.ToString(reader["CreatedAt"]))
            };
        }
    }
}
