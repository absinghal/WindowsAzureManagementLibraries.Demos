using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Win32;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management;
using Microsoft.WindowsAzure.Management.Models;
using WamlDemos.Commanding;
using WamlDemos.Models;
using WamlDemos.Navigation;

namespace WamlDemos.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly ObservableCollection<PublishSettingsSubscriptionItem> _subscriptionItems = new ObservableCollection<PublishSettingsSubscriptionItem>();
        private IList<LocationsListResponse.Location> _locations;
        private PublishSettingsSubscriptionItem _selectedSubscription;
        private string _statusMessage;

        public MainWindowViewModel()
        {
            foreach (var vm in ModelBasedNavigationHelper.ViewModels)
            {
                vm.SetHost(this);
            }
        }

        public IList<LocationsListResponse.Location> Locations
        {
            get { return _locations; }
            private set
            {
                if (value != null)
                {
                    _locations = value.OrderBy(x => x.DisplayName).ToList();
                }
                else
                {
                    _locations = null;
                }

                OnPropertyChanged();
            }
        }

        public PublishSettingsSubscriptionItem SelectedSubscription
        {
            get { return _selectedSubscription; }
            set
            {
                _selectedSubscription = value;
                GetRegionList();
                NotifyAllHostedControlsOfSubscriptionChange();
                OnPropertyChanged();
            }
        }

        public ActionCommand SelectPublishSettingsFileCommand
        {
            get { return (Action)SelectPublishSettingsFile; }
        }

        public ActionCommand<PublishSettingsSubscriptionItem> SelectSubscriptionCommand
        {
            get { return (Action<PublishSettingsSubscriptionItem>)(o => SelectedSubscription = o); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PublishSettingsSubscriptionItem> SubscriptionItems
        {
            get { return _subscriptionItems; }
        }

        public ActionCommand TraceCommand
        {
            get { return (Action)Trace; }
        }

        public IReadOnlyList<IHostedControlViewModel> WAMLViewModels
        {
            get { return ModelBasedNavigationHelper.ViewModels; }
        }

        private static void NotifyAllHostedControlsOfSubscriptionChange()
        {
            foreach (var vm in ModelBasedNavigationHelper.ViewModels)
            {
                vm.SubscriptionChanged();
            }
        }
        private static void Trace()
        {
            var traceWindow = TraceWindow.Instance;
            traceWindow.Show();
        }

        private async void GetRegionList()
        {
            StatusMessage = "Getting Regions";

            using (var client = new ManagementClient(
                new CertificateCloudCredentials(SelectedSubscription.SubscriptionId,
                    new X509Certificate2(Convert.FromBase64String(
                        SelectedSubscription.ManagementCertificate)))))
            {
                var result = await client.Locations.ListAsync();
                Locations = result.Locations;
            }

            StatusMessage = "Regions Loaded";
        }

        private void SelectPublishSettingsFile()
        {
            var dlg = new OpenFileDialog();

            dlg.FileOk += (dialog, a) =>
            {
                using (var fs = File.OpenRead(((OpenFileDialog)dialog).FileName))
                {
                    _subscriptionItems.Clear();

                    var document = XDocument.Load(fs);

                    var subscriptions = document.XPathSelectElements("/PublishData/PublishProfile/Subscription");

                    foreach (var subscriptionNode in subscriptions)
                    {
                        _subscriptionItems.Add(new PublishSettingsSubscriptionItem
                        {
                            ManagementCertificate = subscriptionNode.Attribute("ManagementCertificate").Value,
                            SubscriptionName = subscriptionNode.Attribute("Name").Value,
                            SubscriptionId = subscriptionNode.Attribute("Id").Value
                        });
                    }
                }
            };

            dlg.Filter = "Publish Settings Files|*.publishsettings";
            dlg.ShowDialog();
        }
    }
}
