namespace WamlDemos.Views
{
    public interface IView<TViewModel>
    {
        TViewModel ViewModel { get; set; }
    }
}
