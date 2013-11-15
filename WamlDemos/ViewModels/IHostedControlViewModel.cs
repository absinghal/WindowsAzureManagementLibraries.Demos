namespace WamlDemos.ViewModels
{
    public interface IHostedControlViewModel
    {
        void SetHost(MainWindowViewModel host);
        
        void SubscriptionChanged();

        string Name { get; }
    }
}