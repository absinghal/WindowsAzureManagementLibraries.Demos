using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management.Models;
using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Management.Storage.Models;
using WamlDemos.Commanding;
using System.Threading.Tasks;

namespace WamlDemos.ViewModels
{
    [Export(typeof(IHostedControlViewModel))]
    public class StorageAccountManagementViewModel : BindableBase, IHostedControlViewModel
    {
        private string _connectionString;
        private string _newStorageAccountName;
        private StorageServiceListResponse.StorageService _selectedStorageService;
        private List<StorageServiceListResponse.StorageService> _storageServices;
        private LocationsListResponse.Location _selectedLocation;
        private StorageManagementClient _storageClient;
        private string _nameOkayStatus = "[Enter Name]";
        private bool _isCreateEnabled = false;

        public MainWindowViewModel Host { get; private set; }

        public string Name { get { return "Storage Accounts"; } }

        public void SetHost(MainWindowViewModel host)
        {
            Host = host;
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                _connectionString = value;
                OnPropertyChanged();
            }
        }

        public string IsNameOkay
        {
            get { return _nameOkayStatus; }
            set 
            { 
                _nameOkayStatus = value;
                OnPropertyChanged();
            }
        }

        public bool IsCreateEnabled
        {
            get { return _isCreateEnabled; }
            set
            {
                _isCreateEnabled = value;

                if (value)
                    IsNameOkay = "Name is Okay";
                else
                    IsNameOkay = "Name is Not Okay";

                OnPropertyChanged();
            }
        }

        public ActionCommand CreateStorageAccountCommand
        {
            get { return (Action)CreateStorageAccount; }
        }

        public ActionCommand CheckStorageAccountNameCommand
        {
            get { return (Action)CheckStorageAccountName; }
        }

        public ActionCommand<StorageServiceListResponse.StorageService> DeleteAccountCommand
        {
            get { return (Action<StorageServiceListResponse.StorageService>)DeleteAccount; }
        }

        public string NewStorageAccountName
        {
            get { return _newStorageAccountName; }
            set
            {
                _newStorageAccountName = value;
                CheckStorageAccountName();
                OnPropertyChanged();
            }
        }

        public StorageServiceListResponse.StorageService SelectedStorageService
        {
            get { return _selectedStorageService; }
            set
            {
                _selectedStorageService = value;
                SelectedStorageAccountChanged();
                OnPropertyChanged();
            }
        }

        public List<StorageServiceListResponse.StorageService> StorageServices
        {
            get { return _storageServices; }
            set
            {
                _storageServices = value;
                OnPropertyChanged();
            }
        }

        public LocationsListResponse.Location SelectedLocation
        {
            get { return _selectedLocation; }
            set
            {
                _selectedLocation = value;
                OnPropertyChanged();
            }
        }

        public async void SubscriptionChanged()
        {
            if (_storageClient != null)
                _storageClient.Dispose();

            _storageClient = new StorageManagementClient(
                new CertificateCloudCredentials(Host.SelectedSubscription.SubscriptionId,
                    new X509Certificate2(
                        Convert.FromBase64String(
                            Host.SelectedSubscription.ManagementCertificate))));

            var result = await _storageClient.StorageAccounts.ListAsync();
            StorageServices = result.StorageServices.ToList();

            await GetStorageAccounts();
        }

        public async Task GetStorageAccounts()
        {
            Host.StatusMessage = "Getting Storage Accounts";

            var result = await _storageClient.StorageAccounts.ListAsync();
            StorageServices = result.StorageServices.ToList();

            Host.StatusMessage = "Storage Accounts Loaded";
        }

        private async void DeleteAccount(StorageServiceListResponse.StorageService account)
        {
            Host.StatusMessage = "Deleting storage account " + account.ServiceName;

            await _storageClient.StorageAccounts.DeleteAsync(account.ServiceName);
            await GetStorageAccounts();

            Host.StatusMessage = "Storage account " + account.ServiceName + " deleted";
        }

        private async void CreateStorageAccount()
        {
            Host.StatusMessage = "Creating Storage Account";

            var acct = new StorageAccountCreateParameters
            {
                ServiceName = NewStorageAccountName,
                Location = SelectedLocation.Name
            };

            await _storageClient.StorageAccounts.CreateAsync(acct);

            Host.StatusMessage = "Storage Account Created";

            await GetStorageAccounts();
        }

        private async void CheckStorageAccountName()
        {
            try
            {
                var result = await _storageClient.StorageAccounts.CheckNameAvailabilityAsync(
                    this._newStorageAccountName);
                this.IsCreateEnabled = result.IsAvailable;
            }
            catch
            {
                this.IsCreateEnabled = false;
            }
        }


        private async void SelectedStorageAccountChanged()
        {
            var selected = _selectedStorageService;

            if (selected == null)
            {
                return;
            }

            Host.StatusMessage = "Getting Connection String";

            var keys = await _storageClient.StorageAccounts.GetKeysAsync(selected.ServiceName);

            const string template = "DefaultEndpointsProtocol=http;AccountName={0};AccountKey={1};";

            ConnectionString = string.Format(template, selected.ServiceName, keys.SecondaryKey);

            Host.StatusMessage = "Connection String Received";
        }

    }
}
