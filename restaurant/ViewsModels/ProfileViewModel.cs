using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using restaurant.Models;
using restaurant.Services;

namespace restaurant.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private Utilisateur _utilisateur;
        private bool _isEditing;
        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;
        private bool _isBusy;
        private string _message;
        private bool _isSuccess;

        public Utilisateur Utilisateur
        {
            get => _utilisateur;
            set
            {
                if (_utilisateur != value)
                {
                    _utilisateur = value;
                    OnPropertyChanged(nameof(Utilisateur));
                }
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged(nameof(IsEditing));
                }
            }
        }

        public string CurrentPassword
        {
            get => _currentPassword;
            set
            {
                if (_currentPassword != value)
                {
                    _currentPassword = value;
                    OnPropertyChanged(nameof(CurrentPassword));
                }
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                if (_newPassword != value)
                {
                    _newPassword = value;
                    OnPropertyChanged(nameof(NewPassword));
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

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                }
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged(nameof(Message));
                    OnPropertyChanged(nameof(HasMessage));
                }
            }
        }

        public bool IsSuccess
        {
            get => _isSuccess;
            set
            {
                if (_isSuccess != value)
                {
                    _isSuccess = value;
                    OnPropertyChanged(nameof(IsSuccess));
                }
            }
        }

        public bool HasMessage => !string.IsNullOrEmpty(Message);

        public ICommand EditProfileCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand LogoutCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler LogoutRequested;

        public ProfileViewModel(AuthService authService)
        {
            _authService = authService;
            
            // Charger les informations de l'utilisateur actuel
            Utilisateur = _authService.CurrentUser;
            
            EditProfileCommand = new Command(() => IsEditing = true);
            
            SaveProfileCommand = new Command(
                async () => await ExecuteSaveProfileCommand(),
                () => !IsBusy
            );
            
            CancelEditCommand = new Command(() => 
            {
                IsEditing = false;
                Utilisateur = _authService.CurrentUser; // Réinitialiser les modifications
            });
            
            ChangePasswordCommand = new Command(
                async () => await ExecuteChangePasswordCommand(),
                () => !IsBusy && !string.IsNullOrWhiteSpace(CurrentPassword) && 
                      !string.IsNullOrWhiteSpace(NewPassword) && 
                      !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                      NewPassword == ConfirmPassword
            );
            
            LogoutCommand = new Command(() => 
            {
                _authService.Logout();
                LogoutRequested?.Invoke(this, EventArgs.Empty);
            });

            // S'abonner aux changements d'authentification
            _authService.AuthenticationChanged += (s, e) => 
            {
                Utilisateur = _authService.CurrentUser;
            };
        }

        private async Task ExecuteSaveProfileCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            Message = string.Empty;

            try
            {
                bool result = await _authService.UpdateProfileAsync(Utilisateur);
                
                if (result)
                {
                    IsSuccess = true;
                    Message = "Profil mis à jour avec succès.";
                    IsEditing = false;
                }
                else
                {
                    IsSuccess = false;
                    Message = "Impossible de mettre à jour le profil.";
                }
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                Message = $"Erreur: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecuteChangePasswordCommand()
        {
            if (IsBusy)
                return;

            if (NewPassword != ConfirmPassword)
            {
                IsSuccess = false;
                Message = "Les mots de passe ne correspondent pas.";
                return;
            }

            IsBusy = true;
            Message = string.Empty;

            try
            {
                bool result = await _authService.ChangePasswordAsync(
                    Utilisateur.UtilisateurID, 
                    CurrentPassword, 
                    NewPassword);
                
                if (result)
                {
                    IsSuccess = true;
                    Message = "Mot de passe changé avec succès.";
                    CurrentPassword = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmPassword = string.Empty;
                }
                else
                {
                    IsSuccess = false;
                    Message = "Impossible de changer le mot de passe. Vérifiez votre mot de passe actuel.";
                }
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                Message = $"Erreur: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            
            if (propertyName == nameof(CurrentPassword) || 
                propertyName == nameof(NewPassword) || 
                propertyName == nameof(ConfirmPassword))
            {
                ((Command)ChangePasswordCommand).ChangeCanExecute();
            }
        }
    }
}