using System;
using System.Collections.Generic;
using System.Data.SQLite;
using BankCore.Models;
using BankCore.Infrastructure;

namespace BankCore.Repositories
{
    public class ServiceActivityRepository : IServiceActivityRepository
    {
        public ServiceActivity GetById(int id)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM ServiceActivities WHERE ID = @ID";
                cmd.Parameters.AddWithValue("@ID", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapServiceActivity(reader);
                    }
                }
            }
            return null;
        }

        public List<ServiceActivity> GetByCustomerId(int customerId)
        {
            var list = new List<ServiceActivity>();
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM ServiceActivities WHERE CustomerID = @CustomerID ORDER BY Timestamp DESC";
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapServiceActivity(reader));
                    }
                }
            }
            return list;
        }

        public List<ServiceActivity> GetAll()
        {
            var list = new List<ServiceActivity>();
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM ServiceActivities ORDER BY Timestamp DESC";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapServiceActivity(reader));
                    }
                }
            }
            return list;
        }

        public void Insert(ServiceActivity entity)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO ServiceActivities (CustomerID, ServiceType, Description, Timestamp)
                    VALUES (@CustomerID, @ServiceType, @Description, @Timestamp);
                    SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@CustomerID", entity.CustomerID);
                cmd.Parameters.AddWithValue("@ServiceType", entity.ServiceType.ToString());
                cmd.Parameters.AddWithValue("@Description", entity.Description);
                cmd.Parameters.AddWithValue("@Timestamp", entity.Timestamp.ToString("o"));

                entity.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Update(ServiceActivity entity)
        {
            using (var conn = DatabaseManager.Instance.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE ServiceActivities
                    SET CustomerID = @CustomerID, ServiceType = @ServiceType, Description = @Description, Timestamp = @Timestamp
                    WHERE ID = @ID";

                cmd.Parameters.AddWithValue("@ID", entity.Id);
                cmd.Parameters.AddWithValue("@CustomerID", entity.CustomerID);
                cmd.Parameters.AddWithValue("@ServiceType", entity.ServiceType.ToString());
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
                cmd.CommandText = "DELETE FROM ServiceActivities WHERE ID = @ID";
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }
        }

        private ServiceActivity MapServiceActivity(SQLiteDataReader reader)
        {
            return new ServiceActivity
            {
                Id = Convert.ToInt32(reader["ID"]),
                CustomerID = Convert.ToInt32(reader["CustomerID"]),
                ServiceType = (ServiceType)Enum.Parse(typeof(ServiceType), Convert.ToString(reader["ServiceType"])),
                Description = Convert.ToString(reader["Description"]),
                Timestamp = DateTime.Parse(Convert.ToString(reader["Timestamp"]))
            };
        }
    }
}
