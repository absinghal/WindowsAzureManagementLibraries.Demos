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

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                _connectionString = value;
                OnPropertyChanged();
            }
        }

        public ActionCommand CreateStorageAccountCommand
        {
            get { return (Action)CreateStorageAccount; }
        }

        public MainWindowViewModel Host { get; private set; }

        public string Name { get { return "Storage Accounts"; } }

        public string NewStorageAccountName
        {
            get { return _newStorageAccountName; }
            set
            {
                _newStorageAccountName = value;
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

        public async void GetStorageAccounts()
        {
            Host.StatusMessage = "Getting Storage Accounts";

            using (var storageClient = new StorageManagementClient(
                new CertificateCloudCredentials(Host.SelectedSubscription.SubscriptionId,
                    new X509Certificate2(
                        Convert.FromBase64String(
                            Host.SelectedSubscription.ManagementCertificate)))))
            {
                var result = await storageClient.StorageAccounts.ListAsync();
                StorageServices = result.StorageServices.ToList();
            }

            Host.StatusMessage = "Storage Accounts Loaded";
        }

        public void SetHost(MainWindowViewModel host)
        {
            Host = host;
        }

        public void SubscriptionChanged()
        {
            GetStorageAccounts();
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

        private void CreateStorageAccount()
        {
            var acct = new StorageAccountCreateParameters
            {
                ServiceName = NewStorageAccountName,
                Location = SelectedLocation.Name
            };
        }

        private async void SelectedStorageAccountChanged()
        {
            var selected = _selectedStorageService;

            if (selected == null)
            {
                return;
            }

            Host.StatusMessage = "Getting Connection String";
                
            using (var storageClient = new StorageManagementClient(
                new CertificateCloudCredentials(Host.SelectedSubscription.SubscriptionId,
                    new X509Certificate2(
                        Convert.FromBase64String(
                            Host.SelectedSubscription.ManagementCertificate)))))
            {
                var keys = await storageClient.StorageAccounts.GetKeysAsync(selected.ServiceName);

                const string template = "DefaultEndpointsProtocol=http;AccountName={0};AccountKey={1};";

                ConnectionString = string.Format(template, selected.ServiceName, keys.SecondaryKey);
            }

            Host.StatusMessage = "Connection String Received";
        }
    }
}
