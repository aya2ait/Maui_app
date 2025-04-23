using Microsoft.Maui.Controls;
using restaurant.Services;
using restaurant.Views;

namespace restaurant
{
   
    public partial class App : Application
    {
        private readonly AuthService authService;

        public App(DatabaseService databaseService, AuthService authService, PanierService panierService)
        {
            InitializeComponent();

            this.authService = authService;
        
            // Toujours utiliser le Shell, mais configurer la page d'accueil plus tard
            MainPage = new AppShell(databaseService, authService, panierService);
        }
    }
        
    
}