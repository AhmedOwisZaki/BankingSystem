namespace BankCore.Models
{
    public enum AccountType
    {
        Saving,
        Salary
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public enum CertificatePeriod
    {
        OneYear = 1,
        ThreeYears = 3,
        FiveYears = 5
    }

    public enum CardStatus
    {
        Active,
        Suspended,
        Blocked,
        Expired
    }

    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        Interest,
        ServicePayment
    }

    public enum ServiceType
    {
        Certificate,
        CreditCard
    }
}
