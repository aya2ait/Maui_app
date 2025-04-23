using Microsoft.Maui.Controls;
using restaurant.Services;
using restaurant.ViewModels;

namespace restaurant.Views
{
    public partial class PanierPage : ContentPage
    {
        private PanierViewModel _viewModel;

        public PanierPage(PanierService panierService, DatabaseService databaseService,AuthService authService)
        {
            InitializeComponent();
            
            _viewModel = new PanierViewModel(panierService, databaseService, authService);
            BindingContext = _viewModel;
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadPanierCommand.Execute(null);
        }
    }
}