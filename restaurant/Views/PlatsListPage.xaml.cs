using restaurant.ViewModels;

namespace restaurant.Views
{
    public partial class PlatsListPage : ContentPage
    {
        private readonly PlatsListViewModel _viewModel;

        public PlatsListPage(PlatsListViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadPlatsCommand.Execute(null);
        }
    }
}