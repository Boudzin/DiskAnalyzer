using DiskAnalyzer.ViewModels;

namespace DiskAnalyzer
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainViewModel();
            if (BindingContext is MainViewModel vm)
            {
                vm.Page = this;
            }
        }
    }
}
