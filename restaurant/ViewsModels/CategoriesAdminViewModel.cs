using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using restaurant.Models;
using restaurant.Services;

namespace restaurant.ViewModels
{
    public class CategoriesAdminViewModel : INotifyPropertyChanged
    {
        private readonly MenuService _menuService;
        private bool _isLoading;
        private Categorie _selectedCategorie;

        public ObservableCollection<Categorie> Categories { get; private set; }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public Categorie SelectedCategorie
        {
            get => _selectedCategorie;
            set
            {
                _selectedCategorie = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddCategorieCommand { get; }
        public ICommand EditCategorieCommand { get; }
        public ICommand DeleteCategorieCommand { get; }
        public ICommand RefreshCommand { get; }

        public CategoriesAdminViewModel(MenuService menuService)
        {
            _menuService = menuService;
            Categories = new ObservableCollection<Categorie>();

            AddCategorieCommand = new Command(async () => await AddCategorie());
            EditCategorieCommand = new Command<Categorie>(async (categorie) => await EditCategorie(categorie), (categorie) => categorie != null);
            DeleteCategorieCommand = new Command<Categorie>(async (categorie) => await DeleteCategorie(categorie), (categorie) => categorie != null);
            RefreshCommand = new Command(async () => await LoadCategories());

            // Charger les catégories au démarrage
            Task.Run(async () => await LoadCategories());
        }

        private async Task LoadCategories()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                var categories = await _menuService.GetAllCategoriesAsync();

                Categories.Clear();
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
                IsLoading = false;
            }
        }

        private async Task AddCategorie()
        {
            await Shell.Current.GoToAsync("edit_categorie");
        }

        private async Task EditCategorie(Categorie categorie)
        {
            if (categorie == null) return;

            var navigationParameter = new Dictionary<string, object>
            {
                { "categorie", categorie }
            };

            await Shell.Current.GoToAsync("edit_categorie", navigationParameter);
        }

        private async Task DeleteCategorie(Categorie categorie)
        {
            if (categorie == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirmation", 
                $"Êtes-vous sûr de vouloir supprimer la catégorie '{categorie.Nom}' ?", 
                "Oui", "Non");

            if (!confirm) return;

            try
            {
                IsLoading = true;
                await _menuService.DeleteCategorieAsync(categorie.CategorieID);
                Categories.Remove(categorie);
                await Application.Current.MainPage.DisplayAlert("Succès", "Catégorie supprimée avec succès", "OK");
            }
            catch (InvalidOperationException ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erreur", ex.Message, "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erreur", $"Erreur lors de la suppression: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}