using restaurant.Services;
using restaurant.ViewModels;

namespace restaurant.Views
{
    public partial class RegisterPage : ContentPage
    {
        private readonly RegisterViewModel _viewModel;

        public RegisterPage(AuthService authService)
        {
            InitializeComponent();
            
            _viewModel = new RegisterViewModel(authService);
            BindingContext = _viewModel;
            
            _viewModel.RegistrationCompleted += OnRegistrationCompleted;
        }

       

        private async void OnRegistrationCompleted(object sender, bool success)
        {
            if (success)
            {
                // Rediriger vers la page principale après inscription réussie
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                // Retourner à la page de connexion
                await Shell.Current.GoToAsync("..");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.RegistrationCompleted -= OnRegistrationCompleted;
        }
    }
}