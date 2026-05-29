namespace BankCore.Models
{
    public class BankStatistics
    {
        public int TotalCustomers { get; set; }
        public decimal TotalAccountAssets { get; set; }
        public decimal TotalCertificateAssets { get; set; }
        public int TotalCertificates { get; set; }
        public int TotalCreditCards { get; set; }

        public decimal TotalAssets => TotalAccountAssets + TotalCertificateAssets;
    }
}
