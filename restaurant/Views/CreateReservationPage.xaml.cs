using restaurant.ViewModels;
using restaurant.Services;
using Microsoft.Extensions.DependencyInjection;

namespace restaurant.Views;

public partial class CreateReservationPage : ContentPage
{
    public CreateReservationPage()
    {
        InitializeComponent();
        
        // Récupérer les services nécessaires depuis le conteneur d'injection de dépendances
        var reservationService = Application.Current.Handler.MauiContext.Services.GetService<ReservationService>();
        var authService = Application.Current.Handler.MauiContext.Services.GetService<AuthService>();
        
        // Initialiser le ViewModel avec les services requis
        BindingContext = new CreateReservationViewModel(reservationService, authService);
    }
}