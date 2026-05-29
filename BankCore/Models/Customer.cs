using System;
using System.Collections.Generic;

namespace BankCore.Models
{
    public class Customer : BankEntity
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public string Address { get; set; }
        public string NationalID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties for OO design
        public List<Account> Accounts { get; set; } = new List<Account>();
        public List<Certificate> Certificates { get; set; } = new List<Certificate>();
        public CreditCard CreditCard { get; set; }

        public override string ToString()
        {
            return $"{Name} (ID: {NationalID})";
        }
    }
}
