using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using restaurant.Models;
using restaurant.Services;

namespace restaurant.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private string _nom;
        private string _prenom;
        private string _email;
        private string _password;
        private string _confirmPassword;
        private string _telephone;
        private bool _isBusy;
        private string _errorMessage;

        public string Nom
        {
            get => _nom;
            set
            {
                if (_nom != value)
                {
                    _nom = value;
                    OnPropertyChanged(nameof(Nom));
                }
            }
        }

        public string Prenom
        {
            get => _prenom;
            set
            {
                if (_prenom != value)
                {
                    _prenom = value;
                    OnPropertyChanged(nameof(Prenom));
                }
            }
        }

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

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (_confirmPassword != value)
                {
                    _confirmPassword = value;
                    OnPropertyChanged(nameof(ConfirmPassword));
                }
            }
        }

        public string Telephone
        {
            get => _telephone;
            set
            {
                if (_telephone != value)
                {
                    _telephone = value;
                    OnPropertyChanged(nameof(Telephone));
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
                    ((Command)RegisterCommand).ChangeCanExecute();
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

        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<bool> RegistrationCompleted;

        public RegisterViewModel(AuthService authService)
        {
            _authService = authService;
            
            RegisterCommand = new Command(
                async () => await ExecuteRegisterCommand(),
                () => !IsBusy && ValidateForm()
            );
            
            NavigateToLoginCommand = new Command(
                () => RegistrationCompleted?.Invoke(this, false)
            );
        }

        private bool ValidateForm()
        {
            // Validation basique
            return !string.IsNullOrWhiteSpace(Nom) &&
                   !string.IsNullOrWhiteSpace(Prenom) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   Password == ConfirmPassword;
        }

        private async Task ExecuteRegisterCommand()
        {
            if (IsBusy)
                return;

            if (!ValidateForm())
            {
                ErrorMessage = "Veuillez remplir tous les champs correctement.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Les mots de passe ne correspondent pas.";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var newUser = new Utilisateur
                {
                    Nom = Nom,
                    Prenom = Prenom,
                    Email = Email,
                    Telephone = Telephone,
                    TypeUtilisateur = "Client", // Par défaut, les nouveaux utilisateurs sont des clients
                    DateInscription = DateTime.Now
                };

                bool result = await _authService.RegisterAsync(newUser, Password);
                
                if (result)
                {
                    // Inscription réussie
                    RegistrationCompleted?.Invoke(this, true);
                }
                else
                {
                    // Échec d'inscription
                    ErrorMessage = "Impossible de créer le compte. Cet email est peut-être déjà utilisé.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur d'inscription: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            
            // Mettre à jour l'état du bouton d'inscription lorsque les propriétés changent
            if (propertyName == nameof(Nom) || 
                propertyName == nameof(Prenom) || 
                propertyName == nameof(Email) || 
                propertyName == nameof(Password) || 
                propertyName == nameof(ConfirmPassword))
            {
                ((Command)RegisterCommand).ChangeCanExecute();
            }
        }
    }
}