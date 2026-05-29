using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BankApp.Helpers;
using BankCore.Infrastructure;
using BankCore.Models;
using BankCore.Services;

namespace BankApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        // Core services
        private readonly CustomerService _customerService;
        private readonly AccountService _accountService;
        private readonly CertificateService _certificateService;
        private readonly CreditCardService _creditCardService;
        private readonly ReportingService _reportingService;

        // Observable collections
        public ObservableCollection<Customer> Customers { get; } = new ObservableCollection<Customer>();
        public ObservableCollection<Customer> PagedCustomers { get; } = new ObservableCollection<Customer>();
        public ObservableCollection<string> GenderOptions { get; } = new ObservableCollection<string> { "Male", "Female", "Other" };
        public ObservableCollection<int> CertificatePeriods { get; } = new ObservableCollection<int> { 1, 3, 5 };
        public ObservableCollection<string> AccountTypeOptions { get; } = new ObservableCollection<string> { "Saving", "Salary" };

        // Filtering & Pagination
        private string _filterText = string.Empty;
        private int _currentPage = 1;
        private const int PageSize = 8;
        private List<Customer> _filteredCustomers = new List<Customer>();

        // State variables
        private Customer _selectedCustomer;
        private BankReport _selectedCustomerReport;
        private BankStatistics _bankStatistics;
        private string _logContent;

        // Inputs for Adding/Editing Customer
        private string _custName;
        private int _custAge = 25;
        private string _custGender = "Male";
        private string _custAddress;
        private string _custNationalId;
        private bool _isEditingCustomer;

        // Inputs for Accounts
        private string _newAccountType = "Saving";
        private decimal _initialDeposit = 1000m;
        private Account _selectedAccount;
        private decimal _transactionAmount;
        private string _transactionDescription = "Manual Deposit/Withdrawal";

        // Inputs for Certificates
        private decimal _certPrice = 1000m;
        private int _certPeriod = 1;
        private Certificate _selectedCertificate;

        // Inputs for Credit Card
        private decimal _ccLimit = 50000m;
        private decimal _ccChargeAmount;
        private decimal _ccRepayAmount;

        public MainViewModel()
        {
            // Initializing Services
            _customerService = new CustomerService();
            _accountService = new AccountService();
            _certificateService = new CertificateService();
            _creditCardService = new CreditCardService();
            _reportingService = new ReportingService();

            // Commands wiring
            SaveCustomerCommand = new RelayCommand(ExecuteSaveCustomer);
            CancelEditCommand = new RelayCommand(ExecuteCancelEdit);
            EditCustomerCommand = new RelayCommand(ExecuteEditCustomer, () => SelectedCustomer != null);
            DeleteCustomerCommand = new RelayCommand(ExecuteDeleteCustomer, () => SelectedCustomer != null);

            OpenAccountCommand = new RelayCommand(ExecuteOpenAccount, () => SelectedCustomer != null);
            DepositCommand = new RelayCommand(ExecuteDeposit, () => SelectedAccount != null && !SelectedAccount.IsClosed && TransactionAmount > 0);
            WithdrawCommand = new RelayCommand(ExecuteWithdraw, () => SelectedAccount != null && !SelectedAccount.IsClosed && TransactionAmount > 0);
            CloseAccountCommand = new RelayCommand(ExecuteCloseAccount, () => SelectedAccount != null && !SelectedAccount.IsClosed);

            BuyCertificateCommand = new RelayCommand(ExecuteBuyCertificate, () => SelectedCustomer != null && CertPrice >= 1000);
            ModifyCertificateCommand = new RelayCommand(ExecuteModifyCertificate, () => SelectedCertificate != null);
            DeleteCertificateCommand = new RelayCommand(ExecuteDeleteCertificate, () => SelectedCertificate != null);

            IssueCreditCardCommand = new RelayCommand(ExecuteIssueCreditCard, () => SelectedCustomer != null && SelectedCustomerReport?.CreditCard == null);
            UpdateCreditCardLimitCommand = new RelayCommand(ExecuteUpdateCreditCardLimit, () => SelectedCustomerReport?.CreditCard != null);
            ChargeCreditCardCommand = new RelayCommand(ExecuteChargeCreditCard, () => SelectedCustomerReport?.CreditCard != null && CreditCardChargeAmount > 0);
            RepayCreditCardCommand = new RelayCommand(ExecuteRepayCreditCard, () => SelectedCustomerReport?.CreditCard != null && CreditCardRepayAmount > 0);

            RefreshStatsCommand = new RelayCommand(ExecuteRefreshStats);
            LoadLogsCommand = new RelayCommand(ExecuteLoadLogs);
            NextPageCommand = new RelayCommand(ExecuteNextPage, () => _currentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(ExecutePreviousPage, () => _currentPage > 1);
            ClearFilterCommand = new RelayCommand(() => { FilterText = string.Empty; });

            // Load Initial Data
            LoadData();
        }

        #region Properties

        // ── Filter & Pagination ──────────────────────────────────────────────────
        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value))
                {
                    _currentPage = 1;
                    RebuildPagedView();
                }
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            private set
            {
                if (SetProperty(ref _currentPage, value))
                {
                    OnPropertyChanged(nameof(PageInfo));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public int TotalPages => _filteredCustomers.Count == 0 ? 1
            : (int)Math.Ceiling(_filteredCustomers.Count / (double)PageSize);

        public string PageInfo => $"Page {CurrentPage} of {TotalPages}  ({_filteredCustomers.Count} customers)";
        // ────────────────────────────────────────────────────────────────────────

        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    OnCustomerSelected();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public BankReport SelectedCustomerReport
        {
            get => _selectedCustomerReport;
            set => SetProperty(ref _selectedCustomerReport, value);
        }

        public BankStatistics BankStatistics
        {
            get => _bankStatistics;
            set => SetProperty(ref _bankStatistics, value);
        }

        public string LogContent
        {
            get => _logContent;
            set => SetProperty(ref _logContent, value);
        }

        // Customer Registration Bindings
        public string CustName
        {
            get => _custName;
            set => SetProperty(ref _custName, value);
        }

        public int CustAge
        {
            get => _custAge;
            set => SetProperty(ref _custAge, value);
        }

        public string CustGender
        {
            get => _custGender;
            set => SetProperty(ref _custGender, value);
        }

        public string CustAddress
        {
            get => _custAddress;
            set => SetProperty(ref _custAddress, value);
        }

        public string CustNationalId
        {
            get => _custNationalId;
            set => SetProperty(ref _custNationalId, value);
        }

        public bool IsEditingCustomer
        {
            get => _isEditingCustomer;
            set => SetProperty(ref _isEditingCustomer, value);
        }

        // Account Bindings
        public string NewAccountType
        {
            get => _newAccountType;
            set => SetProperty(ref _newAccountType, value);
        }

        public decimal InitialDeposit
        {
            get => _initialDeposit;
            set => SetProperty(ref _initialDeposit, value);
        }

        public Account SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                if (SetProperty(ref _selectedAccount, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public decimal TransactionAmount
        {
            get => _transactionAmount;
            set => SetProperty(ref _transactionAmount, value);
        }

        public string TransactionDescription
        {
            get => _transactionDescription;
            set => SetProperty(ref _transactionDescription, value);
        }

        // Certificate Bindings
        public decimal CertPrice
        {
            get => _certPrice;
            set => SetProperty(ref _certPrice, value);
        }

        public int CertPeriod
        {
            get => _certPeriod;
            set => SetProperty(ref _certPeriod, value);
        }

        public Certificate SelectedCertificate
        {
            get => _selectedCertificate;
            set
            {
                if (SetProperty(ref _selectedCertificate, value))
                {
                    if (value != null)
                    {
                        CertPrice = value.Price;
                        CertPeriod = value.Period;
                    }
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        // Credit Card Bindings
        public decimal CreditCardLimit
        {
            get => _ccLimit;
            set => SetProperty(ref _ccLimit, value);
        }

        public decimal CreditCardChargeAmount
        {
            get => _ccChargeAmount;
            set => SetProperty(ref _ccChargeAmount, value);
        }

        public decimal CreditCardRepayAmount
        {
            get => _ccRepayAmount;
            set => SetProperty(ref _ccRepayAmount, value);
        }

        #endregion

        #region Commands

        public ICommand SaveCustomerCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }

        public ICommand OpenAccountCommand { get; }
        public ICommand DepositCommand { get; }
        public ICommand WithdrawCommand { get; }
        public ICommand CloseAccountCommand { get; }

        public ICommand BuyCertificateCommand { get; }
        public ICommand ModifyCertificateCommand { get; }
        public ICommand DeleteCertificateCommand { get; }

        public ICommand IssueCreditCardCommand { get; }
        public ICommand UpdateCreditCardLimitCommand { get; }
        public ICommand ChargeCreditCardCommand { get; }
        public ICommand RepayCreditCardCommand { get; }

        public ICommand RefreshStatsCommand { get; }
        public ICommand LoadLogsCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand ClearFilterCommand { get; }

        #endregion

        #region Logic Handlers

        private void LoadData()
        {
            try
            {
                Customers.Clear();
                var list = _customerService.GetAllCustomers();
                foreach (var customer in list)
                    Customers.Add(customer);

                RebuildPagedView();
                ExecuteRefreshStats();
                ExecuteLoadLogs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Applies the current FilterText to Customers, then slices the result
        /// for the current page and populates PagedCustomers.
        /// </summary>
        private void RebuildPagedView()
        {
            string search = (_filterText ?? string.Empty).Trim().ToLowerInvariant();

            _filteredCustomers = string.IsNullOrEmpty(search)
                ? Customers.ToList()
                : Customers.Where(c =>
                    c.Name.ToLowerInvariant().Contains(search) ||
                    c.NationalID.ToLowerInvariant().Contains(search) ||
                    c.Address.ToLowerInvariant().Contains(search))
                  .ToList();

            // Clamp page within valid range
            if (_currentPage > TotalPages) _currentPage = TotalPages;
            if (_currentPage < 1) _currentPage = 1;

            var page = _filteredCustomers
                .Skip((_currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            PagedCustomers.Clear();
            foreach (var c in page)
                PagedCustomers.Add(c);

            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(PageInfo));
            CurrentPage = _currentPage; // triggers page-button re-evaluation
        }

        private void ExecuteNextPage()
        {
            _currentPage++;
            RebuildPagedView();
        }

        private void ExecutePreviousPage()
        {
            _currentPage--;
            RebuildPagedView();
        }

        private void OnCustomerSelected()
        {
            if (SelectedCustomer == null)
            {
                SelectedCustomerReport = null;
                SelectedAccount = null;
                SelectedCertificate = null;
                return;
            }

            try
            {
                RefreshSelectedCustomerReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Reporting Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshSelectedCustomerReport()
        {
            if (SelectedCustomer == null) return;
            SelectedCustomerReport = _reportingService.GenerateCustomerReport(SelectedCustomer.Id);
            SelectedAccount = SelectedCustomerReport.Accounts.FirstOrDefault(a => !a.IsClosed) ?? SelectedCustomerReport.Accounts.FirstOrDefault();
            SelectedCertificate = SelectedCustomerReport.Certificates.FirstOrDefault();
            
            // If credit card exists, update CreditCardLimit field with it
            if (SelectedCustomerReport.CreditCard != null)
            {
                CreditCardLimit = SelectedCustomerReport.CreditCard.CashLimit;
            }
            else
            {
                CreditCardLimit = 50000m; // Default
            }
            CommandManager.InvalidateRequerySuggested();
        }

        private void ExecuteSaveCustomer()
        {
            try
            {
                Gender gender = (Gender)Enum.Parse(typeof(Gender), CustGender);

                if (IsEditingCustomer)
                {
                    if (SelectedCustomer == null) return;
                    _customerService.UpdateCustomer(SelectedCustomer.Id, CustName, CustAge, gender, CustAddress, CustNationalId);
                    MessageBox.Show("Customer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var customer = _customerService.RegisterCustomer(CustName, CustAge, gender, CustAddress, CustNationalId);
                    MessageBox.Show($"Customer {customer.Name} registered successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                ClearCustomerForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteEditCustomer()
        {
            if (SelectedCustomer == null) return;

            CustName = SelectedCustomer.Name;
            CustAge = SelectedCustomer.Age;
            CustGender = SelectedCustomer.Gender.ToString();
            CustAddress = SelectedCustomer.Address;
            CustNationalId = SelectedCustomer.NationalID;
            IsEditingCustomer = true;
        }

        private void ExecuteCancelEdit()
        {
            ClearCustomerForm();
        }

        private void ExecuteDeleteCustomer()
        {
            if (SelectedCustomer == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete customer '{SelectedCustomer.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                _customerService.DeleteCustomer(SelectedCustomer.Id);
                SelectedCustomer = null;
                LoadData();
                MessageBox.Show("Customer deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteOpenAccount()
        {
            if (SelectedCustomer == null) return;

            try
            {
                AccountType type = (AccountType)Enum.Parse(typeof(AccountType), NewAccountType);
                _accountService.OpenAccount(SelectedCustomer.Id, type, InitialDeposit);
                RefreshSelectedCustomerReport();
                ExecuteRefreshStats();
                ExecuteLoadLogs();
                MessageBox.Show($"Successfully opened {type} account with {InitialDeposit:N2} L.E. initial deposit.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Opening Account", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteDeposit()
        {
            if (SelectedAccount == null) return;

            try
            {
                _accountService.Deposit(SelectedAccount.Id, TransactionAmount, TransactionDescription);
                RefreshSelectedCustomerReport();
                ExecuteRefreshStats();
                ExecuteLoadLogs();
                TransactionAmount = 0;
                MessageBox.Show("Deposit completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Deposit Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteWithdraw()
        {
            if (SelectedAccount == null) return;

            try
            {
                _accountService.Withdraw(SelectedAccount.Id, TransactionAmount, TransactionDescription);
                RefreshSelectedCustomerReport();
                ExecuteRefreshStats();
                ExecuteLoadLogs();
                TransactionAmount = 0;
                MessageBox.Show("Withdrawal completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Withdrawal Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteCloseAccount()
        {
            if (SelectedAccount == null) return;

            var result = MessageBox.Show($"Are you sure you want to close account #{SelectedAccount.Id}? Any remaining balance of {SelectedAccount.Balance:N2} L.E. will be paid out.", "Confirm Close Account", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                _accountService.CloseAccount(SelectedAccount.Id);
                RefreshSelectedCustomerReport();
                ExecuteRefreshStats();
                ExecuteLoadLogs();
                MessageBox.Show("Account closed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Closure Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteBuyCertificate()
        {
            if (SelectedCustomer == null) return;

            try
            {
                _certificateService.BuyCertificate(SelectedCustomer.Id, CertPrice, CertPeriod);
                RefreshSelectedCustomerReport();
                ExecuteRefreshStats();
                ExecuteLoadLogs();
                MessageBox.Show("Certificate purchased successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Purchase Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteModifyCertificate()
        {
            if (SelectedCertificate == null) return;

            try
            {
                _certificateService.ModifyCertificate(SelectedCertificate.Id, CertPrice, CertPeriod);
                RefreshSelectedCustomerReport();
                ExecuteRefreshStats();
                ExecuteLoadLogs();
                MessageBox.Show("Certificate modified successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Modification Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteDeleteCertificate()
        {
            if (SelectedCertificate == null) return;

            var result = MessageBox.Show($"Are you sure you want to cancel/delete Certificate #{SelectedCertificate.Id}?", "Confirm Cancellation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                _certificateService.DeleteCertificate(SelectedCertificate.Id);
                RefreshSelectedCustomerReport();
                ExecuteRefreshStats();
                ExecuteLoadLogs();
                MessageBox.Show("Certificate deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Deletion Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteIssueCreditCard()
        {
            if (SelectedCustomer == null) return;

            try
            {
                _creditCardService.IssueCreditCard(SelectedCustomer.Id, CreditCardLimit);
                RefreshSelectedCustomerReport();
                ExecuteRefreshStats();
                ExecuteLoadLogs();
                MessageBox.Show("Credit Card issued successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Issuance Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteUpdateCreditCardLimit()
        {
            if (SelectedCustomerReport?.CreditCard == null) return;

            try
            {
                _creditCardService.UpdateCreditCardLimit(SelectedCustomerReport.CreditCard.Id, CreditCardLimit);
                RefreshSelectedCustomerReport();
                ExecuteRefreshStats();
                ExecuteLoadLogs();
                MessageBox.Show("Credit Card limit updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Limit Update Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteChargeCreditCard()
        {
            if (SelectedCustomerReport?.CreditCard == null) return;

            try
            {
                _creditCardService.ChargeCreditCard(SelectedCustomerReport.CreditCard.Id, CreditCardChargeAmount, "Manual Card Charge");
                RefreshSelectedCustomerReport();
                ExecuteRefreshStats();
                ExecuteLoadLogs();
                CreditCardChargeAmount = 0;
                MessageBox.Show("Charge completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Charge Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteRepayCreditCard()
        {
            if (SelectedCustomerReport?.CreditCard == null) return;

            try
            {
                _creditCardService.RepayCreditCard(SelectedCustomerReport.CreditCard.Id, CreditCardRepayAmount, "Manual Repayment");
                RefreshSelectedCustomerReport();
                ExecuteRefreshStats();
                ExecuteLoadLogs();
                CreditCardRepayAmount = 0;
                MessageBox.Show("Repayment completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Repayment Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteRefreshStats()
        {
            try
            {
                BankStatistics = _reportingService.GenerateBankStatistics();
            }
            catch (Exception)
            {
                // fail silently during startup/data loads
            }
        }

        private void ExecuteLoadLogs()
        {
            try
            {
                string logPath = BankLogger.Instance.LogFilePath;
                if (File.Exists(logPath))
                {
                    // Read last 100 lines of logs to keep GUI performing fast and responsive
                    var lines = File.ReadLines(logPath).Reverse().Take(100).Reverse();
                    LogContent = string.Join(Environment.NewLine, lines);
                }
                else
                {
                    LogContent = "Log file is empty or has not been created yet.";
                }
            }
            catch (Exception ex)
            {
                LogContent = $"Failed to read logs: {ex.Message}";
            }
        }

        private void ClearCustomerForm()
        {
            CustName = string.Empty;
            CustAge = 25;
            CustGender = "Male";
            CustAddress = string.Empty;
            CustNationalId = string.Empty;
            IsEditingCustomer = false;
        }

        #endregion
    }
}
