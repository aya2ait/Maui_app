using Microsoft.Maui.Controls;
using restaurant.ViewModels;
using restaurant.Services;

namespace restaurant.Views
{
    public partial class CategoriesPage : ContentPage
    {
        private CategoriesViewModel _viewModel;

        public CategoriesPage(DatabaseService databaseService)
        {
            InitializeComponent();
            
            _viewModel = new CategoriesViewModel(databaseService);
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadCategoriesCommand.Execute(null);
        }
    }
}