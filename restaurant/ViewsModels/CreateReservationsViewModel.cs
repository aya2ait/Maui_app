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
    public class CreateReservationViewModel : INotifyPropertyChanged
    {
        private readonly ReservationService _reservationService;
        private readonly AuthService _authService;
        private readonly DatabaseService _databaseService;

        // Propriétés pour la date et heure de réservation
        private DateTime _reservationDate = DateTime.Today;
        // Add this constructor to CreateReservationViewModel.cs
        public CreateReservationViewModel(DatabaseService databaseService,AuthService authService)
        {
            _databaseService = databaseService;
            _authService = authService;
            
            SearchTablesCommand = new Command(async () => await SearchAvailableTables());
            ConfirmReservationCommand = new Command(async () => await ConfirmReservation());
        }
        public DateTime ReservationDate
        {
            get => _reservationDate;
            set
            {
                if (_reservationDate != value)
                {
                    _reservationDate = value;
                    OnPropertyChanged();
                    ShowTableSelection = false;
                }
            }
        }

        private TimeSpan _reservationTime = new TimeSpan(19, 0, 0); // Default to 7 PM
        public TimeSpan ReservationTime
        {
            get => _reservationTime;
            set
            {
                if (_reservationTime != value)
                {
                    _reservationTime = value;
                    OnPropertyChanged();
                    ShowTableSelection = false;
                }
            }
        }

        private int _nombrePersonnes = 2;
        public int NombrePersonnes
        {
            get => _nombrePersonnes;
            set
            {
                if (_nombrePersonnes != value)
                {
                    _nombrePersonnes = value;
                    OnPropertyChanged();
                    ShowTableSelection = false;
                }
            }
        }

        // Collection des tables disponibles
        private ObservableCollection<Table> _availableTables = new ObservableCollection<Table>();
        public ObservableCollection<Table> AvailableTables
        {
            get => _availableTables;
            set
            {
                _availableTables = value;
                OnPropertyChanged();
            }
        }

        // Table sélectionnée
        private Table _selectedTable;
        public Table SelectedTable
        {
            get => _selectedTable;
            set
            {
                if (_selectedTable != value)
                {
                    _selectedTable = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanConfirm));
                }
            }
        }

        // Notes pour la réservation
        private string _notes;
        public string Notes
        {
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged();
                }
            }
        }

        // États et messages
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _showTableSelection;
        public bool ShowTableSelection
        {
            get => _showTableSelection;
            set
            {
                if (_showTableSelection != value)
                {
                    _showTableSelection = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ShowConfirmButton));
                }
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ShowStatusMessage));
                }
            }
        }

        private string _statusMessageColor = "Green";
        public string StatusMessageColor
        {
            get => _statusMessageColor;
            set
            {
                if (_statusMessageColor != value)
                {
                    _statusMessageColor = value;
                    OnPropertyChanged();
                }
            }
        }

        // Propriétés calculées
        public bool ShowStatusMessage => !string.IsNullOrEmpty(StatusMessage);
        public bool ShowConfirmButton => ShowTableSelection && AvailableTables.Count > 0;
        public bool CanConfirm => SelectedTable != null;
        public DateTime MinimumDate => DateTime.Today;

        // Commandes
        public ICommand SearchTablesCommand { get; }
        public ICommand ConfirmReservationCommand { get; }

        // Event pour la navigation
        public event Action<bool, string> ReservationCompleted;

        public CreateReservationViewModel(ReservationService reservationService, AuthService authService)
        {
            _reservationService = reservationService;
            _authService = authService;

            SearchTablesCommand = new Command(async () => await SearchAvailableTables());
            ConfirmReservationCommand = new Command(async () => await ConfirmReservation());
        }

        // Vérifier si l'utilisateur est connecté
        public bool IsUserAuthenticated()
        {
            return _authService.IsAuthenticated;
        }

        // Méthode pour rechercher les tables disponibles
        public async Task SearchAvailableTables()
        {
            if (!ValidateInput())
                return;

            try
            {
                IsBusy = true;
                StatusMessage = "";

                DateTime reservationDateTime = ReservationDate.Date.Add(ReservationTime);
                
                // Vérifier si la date et l'heure sont dans le futur
                if (reservationDateTime <= DateTime.Now)
                {
                    ReservationCompleted?.Invoke(false, "La date et l'heure de réservation doivent être dans le futur.");
                    return;
                }

                var tables = await _reservationService.GetAvailableTablesAsync(reservationDateTime, NombrePersonnes);
                
                AvailableTables.Clear();
                foreach (var table in tables)
                {
                    AvailableTables.Add(table);
                }

                ShowTableSelection = true;
                
                if (AvailableTables.Count == 0)
                {
                    StatusMessage = "Aucune table disponible pour cette date et ce nombre de personnes.";
                    StatusMessageColor = "Red";
                }
            }
            catch (Exception ex)
            {
                ReservationCompleted?.Invoke(false, $"Une erreur est survenue: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Méthode pour confirmer la réservation
        public async Task ConfirmReservation()
        {
            if (SelectedTable == null)
            {
                ReservationCompleted?.Invoke(false, "Veuillez sélectionner une table.");
                return;
            }

            try
            {
                IsBusy = true;
                
                var reservation = new Reservation
                {
                    UtilisateurID = _authService.CurrentUser.UtilisateurID,
                    TableID = SelectedTable.TableID,
                    DateHeure = ReservationDate.Date.Add(ReservationTime),
                    NombrePersonnes = NombrePersonnes,
                    Notes = Notes,
                    Statut = "Confirmée"
                };

                int reservationId = await _reservationService.CreateReservationAsync(reservation);
                
                if (reservationId > 0)
                {
                    StatusMessage = "Réservation confirmée avec succès!";
                    StatusMessageColor = "Green";
                    
                    // Réinitialiser le formulaire
                    ReservationDate = DateTime.Today;
                    ReservationTime = new TimeSpan(19, 0, 0);
                    NombrePersonnes = 2;
                    Notes = string.Empty;
                    SelectedTable = null;
                    AvailableTables.Clear();
                    ShowTableSelection = false;
                    
                    ReservationCompleted?.Invoke(true, "Votre réservation a été confirmée.");
                }
                else
                {
                    StatusMessage = "Erreur lors de la création de la réservation.";
                    StatusMessageColor = "Red";
                    ReservationCompleted?.Invoke(false, "Erreur lors de la création de la réservation.");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Erreur lors de la création de la réservation.";
                StatusMessageColor = "Red";
                ReservationCompleted?.Invoke(false, $"Une erreur est survenue: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Validation des entrées
        private bool ValidateInput()
        {
            if (NombrePersonnes <= 0)
            {
                ReservationCompleted?.Invoke(false, "Le nombre de personnes doit être supérieur à 0.");
                return false;
            }

            if (ReservationDate.Date < DateTime.Today)
            {
                ReservationCompleted?.Invoke(false, "La date de réservation ne peut pas être dans le passé.");
                return false;
            }

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}