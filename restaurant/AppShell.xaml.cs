using Microsoft.Maui.Controls;
using restaurant.Services;
using restaurant.ViewModels;
using restaurant.Views;

namespace restaurant
{
    public partial class AppShell : Shell
    {
        private readonly DatabaseService databaseService;
        private readonly ShellViewModel viewModel;
        private readonly AuthService authService;
        
        public AppShell(DatabaseService databaseService, AuthService authService, PanierService panierService)
        {
            InitializeComponent();
            
            this.databaseService = databaseService;
            this.authService = authService;
            this.viewModel = new ShellViewModel(panierService);
            BindingContext = viewModel;
            
            // Enregistrement des routes avec leurs chemins complets
            Routing.RegisterRoute(nameof(CategoriesPage), typeof(CategoriesPage));
            Routing.RegisterRoute("login", typeof(LoginPage));
            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute(nameof(PlatsPage), typeof(PlatsPage));
            Routing.RegisterRoute("reservations", typeof(CreateReservationPage));
            Routing.RegisterRoute("mesreservations", typeof(UserReservationsPage));
            Routing.RegisterRoute("paiement", typeof(PaiementPage));


            


           // Routing.RegisterRoute(nameof(ReservationsPage), typeof(ReservationsPage));
            // Vérifier si l'utilisateur est déjà connecté
            if (authService.IsAuthenticated)
            {
                // Cacher la page d'accueil
                WelcomeTab.IsVisible = false;
                
                // Vérifier le type d'utilisateur et afficher la TabBar appropriée
                if (authService.CurrentUser.TypeUtilisateur == "Admin")
                {
                    MainTab.IsVisible = false;
                    AdminTab.IsVisible = true;
                }
                else
                {
                    MainTab.IsVisible = true;
                    AdminTab.IsVisible = false;
                    Current.GoToAsync("//main/menu/categories");
                }
            }
        }
        
        // Méthode pour gérer la navigation après authentification
        public void NavigateToMainAfterLogin()
        {
            WelcomeTab.IsVisible = false;
            AuthTab.IsVisible = false;
            
            // Vérifier le type d'utilisateur et afficher la TabBar appropriée
            if (authService.CurrentUser.TypeUtilisateur == "Admin")
            {
                MainTab.IsVisible = false;
                AdminTab.IsVisible = true;
            }
            else
            {
                MainTab.IsVisible = true;
                AdminTab.IsVisible = false;
                Dispatcher.Dispatch(async () => await Current.GoToAsync("//main/menu/categories"));
            }
        }
        
        // Méthodes existantes pour la navigation
        public void NavigateToLogin()
        {
            WelcomeTab.IsVisible = false;
            MainTab.IsVisible = false;
            AdminTab.IsVisible = false;
            AuthTab.IsVisible = true;
            Dispatcher.Dispatch(async () => await Current.GoToAsync("//auth/login"));
        }
        
        public void NavigateToRegister()
        {
            WelcomeTab.IsVisible = false;
            MainTab.IsVisible = false;
            AdminTab.IsVisible = false;
            AuthTab.IsVisible = true;
            Dispatcher.Dispatch(async () => await Current.GoToAsync("//auth/register"));
        }
        // Dans la méthode NavigateToReservations()
        // Méthode pour naviguer vers la page de réservations
        public void NavigateToReservations()
        {
            Dispatcher.Dispatch(async () => await Current.GoToAsync("//main/reservations"));

        }
        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);

            // Vérifier si nous naviguons vers la page de paiement
            if (args.Target.Location.OriginalString.Contains("paiement"))
            {
                // Extraire les paramètres manuellement depuis la chaîne d'URL
                string query = args.Target.Location.Query;
                if (!string.IsNullOrEmpty(query))
                {
                    // Supprimer le ? initial
                    if (query.StartsWith("?"))
                        query = query.Substring(1);
                
                    // Séparer les paramètres
                    var queryParams = query.Split('&')
                        .Select(p => p.Split('='))
                        .ToDictionary(p => p[0], p => Uri.UnescapeDataString(p[1]));
                
                    // Vérifier si les paramètres commandeId et montantTotal sont présents
                    if (queryParams.ContainsKey("commandeId") && queryParams.ContainsKey("montantTotal"))
                    {
                        try
                        {
                            // Obtenir l'instance de la page de paiement depuis le conteneur DI
                            var paiementPage = Handler.MauiContext.Services.GetService<PaiementPage>();
                    
                            if (paiementPage != null)
                            {
                                // Convertir les paramètres
                                int commandeId = int.Parse(queryParams["commandeId"]);
                                decimal montantTotal = decimal.Parse(queryParams["montantTotal"], 
                                    System.Globalization.CultureInfo.InvariantCulture);
                            
                                // Initialiser la page
                                paiementPage.InitialiserAvecCommande(commandeId, montantTotal);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erreur lors de l'initialisation de la page de paiement: {ex.Message}");
                        }
                    }
                }
            }
        }        
    }
}