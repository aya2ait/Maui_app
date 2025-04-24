using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using restaurant.Models;
using restaurant.Services;
using Microsoft.Maui.Controls;

namespace restaurant.ViewModels
{
    [QueryProperty(nameof(PlatId), "PlatId")]
    public class PlatDetailViewModel : BaseViewModel
    {
        private readonly MenuService _menuService;
        private int _platId;
        private string _nom;
        private string _description;
        private decimal _prix;
        private string _imageUrl;
        private bool _estDisponible;
        private Categorie _selectedCategorie;
        private ObservableCollection<Categorie> _categories;

        public int PlatId
        {
            get => _platId;
            set
            {
                _platId = value;
                LoadPlatCommand.Execute(null);
            }
        }

        public string Nom
        {
            get => _nom;
            set => SetProperty(ref _nom, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public decimal Prix
        {
            get => _prix;
            set => SetProperty(ref _prix, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set
            {
                if (SetProperty(ref _imageUrl, value))
                {
                    OnPropertyChanged(nameof(ImageUrlHasValue));
                }
            }
        }

        public bool EstDisponible
        {
            get => _estDisponible;
            set => SetProperty(ref _estDisponible, value);
        }

        public Categorie SelectedCategorie
        {
            get => _selectedCategorie;
            set => SetProperty(ref _selectedCategorie, value);
        }

        public ObservableCollection<Categorie> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        // Propriété calculée qui remplace le convertisseur StringToBool
        public bool ImageUrlHasValue => !string.IsNullOrEmpty(ImageUrl);

        public ICommand LoadPlatCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand LoadCategoriesCommand { get; }

        public PlatDetailViewModel(MenuService menuService)
        {
            _menuService = menuService;
            Categories = new ObservableCollection<Categorie>();

            Title = "Détails du Plat";
            EstDisponible = true; // Par défaut un nouveau plat est disponible

            LoadPlatCommand = new Command(async () => await LoadPlatAsync());
            SaveCommand = new Command(async () => await SavePlatAsync());
            CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            LoadCategoriesCommand = new Command(async () => await LoadCategoriesAsync());

            // Charger les catégories au démarrage
            LoadCategoriesCommand.Execute(null);
        }

        private async Task LoadCategoriesAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                Categories.Clear();
                var categories = await _menuService.GetAllCategoriesAsync();
                foreach (var categorie in categories)
                {
                    Categories.Add(categorie);
                }

                // Si on n'a pas sélectionné de catégorie et qu'il y en a au moins une disponible
                if (SelectedCategorie == null && Categories.Count > 0)
                {
                    SelectedCategorie = Categories[0];
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadPlatAsync()
        {
            if (PlatId <= 0)
            {
                // Nouveau plat
                Title = "Nouveau Plat";
                return;
            }

            IsBusy = true;
            try
            {
                var plat = await _menuService.GetPlatByIdAsync(PlatId);
                if (plat != null)
                {
                    Title = $"Modifier {plat.Nom}";
                    Nom = plat.Nom;
                    Description = plat.Description;
                    Prix = plat.Prix;
                    ImageUrl = plat.ImageUrl;
                    EstDisponible = plat.EstDisponible;
                    
                    // Trouver la catégorie correspondante
                    var categorie = Categories.FirstOrDefault(c => c.CategorieID == plat.CategorieID);
                    if (categorie != null)
                    {
                        SelectedCategorie = categorie;
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SavePlatAsync()
        {
            if (string.IsNullOrWhiteSpace(Nom))
            {
                await Shell.Current.DisplayAlert("Erreur", "Le nom du plat est obligatoire", "OK");
                return;
            }

            if (Prix <= 0)
            {
                await Shell.Current.DisplayAlert("Erreur", "Le prix doit être supérieur à 0", "OK");
                return;
            }

            if (SelectedCategorie == null)
            {
                await Shell.Current.DisplayAlert("Erreur", "Veuillez sélectionner une catégorie", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var plat = new Plat
                {
                    PlatID = PlatId,
                    Nom = Nom,
                    Description = Description,
                    Prix = Prix,
                    ImageUrl = ImageUrl,
                    EstDisponible = EstDisponible,
                    CategorieID = SelectedCategorie.CategorieID
                };

                if (PlatId > 0)
                {
                    await _menuService.UpdatePlatAsync(plat);
                }
                else
                {
                    var newId = await _menuService.AddPlatAsync(plat);
                    plat.PlatID = newId;
                }

                await Shell.Current.GoToAsync("..");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}