using System;
using System.Collections.Generic;
using BankCore.Models;
using BankCore.Repositories;
using BankCore.Infrastructure;

namespace BankCore.Services
{
    public class CreditCardService
    {
        private readonly ICreditCardRepository _creditCardRepo;
        private readonly IServiceActivityRepository _activityRepo;
        private readonly ICustomerRepository _customerRepo;

        public CreditCardService()
        {
            _creditCardRepo = new CreditCardRepository();
            _activityRepo = new ServiceActivityRepository();
            _customerRepo = new CustomerRepository();
        }

        public CreditCardService(ICreditCardRepository creditCardRepo, IServiceActivityRepository activityRepo, ICustomerRepository customerRepo)
        {
            _creditCardRepo = creditCardRepo;
            _activityRepo = activityRepo;
            _customerRepo = customerRepo;
        }

        public CreditCard IssueCreditCard(int customerId, decimal cashLimit)
        {
            var customer = _customerRepo.GetById(customerId);
            if (customer == null)
                throw new KeyNotFoundException("Customer not found.");

            // Check if customer already has a credit card
            var existing = _creditCardRepo.GetByCustomerId(customerId);
            if (existing != null)
                throw new InvalidOperationException("Customer already has an active credit card. Only one credit card is allowed per customer.");

            // Validate cash limit through CreditCard object instantiation
            var card = new CreditCard(customerId, cashLimit);

            try
            {
                _creditCardRepo.Insert(card);

                string desc = $"Issued Credit Card with limit {cashLimit:N0} L.E.";
                var activity = new ServiceActivity(customerId, ServiceType.CreditCard, desc);
                _activityRepo.Insert(activity);

                BankLogger.Instance.LogInfo($"Successfully issued Credit Card ID {card.Id} for Customer ID {customerId} with limit {cashLimit:N0} L.E.", "CreditCardService");
                return card;
            }
            catch (Exception ex)
            {
                BankLogger.Instance.LogError($"Failed to issue credit card for Customer ID {customerId}", ex, "CreditCardService");
                throw;
            }
        }

        public void UpdateCreditCardLimit(int cardId, decimal newLimit)
        {
            var card = _creditCardRepo.GetById(cardId);
            if (card == null)
                throw new KeyNotFoundException("Credit card not found.");

            decimal oldLimit = card.CashLimit;
            card.CashLimit = newLimit; // validates limit inside domain model

            try
            {
                _creditCardRepo.Update(card);

                string desc = $"Updated Credit Card limit: {oldLimit:N0} -> {newLimit:N0} L.E.";
                var activity = new ServiceActivity(card.CustomerID, ServiceType.CreditCard, desc);
                _activityRepo.Insert(activity);

                BankLogger.Instance.LogInfo($"Successfully updated limit on Card ID {cardId}: {desc}", "CreditCardService");
            }
            catch (Exception ex)
            {
                BankLogger.Instance.LogError($"Failed to update credit card limit for Card ID {cardId}", ex, "CreditCardService");
                throw;
            }
        }

        public void ChargeCreditCard(int cardId, decimal amount, string description)
        {
            var card = _creditCardRepo.GetById(cardId);
            if (card == null)
                throw new KeyNotFoundException("Credit card not found.");

            card.Charge(amount); // validates limit inside domain model

            try
            {
                _creditCardRepo.Update(card);

                string desc = $"Charged Credit Card: {amount:N2} L.E. - {description}";
                var activity = new ServiceActivity(card.CustomerID, ServiceType.CreditCard, desc);
                _activityRepo.Insert(activity);

                BankLogger.Instance.LogInfo($"Charged Credit Card ID {cardId} for {amount:N2} L.E. Debt is now {card.CurrentDebt:N2} L.E.", "CreditCardService");
            }
            catch (Exception ex)
            {
                BankLogger.Instance.LogError($"Failed to charge credit card ID {cardId}", ex, "CreditCardService");
                throw;
            }
        }

        public void RepayCreditCard(int cardId, decimal amount, string description)
        {
            var card = _creditCardRepo.GetById(cardId);
            if (card == null)
                throw new KeyNotFoundException("Credit card not found.");

            card.Repay(amount); // validates inside domain model

            try
            {
                _creditCardRepo.Update(card);

                string desc = $"Repaid Credit Card debt: {amount:N2} L.E. - {description}";
                var activity = new ServiceActivity(card.CustomerID, ServiceType.CreditCard, desc);
                _activityRepo.Insert(activity);

                BankLogger.Instance.LogInfo($"Repaid Credit Card ID {cardId} for {amount:N2} L.E. Remaining Debt is {card.CurrentDebt:N2} L.E.", "CreditCardService");
            }
            catch (Exception ex)
            {
                BankLogger.Instance.LogError($"Failed to repay credit card ID {cardId}", ex, "CreditCardService");
                throw;
            }
        }

        public void DeleteCreditCard(int cardId)
        {
            // Requirement explicitly says: "Customers can delete a certificate but not a credit card."
            throw new InvalidOperationException("Credit cards cannot be deleted from the system once issued.");
        }

        public CreditCard GetCreditCardByCustomerId(int customerId)
        {
            return _creditCardRepo.GetByCustomerId(customerId);
        }
    }
}
