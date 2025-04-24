using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using restaurant.Models;
using restaurant.Services;

namespace restaurant.ViewModels
{
    [QueryProperty(nameof(Categorie), "categorie")]
    public class EditCategorieViewModel : INotifyPropertyChanged
    {
        private readonly MenuService _menuService;
        private Categorie _categorie;
        private bool _isLoading;
        private bool _isNewCategorie;

        public Categorie Categorie
        {
            get => _categorie;
            set
            {
                _categorie = value;
                _isNewCategorie = value == null;
                if (_isNewCategorie)
                {
                    _categorie = new Categorie();
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(PageTitle));
                OnPropertyChanged(nameof(ButtonText));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string PageTitle => _isNewCategorie ? "Ajouter une catégorie" : "Modifier une catégorie";
        public string ButtonText => _isNewCategorie ? "Ajouter" : "Enregistrer";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public EditCategorieViewModel(MenuService menuService)
        {
            _menuService = menuService;
            _categorie = new Categorie(); // Initialiser avec un objet vide par défaut
            _isNewCategorie = true;
            SaveCommand = new Command(async () => await SaveCategorieAsync());
            CancelCommand = new Command(async () => await CancelAsync());
        }

        private async Task SaveCategorieAsync()
        {
            if (string.IsNullOrWhiteSpace(Categorie.Nom))
            {
                await Application.Current.MainPage.DisplayAlert("Erreur", "Le nom de la catégorie est obligatoire", "OK");
                return;
            }

            try
            {
                IsLoading = true;

                if (_isNewCategorie)
                {
                    int id = await _menuService.AddCategorieAsync(Categorie);
                    Categorie.CategorieID = id;
                    await Application.Current.MainPage.DisplayAlert("Succès", "Catégorie ajoutée avec succès", "OK");
                }
                else
                {
                    await _menuService.UpdateCategorieAsync(Categorie);
                    await Application.Current.MainPage.DisplayAlert("Succès", "Catégorie modifiée avec succès", "OK");
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erreur", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}