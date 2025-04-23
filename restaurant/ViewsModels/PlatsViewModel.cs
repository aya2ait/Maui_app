using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using restaurant.Models;
using restaurant.Services;

namespace restaurant.ViewModels
{
    [QueryProperty(nameof(Categorie), "Categorie")]
    public class PlatsViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly PanierService _panierService;
        
        private Categorie _categorie;
        public Categorie Categorie
        {
            get => _categorie;
            set
            {
                if (SetProperty(ref _categorie, value))
                {
                    Title = value?.Nom ?? "Plats";
                    LoadPlatsCommand.Execute(null);
                }
            }
        }
        
        public ObservableCollection<Plat> Plats { get; } = new ObservableCollection<Plat>();
        
        public ICommand LoadPlatsCommand { get; }
        public ICommand AddToPanierCommand { get; }
        public ICommand ViewPlatDetailsCommand { get; }
        
        public PlatsViewModel(DatabaseService databaseService, PanierService panierService)
        {
            _databaseService = databaseService;
            _panierService = panierService;
            
            LoadPlatsCommand = new Command(async () => await LoadPlatsAsync());
            AddToPanierCommand = new Command<Plat>(AddToPanier);
            ViewPlatDetailsCommand = new Command<Plat>(ViewPlatDetails);
        }
        
        public async Task LoadPlatsAsync()
        {
            if (IsBusy || Categorie == null)
                return;
                
            IsBusy = true;
            
            try
            {
                Plats.Clear();
                var plats = await _databaseService.GetPlatsByCategorieAsync(Categorie.CategorieID);
                
                foreach (var plat in plats)
                {
                    Plats.Add(plat);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erreur", $"Impossible de charger les plats: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        private void AddToPanier(Plat plat)
        {
            if (plat == null)
                return;
                
            _panierService.AjouterPlat(plat);
            
            Application.Current.MainPage.DisplayAlert("Ajouté au panier", $"{plat.Nom} a été ajouté à votre commande", "OK");
        }
        
        private async void ViewPlatDetails(Plat plat)
        {
            if (plat == null)
                return;
                
            await Application.Current.MainPage.DisplayAlert(plat.Nom, 
                $"{plat.Description}\n\nPrix: {plat.Prix:C}", 
                "Fermer");
        }
    }
}