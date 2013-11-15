using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WamlDemos
{
    public partial class StorageAccountManagement : UserControl
    {
        public StorageAccountManagement()
        {
            InitializeComponent();

            Commands.SubscriptionSelectedEvent += OnSubscriptionSelected;
        }

        PublishSettingsSubscriptionItem _subscription;

        public async Task GetStorageAccounts()
        {
            Commands.SetStatus.Execute("Getting Storage Accounts", this);

            using (var storageClient = new StorageManagementClient(
                new CertificateCloudCredentials(_subscription.SubscriptionId,
                    new X509Certificate2(
                        Convert.FromBase64String(
                            _subscription.ManagementCertificate)))))
            {
                var result = await storageClient.StorageAccounts.ListAsync();
                _storageAccounts.ItemsSource = result.StorageServices;
            }

            Commands.SetStatus.Execute("Storage Accounts Loaded", this);
        }

        private async void _storageAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Commands.SetStatus.Execute("Getting Connection String", this);

            var storageService = _storageAccounts.SelectedItem
                as Microsoft.WindowsAzure.Management.Storage.Models.StorageServiceListResponse.StorageService;

            using (var storageClient = new StorageManagementClient(
                new CertificateCloudCredentials(_subscription.SubscriptionId,
                    new X509Certificate2(
                        Convert.FromBase64String(
                            _subscription.ManagementCertificate)))))
            {
                var selected = _storageAccounts.SelectedItem
                    as Microsoft.WindowsAzure.Management.Storage.Models.StorageServiceListResponse.StorageService;

                var keys = await storageClient.StorageAccounts.GetKeysAsync(selected.ServiceName);

                var template = "DefaultEndpointsProtocol=http;AccountName={0};AccountKey={1};";

                _connectionString.Text = string.Format(template,
                    selected.ServiceName, keys.SecondaryKey);
            }

            Commands.SetStatus.Execute("Connection String Received", this);
        }

        private void _createStorageAccount_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void OnSubscriptionSelected(object sender, PublishSettingsSubscriptionItem item)
        {
            _subscription = item;
            await GetStorageAccounts();
        }

        private void OnNewStorageAccountCreateRequest(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO
        }
    }
}
