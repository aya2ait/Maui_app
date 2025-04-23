using System;
using restaurant.Services;

namespace restaurant.ViewModels
{
    public class ShellViewModel : BaseViewModel
    {
        private readonly PanierService _panierService;
        
        private int _panierItemCount;
        public int PanierItemCount
        {
            get => _panierItemCount;
            set => SetProperty(ref _panierItemCount, value);
        }
        
        public ShellViewModel(PanierService panierService)
        {
            _panierService = panierService;
            
            // S'abonner à l'événement de changement du panier
            _panierService.PanierChanged += OnPanierChanged;
            
            // Initialiser le compteur d'articles
            PanierItemCount = _panierService.ItemCount;
        }
        
        private void OnPanierChanged(object sender, EventArgs e)
        {
            PanierItemCount = _panierService.ItemCount;
        }
    }
}