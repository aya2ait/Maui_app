using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using restaurant.Models;
using restaurant.Services;
using Microsoft.Maui.Controls;

namespace restaurant.ViewModels
{
    public class PlatsListViewModel : BaseViewModel
    {
        private readonly MenuService _menuService;
        private ObservableCollection<Plat> _plats;
        private Plat _selectedPlat;
        private int? _selectedCategorieId;

        public ObservableCollection<Plat> Plats
        {
            get => _plats;
            set => SetProperty(ref _plats, value);
        }

        public Plat SelectedPlat
        {
            get => _selectedPlat;
            set => SetProperty(ref _selectedPlat, value);
        }

        public int? SelectedCategorieId
        {
            get => _selectedCategorieId;
            set
            {
                if (SetProperty(ref _selectedCategorieId, value))
                {
                    LoadPlatsCommand.Execute(null);
                }
            }
        }

        public ICommand LoadPlatsCommand { get; }
        public ICommand AddPlatCommand { get; }
        public ICommand EditPlatCommand { get; }
        public ICommand DeletePlatCommand { get; }
        public ICommand ToggleDisponibiliteCommand { get; }

        public PlatsListViewModel(MenuService menuService)
        {
            Title = "Plats";
            _menuService = menuService;
            Plats = new ObservableCollection<Plat>();

            LoadPlatsCommand = new Command(async () => await LoadPlatsAsync());
            AddPlatCommand = new Command(async () => await NavigateToAddPlatAsync());
            EditPlatCommand = new Command<Plat>(async (plat) => await NavigateToEditPlatAsync(plat));
            DeletePlatCommand = new Command<Plat>(async (plat) => await DeletePlatAsync(plat));
            ToggleDisponibiliteCommand = new Command<Plat>(async (plat) => await TogglePlatDisponibiliteAsync(plat));
        }

        private async Task LoadPlatsAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                Plats.Clear();
                var plats = _selectedCategorieId.HasValue 
                    ? await _menuService.GetPlatsByCategorieAsync(_selectedCategorieId.Value)
                    : await _menuService.GetAllPlatsAsync();
                
                foreach (var plat in plats)
                {
                    Plats.Add(plat);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task NavigateToAddPlatAsync()
        {
            await Shell.Current.GoToAsync("platdetail");
        }

        private async Task NavigateToEditPlatAsync(Plat plat)
        {
            if (plat == null) return;

            var navigationParameter = new Dictionary<string, object>
            {
                { "PlatId", plat.PlatID }
            };

            await Shell.Current.GoToAsync("platdetail", navigationParameter);
        }

        private async Task DeletePlatAsync(Plat plat)
        {
            if (plat == null) return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Confirmation", 
                $"Voulez-vous vraiment supprimer le plat '{plat.Nom}' ?", 
                "Oui", "Non");

            if (!confirm) return;

            IsBusy = true;
            try
            {
                await _menuService.DeletePlatAsync(plat.PlatID);
                Plats.Remove(plat);
            }
            catch (InvalidOperationException ex)
            {
                await Shell.Current.DisplayAlert("Erreur", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task TogglePlatDisponibiliteAsync(Plat plat)
        {
            if (plat == null) return;

            IsBusy = true;
            try
            {
                bool newStatus = !plat.EstDisponible;
                await _menuService.UpdatePlatDisponibiliteAsync(plat.PlatID, newStatus);
                plat.EstDisponible = newStatus;
                
                // Forcer la mise à jour de l'UI
                int index = Plats.IndexOf(plat);
                if (index >= 0)
                {
                    Plats[index] = plat;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}