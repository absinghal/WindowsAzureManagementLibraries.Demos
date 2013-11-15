using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management.Compute;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace WamlDemos.ViewModels
{
    //[Export(typeof(IHostedControlViewModel))]
    public class VirtualMachinesManagementViewModel : BindableBase, IHostedControlViewModel
    {
        public MainWindowViewModel Host { get; private set; }
        ComputeManagementClient _computeManagementClient;

        public void SetHost(MainWindowViewModel host)
        {
            Host = host;
        }

        public void SubscriptionChanged()
        {
            if (_computeManagementClient != null)
                _computeManagementClient.Dispose();

            _computeManagementClient = new ComputeManagementClient(
                new CertificateCloudCredentials(Host.SelectedSubscription.SubscriptionId,
                    new X509Certificate2(
                        Convert.FromBase64String(
                            Host.SelectedSubscription.ManagementCertificate))));
        }

        public string Name
        {
            get { return "Virtual Machines"; }
        }
    }
}
