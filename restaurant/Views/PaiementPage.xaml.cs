using Microsoft.Maui.Controls;
using restaurant.ViewModels;

namespace restaurant.Views
{
    public partial class PaiementPage : ContentPage
    {
        private PaiementViewModel _viewModel;
        
        public PaiementPage(PaiementViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }
        
        // Cette méthode peut être appelée pour initialiser la page avec les informations de commande
        public void InitialiserAvecCommande(int commandeId, decimal montantTotal)
        {
            _viewModel.InitialiserPaiement(commandeId, montantTotal);
        }
    }
}