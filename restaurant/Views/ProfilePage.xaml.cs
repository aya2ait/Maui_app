using restaurant.Services;
using restaurant.ViewModels;

namespace restaurant.Views
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ProfileViewModel _viewModel;

        public ProfilePage(AuthService authService)
        {
            InitializeComponent();
            
            _viewModel = new ProfileViewModel(authService);
            BindingContext = _viewModel;
            
            _viewModel.LogoutRequested += OnLogoutRequested;
        }

        private async void OnLogoutRequested(object sender, EventArgs e)
        {
            // Rediriger vers la page de connexion
            await Shell.Current.GoToAsync("//login");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.LogoutRequested -= OnLogoutRequested;
        }
    }
}