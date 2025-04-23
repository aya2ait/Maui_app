using System;
using restaurant.Services;
using restaurant.ViewModels;

namespace restaurant.Views
{
    public partial class UserReservationsPage : ContentPage
    {
        private readonly UserReservationsViewModel _viewModel;
        private readonly ReservationService _reservationService;
        private readonly AuthService _authService;

        public UserReservationsPage(ReservationService reservationService, AuthService authService)
        {
            InitializeComponent();
            _reservationService = reservationService;
            _authService = authService;
            
            _viewModel = new UserReservationsViewModel(_reservationService, _authService);
            BindingContext = _viewModel;
            
            // S'abonner aux événements
            _viewModel.OperationCompleted += OnOperationCompleted;
            _viewModel.NavigateToNewReservation += OnNavigateToNewReservation;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            if (!_viewModel.IsUserAuthenticated())
            {
                await DisplayAlert("Erreur", "Vous devez être connecté pour voir vos réservations.", "OK");
                await Navigation.PopAsync();
                return;
            }
            
            await _viewModel.LoadReservations();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Se désabonner des événements
            _viewModel.OperationCompleted -= OnOperationCompleted;
            _viewModel.NavigateToNewReservation -= OnNavigateToNewReservation;
        }

        private async void OnOperationCompleted(bool success, string message)
        {
            await DisplayAlert(success ? "Succès" : "Erreur", message, "OK");
        }

        private async void OnNavigateToNewReservation()
        {
            await Navigation.PushAsync(new CreateReservationPage());
        }
    }
}