using System.ComponentModel.Composition;
using WamlDemos.ViewModels;
using WamlDemos.Views;

namespace WamlDemos
{
    [Export(typeof(IView<SqlViewModel>))]
    public partial class SqlDatabaseManagement : IView<SqlViewModel>
    {
        public SqlDatabaseManagement()
        {
            InitializeComponent();
        }

        public SqlViewModel ViewModel
        {
            get { return DataContext as SqlViewModel; }
            set { DataContext = value; }
        }
    }
}
