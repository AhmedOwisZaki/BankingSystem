using System;

namespace BankCore.Models
{
    public class ServiceActivity : BankEntity
    {
        public int CustomerID { get; set; }
        public ServiceType ServiceType { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public ServiceActivity() { }

        public ServiceActivity(int customerId, ServiceType serviceType, string description)
        {
            CustomerID = customerId;
            ServiceType = serviceType;
            Description = description;
        }

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {ServiceType}: {Description}";
        }
    }
}
