using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using restaurant.Services;
using restaurant.ViewModels;
using restaurant.Views;

namespace restaurant
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Configuration de la chaîne de connexion
            string connectionString = "server=localhost;database=Restaurantdb;user=root;password=";

            // Enregistrement des services
            builder.Services.AddSingleton(new DatabaseService(connectionString));
            builder.Services.AddSingleton<PanierService>();
            // builder.Services.AddSingleton<App>(s => new App(databaseService, panierService));

            builder.Services.AddSingleton<AuthService>();

            // Enregistrement des pages
            builder.Services.AddTransient<CategoriesPage>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddTransient<PlatsPage>();
            builder.Services.AddTransient<PanierPage>();
            builder.Services.AddSingleton<App>();
            // Enregistrement des pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddSingleton<ReservationService>();
            builder.Services.AddTransient<CreateReservationPage>();
            builder.Services.AddTransient<UserReservationsPage>();
            // Dans la méthode ConfigureServices de votre MauiProgram.cs, ajoutez :
            builder.Services.AddTransient<PaiementViewModel>();
            builder.Services.AddTransient<PaiementPage>();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}