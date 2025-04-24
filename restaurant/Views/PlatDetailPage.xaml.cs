using restaurant.ViewModels;

namespace restaurant.Views
{
    public partial class PlatDetailPage : ContentPage
    {
        private readonly PlatDetailViewModel _viewModel;

        public PlatDetailPage(PlatDetailViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }
    }
}