using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using restaurant.Models;
using restaurant.Services;
using restaurant.Views;

namespace restaurant.ViewModels
{
    public class CategoriesViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        
        public ObservableCollection<Categorie> Categories { get; } = new ObservableCollection<Categorie>();
        
        public ICommand LoadCategoriesCommand { get; }
        public ICommand SelectCategoryCommand { get; }
        
        public CategoriesViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Catégories";
            
            LoadCategoriesCommand = new Command(async () => await LoadCategoriesAsync());
            SelectCategoryCommand = new Command<Categorie>(async (categorie) => await OnCategorySelected(categorie));
        }
        
        public async Task LoadCategoriesAsync()
        {
            if (IsBusy)
                return;
                
            IsBusy = true;
            
            try
            {
                Categories.Clear();
                var categories = await _databaseService.GetAllCategoriesAsync();
                
                foreach (var categorie in categories)
                {
                    Categories.Add(categorie);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erreur", $"Impossible de charger les catégories: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        private async Task OnCategorySelected(Categorie categorie)
        {
            if (categorie == null)
                return;
                
            // Navigation vers la page des plats
            var navigationParameter = new Dictionary<string, object>
            {
                { "Categorie", categorie }
            };
            
            await Shell.Current.GoToAsync($"{nameof(PlatsPage)}", navigationParameter);
        }
    }
}