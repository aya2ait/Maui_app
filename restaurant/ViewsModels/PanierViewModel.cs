using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using restaurant.Models;
using restaurant.Services;
using restaurant.Views;

namespace restaurant.ViewModels
{
    public class PanierViewModel : BaseViewModel
    {
        private readonly PanierService _panierService;
        private readonly DatabaseService _databaseService;
        private readonly AuthService _authService;

        
        public ObservableCollection<LigneCommande> LignesCommande { get; } = new ObservableCollection<LigneCommande>();
        
        private decimal _total;
        public decimal Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }
        
        private bool _hasItems;
        public bool HasItems
        {
            get => _hasItems;
            set => SetProperty(ref _hasItems, value);
        }
        
        public ICommand LoadPanierCommand { get; }
        public ICommand AugmenterQuantiteCommand { get; }
        public ICommand DiminuerQuantiteCommand { get; }
        public ICommand SupprimerPlatCommand { get; }
        public ICommand ViderPanierCommand { get; }
        public ICommand PasserCommandeCommand { get; }
        public ICommand ContinuerAchatCommand { get; }
        
        public PanierViewModel(PanierService panierService, DatabaseService databaseService,AuthService authService)
        {
            _panierService = panierService;
            _databaseService = databaseService;
            _authService = authService;

            
            Title = "Mon Panier";
            
            // S'abonner à l'événement de changement du panier
            _panierService.PanierChanged += (sender, args) => LoadPanier();
            
            // Commandes
            LoadPanierCommand = new Command(LoadPanier);
            AugmenterQuantiteCommand = new Command<int>(AugmenterQuantite);
            DiminuerQuantiteCommand = new Command<int>(DiminuerQuantite);
            SupprimerPlatCommand = new Command<int>(SupprimerPlat);
            ViderPanierCommand = new Command(ViderPanier);
            PasserCommandeCommand = new Command(PasserCommande);
            ContinuerAchatCommand = new Command(ContinuerAchat);
        }
        
        private void LoadPanier()
        {
            if (IsBusy)
                return;
                
            IsBusy = true;
            
            try
            {
                LignesCommande.Clear();
                
                foreach (var item in _panierService.Items)
                {
                    LignesCommande.Add(item);
                }
                
                Total = _panierService.Total;
                HasItems = LignesCommande.Count > 0;
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Erreur", $"Impossible de charger le panier: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        private void AugmenterQuantite(int platId)
        {
            var item = _panierService.Items.FirstOrDefault(i => i.PlatID == platId);
            if (item != null)
            {
                _panierService.ModifierQuantite(platId, item.Quantite + 1);
            }
        }
        
        private void DiminuerQuantite(int platId)
        {
            var item = _panierService.Items.FirstOrDefault(i => i.PlatID == platId);
            if (item != null && item.Quantite > 1)
            {
                _panierService.ModifierQuantite(platId, item.Quantite - 1);
            }
        }
        
        private void SupprimerPlat(int platId)
        {
            _panierService.SupprimerPlat(platId);
        }
        
        private void ViderPanier()
        {
            _panierService.ViderPanier();
        }
        
private async void PasserCommande()
{
    if (IsBusy || !HasItems)
        return;
            
    IsBusy = true;
    
    try
    {
        // Vérifier si l'utilisateur est connecté
        if (!_authService.IsAuthenticated)
        {
            await Application.Current.MainPage.DisplayAlert("Erreur", 
                "Vous devez être connecté pour passer une commande.", 
                "OK");
            
            // Rediriger vers la page de connexion
            await Shell.Current.GoToAsync("///login");
            return;
        }
        
        // Créer une commande avec les articles du panier
        var commande = new Commande
        {
            // Utiliser l'ID de l'utilisateur connecté
            UtilisateurID = _authService.CurrentUser.UtilisateurID,
            TableID = null, // commande à emporter
            DateHeure = DateTime.Now,
            Statut = "En attente",
            Total = Total,
            Notes = "Commande via l'application mobile"
        };
        
        // Créer la commande dans la base de données
        int commandeId = await _databaseService.AddCommandeAsync(commande);
        
        // Créer les lignes de commande
        foreach (var item in _panierService.Items)
        {
            var ligneCommande = new LigneCommande
            {
                CommandeID = commandeId,
                PlatID = item.PlatID,
                Quantite = item.Quantite,
                PrixUnitaire = item.PrixUnitaire,
                Notes = item.Notes
            };
            
            // Ajouter la ligne de commande à la base de données
            await _databaseService.AddLigneCommandeAsync(ligneCommande);
        }
        
        // Vider le panier après avoir passé la commande
        _panierService.ViderPanier();
        
        // NOUVEAU : Naviguer vers la page de paiement au lieu d'afficher une alerte
        // Créer un dictionnaire de paramètres de navigation
        var navigationParameters = new Dictionary<string, object>
        {
            { "commandeId", commandeId },
            { "montantTotal", commande.Total }
        };
        
        // Navigation vers la page de paiement avec paramètres
        // Note: Vous devrez enregistrer "paiement" comme route dans votre AppShell.xaml.cs
        await Shell.Current.GoToAsync("paiement", navigationParameters);
    }
    catch (Exception ex)
    {
        await Application.Current.MainPage.DisplayAlert("Erreur", 
            $"Impossible de passer la commande: {ex.Message}", 
            "OK");
    }
    finally
    {
        IsBusy = false;
    }
}        private async void ContinuerAchat()
        {
            // Rediriger l'utilisateur vers la page des catégories
            await Shell.Current.GoToAsync("///menu");
        }
        private void AddToPanier(Plat plat)
        {
            if (plat == null)
                return;
        
            _panierService.AjouterPlat(plat);
    
            // Afficher une notification en bas de l'écran pendant quelques secondes
            Application.Current.MainPage.DisplayAlert("Ajouté au panier", 
                $"{plat.Nom} a été ajouté à votre commande", 
                "OK");
        }
    }
}