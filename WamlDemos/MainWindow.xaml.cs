using Microsoft.Win32;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml.Linq;

namespace WamlDemos
{


    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var traceAction = new Action<string>(trace =>
            {
                Dispatcher.Invoke(() => _trace.Text =
                    trace + Environment.NewLine + _trace.Text);
            });

            CloudContext.Configuration.Tracing.AddTracingInterceptor(
                new WpfAppTracingInterceptor(traceAction)
                );
        }

        List<PublishSettingsSubscriptionItem> _subscriptionItems =
            new List<PublishSettingsSubscriptionItem>();

        PublishSettingsSubscriptionItem _selectedSubscription;

        private async System.Threading.Tasks.Task GetRegionList()
        {
            using (var client = new ManagementClient(
                new CertificateCloudCredentials(_selectedSubscription.SubscriptionId,
                    new X509Certificate2(Convert.FromBase64String(
                        _selectedSubscription.ManagementCertificate)))))
            {
                var result = await client.Locations.ListAsync();
                var regions = result.Locations.Select(x => x.Name);
                _regions.ItemsSource = regions;
            }
        }

        private void _selectPubSettings_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.FileOk += dlg_FileOk;
            dlg.Filter = "Publish Settings Files|*.publishsettings";
            dlg.ShowDialog();
        }

        void dlg_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            using (var fs = File.OpenRead(((OpenFileDialog)sender).FileName))
            {
                _subscriptionItems.Clear();

                var document = XDocument.Load(fs);
                var subscriptions = document.Element("PublishData")
                    .Element("PublishProfile").Elements("Subscription");

                subscriptions.ToList().ForEach(subscriptionNode =>
                {
                    _subscriptionItems.Add(new PublishSettingsSubscriptionItem
                    {
                        ManagementCertificate = subscriptionNode.Attribute("ManagementCertificate").Value,
                        SubscriptionName = subscriptionNode.Attribute("Name").Value,
                        SubscriptionId = subscriptionNode.Attribute("Id").Value
                    });
                });
            }

            _subscriptions.ItemsSource = _subscriptionItems.Select(x => x.SubscriptionName);
        }

        private async void _subscriptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedSubscription = _subscriptionItems.First(x =>
                x.SubscriptionName == (string)_subscriptions.SelectedItem);

            await GetRegionList();
        }
    }

    public class PublishSettingsSubscriptionItem
    {
        public string SubscriptionId { get; set; }
        public string ManagementCertificate { get; set; }
        public string SubscriptionName { get; set; }
    }
}
