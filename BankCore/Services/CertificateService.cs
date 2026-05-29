using System;
using System.Collections.Generic;
using BankCore.Models;
using BankCore.Repositories;
using BankCore.Infrastructure;

namespace BankCore.Services
{
    public class CertificateService
    {
        private readonly ICertificateRepository _certificateRepo;
        private readonly IServiceActivityRepository _activityRepo;
        private readonly ICustomerRepository _customerRepo;

        public CertificateService()
        {
            _certificateRepo = new CertificateRepository();
            _activityRepo = new ServiceActivityRepository();
            _customerRepo = new CustomerRepository();
        }

        public CertificateService(ICertificateRepository certificateRepo, IServiceActivityRepository activityRepo, ICustomerRepository customerRepo)
        {
            _certificateRepo = certificateRepo;
            _activityRepo = activityRepo;
            _customerRepo = customerRepo;
        }

        public Certificate BuyCertificate(int customerId, decimal price, int period)
        {
            var customer = _customerRepo.GetById(customerId);
            if (customer == null)
                throw new KeyNotFoundException("Customer not found.");

            // Create certificate - this will trigger domain model validation for price and period
            var certificate = new Certificate(customerId, price, period);

            try
            {
                _certificateRepo.Insert(certificate);

                // Log as service activity
                string desc = $"Purchased Certificate: {price:N0} L.E., Period: {period} Yr(s), Interest: {certificate.InterestRate:P0}";
                var activity = new ServiceActivity(customerId, ServiceType.Certificate, desc);
                _activityRepo.Insert(activity);

                BankLogger.Instance.LogInfo($"Customer ID {customerId} purchased Certificate ID {certificate.Id}: {desc}", "CertificateService");
                return certificate;
            }
            catch (Exception ex)
            {
                BankLogger.Instance.LogError($"Failed to buy certificate for Customer ID {customerId}", ex, "CertificateService");
                throw;
            }
        }

        public void ModifyCertificate(int certId, decimal newPrice, int newPeriod)
        {
            var cert = _certificateRepo.GetById(certId);
            if (cert == null)
                throw new KeyNotFoundException("Certificate not found.");

            decimal oldPrice = cert.Price;
            int oldPeriod = cert.Period;

            // This will trigger validations on properties
            cert.Price = newPrice;
            cert.Period = newPeriod;

            try
            {
                _certificateRepo.Update(cert);

                string desc = $"Modified Certificate ID {certId}: Price {oldPrice:N0} -> {newPrice:N0} L.E., Period {oldPeriod} -> {newPeriod} Yr(s)";
                var activity = new ServiceActivity(cert.CustomerID, ServiceType.Certificate, desc);
                _activityRepo.Insert(activity);

                BankLogger.Instance.LogInfo($"Successfully modified Certificate ID {certId}: {desc}", "CertificateService");
            }
            catch (Exception ex)
            {
                BankLogger.Instance.LogError($"Failed to modify Certificate ID {certId}", ex, "CertificateService");
                throw;
            }
        }

        public void DeleteCertificate(int certId)
        {
            var cert = _certificateRepo.GetById(certId);
            if (cert == null)
                throw new KeyNotFoundException("Certificate not found.");

            try
            {
                _certificateRepo.Delete(certId);

                string desc = $"Deleted Certificate ID {certId} (Value: {cert.Price:N0} L.E.)";
                var activity = new ServiceActivity(cert.CustomerID, ServiceType.Certificate, desc);
                _activityRepo.Insert(activity);

                BankLogger.Instance.LogInfo($"Successfully deleted Certificate ID {certId} for Customer ID {cert.CustomerID}", "CertificateService");
            }
            catch (Exception ex)
            {
                BankLogger.Instance.LogError($"Failed to delete Certificate ID {certId}", ex, "CertificateService");
                throw;
            }
        }

        public List<Certificate> GetCertificatesByCustomerId(int customerId)
        {
            return _certificateRepo.GetByCustomerId(customerId);
        }
    }
}
