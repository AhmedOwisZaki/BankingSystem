using System;
using System.Collections.Generic;
using BankCore.Models;
using BankCore.Services;
using BankCore.Tests.Framework;
using BankCore.Infrastructure;

namespace BankCore.Tests.Tests
{
    public class CertificateServiceTests
    {
        private int _customerId;

        public void Setup()
        {
            DatabaseManager.Instance.ClearDatabase();
            var customerService = new CustomerService();
            var customer = customerService.RegisterCustomer("Ahmed Ali", 30, Gender.Male, "Cairo", "12345");
            _customerId = customer.Id;
        }

        public void TestBuyCertificate_Success()
        {
            Setup();
            var service = new CertificateService();
            var cert = service.BuyCertificate(_customerId, 5000m, 3); // 3 Years, 15% interest

            SimpleAssert.IsNotNull(cert);
            SimpleAssert.AreEqual(5000m, cert.Price);
            SimpleAssert.AreEqual(3, cert.Period);
            SimpleAssert.AreEqual(0.15m, cert.InterestRate);
            SimpleAssert.AreEqual(_customerId, cert.CustomerID);
            SimpleAssert.IsTrue(cert.Id > 0);
        }

        public void TestBuyCertificate_InvalidPriceLessThan1000_Throws()
        {
            Setup();
            var service = new CertificateService();

            SimpleAssert.Throws<ArgumentException>(() =>
            {
                service.BuyCertificate(_customerId, 500m, 1);
            });
        }

        public void TestBuyCertificate_InvalidPriceNotMultipleOf1000_Throws()
        {
            Setup();
            var service = new CertificateService();

            SimpleAssert.Throws<ArgumentException>(() =>
            {
                service.BuyCertificate(_customerId, 1250m, 1);
            });
        }

        public void TestBuyCertificate_InvalidPeriod_Throws()
        {
            Setup();
            var service = new CertificateService();

            SimpleAssert.Throws<ArgumentException>(() =>
            {
                service.BuyCertificate(_customerId, 2000m, 2); // 2 years is invalid
            });
        }

        public void TestModifyCertificate_Success()
        {
            Setup();
            var service = new CertificateService();
            var cert = service.BuyCertificate(_customerId, 2000m, 1);

            service.ModifyCertificate(cert.Id, 10000m, 5);

            var certs = service.GetCertificatesByCustomerId(_customerId);
            var updated = certs.Find(c => c.Id == cert.Id);

            SimpleAssert.AreEqual(10000m, updated.Price);
            SimpleAssert.AreEqual(5, updated.Period);
            SimpleAssert.AreEqual(0.20m, updated.InterestRate); // 20%
        }

        public void TestDeleteCertificate_Success()
        {
            Setup();
            var service = new CertificateService();
            var cert = service.BuyCertificate(_customerId, 2000m, 1);

            service.DeleteCertificate(cert.Id);

            var certs = service.GetCertificatesByCustomerId(_customerId);
            SimpleAssert.AreEqual(0, certs.Count);
        }
    }
}
