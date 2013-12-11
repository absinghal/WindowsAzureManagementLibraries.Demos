using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management.Sql;
using Microsoft.WindowsAzure.Management.Sql.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WamlDemos.Commanding;

namespace WamlDemos.ViewModels
{
    [Export(typeof(IHostedControlViewModel))]
    public class SqlViewModel : BindableBase, IHostedControlViewModel
    {
        private MainWindowViewModel _host;
        private SqlManagementClient _sqlManagementClient;
        private IList<ServerListResponse.Server> _serverList;
        private ServerListResponse.Server _selectedServer;
        private DatabaseListResponse.Database _selectedDatabase;
        private IList<DatabaseListResponse.Database> _databases;
        private IList<FirewallRuleListResponse.FirewallRule> _firewallRules;
        private FirewallRuleListResponse.FirewallRule _selectedFirewallRule;

        public void SetHost(MainWindowViewModel host)
        {
            _host = host;
        }

        public async void SubscriptionChanged()
        {
            CreateSqlManagementClient();

            await GetSqlServerList();
        }

        public string Name
        {
            get { return "SQL Databases"; }
        }

        public IList<ServerListResponse.Server> ServerList
        {
            get { return _serverList; }
            set
            {
                _serverList = value;
                OnPropertyChanged();
            }
        }

        public ServerListResponse.Server SelectedServer
        {
            get { return _selectedServer; }
            set
            {
                _selectedServer = value;
                OnPropertyChanged();
            }
        }

        public DatabaseListResponse.Database SelectedDatabase
        {
            get { return _selectedDatabase; }
            set
            {
                _selectedDatabase = value;
                OnPropertyChanged();
            }
        }

        public IList<DatabaseListResponse.Database> Databases
        {
            get { return _databases; }
            set
            {
                _databases = value;
                OnPropertyChanged();
            }
        }

        public IList<FirewallRuleListResponse.FirewallRule> FirewallRules
        {
            get { return _firewallRules; }
            set
            {
                _firewallRules = value;
                OnPropertyChanged();
            }
        }

        public FirewallRuleListResponse.FirewallRule SelectedFirewallRule
        {
            get { return _selectedFirewallRule; }
            set 
            { 
                _selectedFirewallRule = value;
                OnPropertyChanged();
            }
        }

        public ActionCommand<ServerListResponse.Server> ViewDatabasesForServerCommand
        {
            get { return (Action<ServerListResponse.Server>)ViewDatabasesForServer; }
        }

        private async void ViewDatabasesForServer(ServerListResponse.Server server)
        {
            SelectedServer = server;
            await GetSqlDatabaseList();
        }

        public ActionCommand<ServerListResponse.Server> ViewFirewallRulesForServerCommand
        {
            get { return (Action<ServerListResponse.Server>)ViewFirewallRulesForServer; }
        }

        private async void ViewFirewallRulesForServer(ServerListResponse.Server server)
        {
            SelectedServer = server;
            await GetSqlFirewallRuleList();
        }

        public ActionCommand<DatabaseListResponse.Database> SelectDatabaseCommand
        {
            get { return (Action<DatabaseListResponse.Database>)SelectDatabase; }
        }

        private void SelectDatabase(DatabaseListResponse.Database database)
        {
            SelectedDatabase = database;

            // TODO: add database-inspection UX and logic
        }

        private void CreateSqlManagementClient()
        {
            if (_sqlManagementClient != null)
                _sqlManagementClient.Dispose();

            _sqlManagementClient = new SqlManagementClient(
                new CertificateCloudCredentials(_host.SelectedSubscription.SubscriptionId,
                    new X509Certificate2(
                        Convert.FromBase64String(
                            _host.SelectedSubscription.ManagementCertificate)))
                );
        }

        public async Task GetSqlServerList()
        {
            _host.StatusMessage = "Retrieving SQL Database Servers";

            var getServerResult = await _sqlManagementClient.Servers.ListAsync();
            ServerList = getServerResult.Servers;

            _host.StatusMessage = "SQL Servers Retrieved";
        }

        private async Task GetSqlDatabaseList()
        {
            _host.StatusMessage = "Getting Databases for Server " + SelectedServer.Name;

            var listDatabaseResult = 
                await _sqlManagementClient.Databases.ListAsync(SelectedServer.Name);
            Databases = listDatabaseResult.Databases;

            _host.StatusMessage = "Databases Retrieved";
        }

        private async Task GetSqlFirewallRuleList()
        {
            _host.StatusMessage = "Getting firewall rules for " + SelectedServer.Name;

            var firewallRuleList = 
                await _sqlManagementClient.FirewallRules.ListAsync(SelectedServer.Name);
            FirewallRules = firewallRuleList.FirewallRules;

            _host.StatusMessage = "Firewall Rules Retrieved";
        }


    }
}
