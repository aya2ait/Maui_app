using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using restaurant.Models;
using restaurant.Services;

namespace restaurant.ViewModels
{
    public class UserReservationsViewModel : INotifyPropertyChanged
    {
        private readonly ReservationService _reservationService;
        private readonly AuthService _authService;

        // Collection des réservations de l'utilisateur
        private ObservableCollection<ReservationDetail> _reservations = new ObservableCollection<ReservationDetail>();
        
        public ObservableCollection<ReservationDetail> Reservations
        {
            get => _reservations;
            set
            {
                _reservations = value;
                OnPropertyChanged();
            }
        }

        // État du rafraîchissement
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        // Commandes
        public ICommand RefreshCommand { get; }
        public ICommand CancelReservationCommand { get; }
        public ICommand NewReservationCommand { get; }

        // Events pour la communication avec la vue
        public event Action<bool, string> OperationCompleted;
        public event Action NavigateToNewReservation;

        public UserReservationsViewModel(ReservationService reservationService, AuthService authService)
        {
            _reservationService = reservationService;
            _authService = authService;
            
            RefreshCommand = new Command(async () => await LoadReservations());
            CancelReservationCommand = new Command<int>(async (id) => await CancelReservation(id));
            NewReservationCommand = new Command(() => NavigateToNewReservation?.Invoke());
        }

        // Vérifier si l'utilisateur est connecté
        public bool IsUserAuthenticated()
        {
            return _authService.IsAuthenticated;
        }

        // Charger les réservations de l'utilisateur
        public async Task LoadReservations()
        {
            try
            {
                if (!IsUserAuthenticated())
                {
                    OperationCompleted?.Invoke(false, "Vous devez être connecté pour voir vos réservations.");
                    return;
                }

                IsRefreshing = true;
                
                var reservations = await _reservationService.GetUserReservationsAsync();
                
                Reservations.Clear();
                foreach (var reservation in reservations)
                {
                    Reservations.Add(reservation);
                }
            }
            catch (Exception ex)
            {
                OperationCompleted?.Invoke(false, $"Une erreur est survenue: {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        // Méthode pour annuler une réservation
        public async Task<bool> CancelReservation(int reservationId)
        {
            try
            {
                bool success = await _reservationService.CancelReservationAsync(reservationId);
                
                if (success)
                {
                    await LoadReservations();
                    OperationCompleted?.Invoke(true, "La réservation a été annulée avec succès.");
                    return true;
                }
                else
                {
                    OperationCompleted?.Invoke(false, "Impossible d'annuler cette réservation.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OperationCompleted?.Invoke(false, $"Une erreur est survenue: {ex.Message}");
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}