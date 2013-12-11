using System.ComponentModel.Composition;
using WamlDemos.ViewModels;
using WamlDemos.Views;

namespace WamlDemos
{
    [Export(typeof(IView<StorageAccountManagementViewModel>))]
    public partial class StorageAccountManagement : IView<StorageAccountManagementViewModel>
    {
        public StorageAccountManagement()
        {
            InitializeComponent();
        }

        public StorageAccountManagementViewModel ViewModel
        {
            get { return DataContext as StorageAccountManagementViewModel; }
            set { DataContext = value; }
        }
    }
}
