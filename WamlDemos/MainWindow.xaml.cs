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

            Commands.SubscriptionSelectedEvent += OnSubscriptionSelected;
        }

        List<PublishSettingsSubscriptionItem> _subscriptionItems = new List<PublishSettingsSubscriptionItem>();

        IList<Microsoft.WindowsAzure.Management.Models.LocationsListResponse.Location> _locations;

        public PublishSettingsSubscriptionItem SelectedSubscription { get; set;  }

        private async System.Threading.Tasks.Task GetRegionList()
        {
            Commands.SetStatus.Execute("Getting Regions", this);

            using (var client = new ManagementClient(
                new CertificateCloudCredentials(SelectedSubscription.SubscriptionId,
                    new X509Certificate2(Convert.FromBase64String(
                        SelectedSubscription.ManagementCertificate)))))
            {
                var result = await client.Locations.ListAsync();
                var regions = result.Locations.Select(x => x.Name);
                
                _locations = result.Locations;
            }

            Commands.SetStatus.Execute("Regions Loaded", this);
        }

        private void OnTraceMenuItemClicked(object sender, RoutedEventArgs e)
        {
            var traceWindow = new TraceWindow();
            traceWindow.Show();
        }

        private void OnSelectPublishSettingsfileMenuClicked(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();

            dlg.FileOk += (dialog, a) =>
            {
                using (var fs = File.OpenRead(((OpenFileDialog)dialog).FileName))
                {
                    _subscriptionItems.Clear();

                    var document = XDocument.Load(fs);

                    var subscriptions = document
                            .Element("PublishData")
                                .Element("PublishProfile")
                                    .Elements("Subscription");

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

                _selectSubscriptionMenuItem.ItemsSource = _subscriptionItems;

                _selectSubscriptionMenuItem.IsEnabled = 
                    (_selectSubscriptionMenuItem.Items.Count > 0);
            };

            dlg.Filter = "Publish Settings Files|*.publishsettings";
            dlg.ShowDialog();
        }

        private void OnSubscriptionSelected(object sender, PublishSettingsSubscriptionItem item)
        {
            SelectedSubscription = item;
            _selectedSubscriptionStatus.Text = SelectedSubscription.SubscriptionName;
        }

        private void OnRegionFilterMenuItemClicked(object sender, ExecutedRoutedEventArgs e)
        {
            var prm = e.Parameter;
        }

        private void OnSetStatus(object sender, ExecutedRoutedEventArgs e)
        {
            var status = e.Parameter;
            _status.Text = status as string;
        }
    }

    public class PublishSettingsSubscriptionItem
    {
        public string SubscriptionId { get; set; }
        public string ManagementCertificate { get; set; }
        public string SubscriptionName { get; set; }
    }
}
