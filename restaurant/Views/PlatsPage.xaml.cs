using Microsoft.Maui.Controls;
using restaurant.Models;
using restaurant.ViewModels;
using restaurant.Services;

namespace restaurant.Views
{
    public partial class PlatsPage : ContentPage
    {
        private PlatsViewModel _viewModel;

        public PlatsPage(DatabaseService databaseService, PanierService panierService)
        {
            InitializeComponent();
            
            _viewModel = new PlatsViewModel(databaseService, panierService);
            BindingContext = _viewModel;
        }
    }
}