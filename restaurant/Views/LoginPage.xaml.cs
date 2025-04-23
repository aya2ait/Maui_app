using Microsoft.Maui.Controls;
using restaurant.ViewModels;
using restaurant.Services;

namespace restaurant.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly LoginViewModel _viewModel;

        public LoginPage(AuthService authService)
        {
            InitializeComponent();
            
            _viewModel = new LoginViewModel(authService);
            _viewModel.LoginCompleted += OnLoginCompleted;
            BindingContext = _viewModel;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.LoginCompleted -= OnLoginCompleted;
        }

        private void OnLoginCompleted(object sender, bool success)
        {
            if (success)
            {
                // Accéder à l'AppShell et appeler sa méthode de navigation
                var appShell = Application.Current.MainPage as AppShell;
                appShell?.NavigateToMainAfterLogin();
            }
            else
            {
                // Navigation vers la page d'inscription
                var appShell = Application.Current.MainPage as AppShell;
                appShell?.NavigateToRegister();
            }
        }
    }
}