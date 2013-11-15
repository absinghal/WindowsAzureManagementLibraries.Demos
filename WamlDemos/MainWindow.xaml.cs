using System.ComponentModel;
using WamlDemos.ViewModels;
using WamlDemos.Views;

namespace WamlDemos
{
    public partial class MainWindow : IView<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainWindowViewModel();
            Closing += OnClosing;
        }

        public MainWindowViewModel ViewModel
        {
            get { return DataContext as MainWindowViewModel; }
            set { DataContext = value; }
        }

        private static void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            TraceWindow.Instance.Close();
        }
    }
}
