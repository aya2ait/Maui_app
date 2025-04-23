using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using restaurant.Models;
using restaurant.Services;

namespace restaurant.ViewModels
{
    public class PaiementViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly AuthService _authService;
        
        private int _commandeId;
        public int CommandeId
        {
            get => _commandeId;
            set => SetProperty(ref _commandeId, value);
        }
        
        private string _commandeNumero;
        public string CommandeNumero
        {
            get => _commandeNumero;
            set => SetProperty(ref _commandeNumero, value);
        }
        
        private decimal _montantTotal;
        public decimal MontantTotal
        {
            get => _montantTotal;
            set => SetProperty(ref _montantTotal, value);
        }
        
        private bool _isCarteBancaireSelected;
        public bool IsCarteBancaireSelected
        {
            get => _isCarteBancaireSelected;
            set => SetProperty(ref _isCarteBancaireSelected, value);
        }
        
        private bool _isEspecesSelected;
        public bool IsEspecesSelected
        {
            get => _isEspecesSelected;
            set => SetProperty(ref _isEspecesSelected, value);
        }
        
        private bool _isPaiementLivraisonSelected;
        public bool IsPaiementLivraisonSelected
        {
            get => _isPaiementLivraisonSelected;
            set => SetProperty(ref _isPaiementLivraisonSelected, value);
        }
        
        private string _numeroCarte;
        public string NumeroCarte
        {
            get => _numeroCarte;
            set => SetProperty(ref _numeroCarte, value);
        }
        
        private string _dateExpiration;
        public string DateExpiration
        {
            get => _dateExpiration;
            set => SetProperty(ref _dateExpiration, value);
        }
        
        private string _codeCVC;
        public string CodeCVC
        {
            get => _codeCVC;
            set => SetProperty(ref _codeCVC, value);
        }
        
        private string _nomCarte;
        public string NomCarte
        {
            get => _nomCarte;
            set => SetProperty(ref _nomCarte, value);
        }
        
        private string _notes;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }
        
        public ICommand ConfirmerPaiementCommand { get; }
        
        public PaiementViewModel(DatabaseService databaseService, AuthService authService)
        {
            _databaseService = databaseService;
            _authService = authService;
            
            Title = "Paiement";
            IsCarteBancaireSelected = true;
            
            // Initialiser les commandes
            ConfirmerPaiementCommand = new Command(ConfirmerPaiement);
        }
        
        public void InitialiserPaiement(int commandeId, decimal montantTotal)
        {
            CommandeId = commandeId;
            CommandeNumero = commandeId.ToString();
            MontantTotal = montantTotal;
        }
        
        private async void ConfirmerPaiement()
        {
            if (IsBusy)
                return;
                
            IsBusy = true;
            
            try
            {
                // Valider les entrées selon la méthode de paiement
                if (IsCarteBancaireSelected)
                {
                    if (string.IsNullOrWhiteSpace(NumeroCarte) || 
                        string.IsNullOrWhiteSpace(DateExpiration) || 
                        string.IsNullOrWhiteSpace(CodeCVC) || 
                        string.IsNullOrWhiteSpace(NomCarte))
                    {
                        await Application.Current.MainPage.DisplayAlert("Erreur", 
                            "Veuillez remplir tous les champs de carte bancaire.", 
                            "OK");
                        return;
                    }
                }
                
                // Déterminer la méthode de paiement
                string methodePaiement = "Carte bancaire";
                if (IsEspecesSelected)
                    methodePaiement = "Espèces";
                else if (IsPaiementLivraisonSelected)
                    methodePaiement = "Paiement à la livraison";
                    
                // Créer l'objet paiement
                var paiement = new Paiement
                {
                    CommandeID = CommandeId,
                    Montant = MontantTotal,
                    MethodePaiement = methodePaiement,
                    DateHeure = DateTime.Now,
                    Reference = Guid.NewGuid().ToString().Substring(0, 8).ToUpper() // Génère une référence unique
                };
                
                // Ajouter le paiement dans la base de données
                int paiementId = await _databaseService.AddPaiementAsync(paiement);
                
                // Mettre à jour le statut de la commande
                string nouveauStatut = "Payée";
                if (methodePaiement == "Paiement à la livraison")
                    nouveauStatut = "En attente de paiement";
                    
                await _databaseService.UpdateCommandeStatutAsync(CommandeId, nouveauStatut);
                
                // Afficher une confirmation
                await Application.Current.MainPage.DisplayAlert("Paiement effectué", 
                    $"Votre paiement a été traité avec succès. Référence: {paiement.Reference}", 
                    "OK");
                
                // Rediriger vers la page d'accueil
                await Shell.Current.GoToAsync("///menu");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erreur", 
                    $"Impossible de traiter le paiement: {ex.Message}", 
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}