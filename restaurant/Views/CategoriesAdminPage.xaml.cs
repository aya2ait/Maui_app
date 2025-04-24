using restaurant.ViewModels;

namespace restaurant.Views
{
    public partial class CategoriesAdminPage : ContentPage
    {
        private CategoriesAdminViewModel _viewModel;

        public CategoriesAdminPage(CategoriesAdminViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Recharger les catégories quand la page apparaît
            if (_viewModel != null)
            {
                _viewModel.RefreshCommand.Execute(null);
            }
        }
    }
}