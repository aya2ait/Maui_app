using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using restaurant.Services;

namespace restaurant.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private string _email;
        private string _password;
        private bool _isBusy;
        private string _errorMessage;

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged(nameof(Email));
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                    ((Command)LoginCommand).ChangeCanExecute();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                    OnPropertyChanged(nameof(HasError));
                }
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<bool> LoginCompleted;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
            
            LoginCommand = new Command(
                async () => await ExecuteLoginCommand(),
                () => !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password)
            );
            
            NavigateToRegisterCommand = new Command(
                () => LoginCompleted?.Invoke(this, false)
            );
        }

        private async Task ExecuteLoginCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                bool result = await _authService.LoginAsync(Email, Password);
                
                if (result)
                {
                    // Connexion réussie
                    LoginCompleted?.Invoke(this, true);
                }
                else
                {
                    // Échec de connexion
                    ErrorMessage = "Email ou mot de passe incorrect.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de connexion: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            
            if (propertyName == nameof(Email) || propertyName == nameof(Password))
            {
                ((Command)LoginCommand).ChangeCanExecute();
            }
        }
    }
}