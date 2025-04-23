using Microsoft.Maui.Controls;
using System.Windows.Input;
using System;

namespace restaurant.ViewModels
{
    public class WelcomeViewModel : BaseViewModel
    {
        public ICommand LoginCommand { get; private set; }
        public ICommand RegisterCommand { get; private set; }
        public ICommand ViewMenuCommand { get; private set; }

        public WelcomeViewModel()
        {
            // Initialiser les commandes avec une méthode qui obtient AppShell au moment de l'exécution
            LoginCommand = new Command(ExecuteLoginCommand);
            RegisterCommand = new Command(ExecuteRegisterCommand);
            ViewMenuCommand = new Command(async () => await Shell.Current.GoToAsync("//CategoriesPage"));
        }

        private void ExecuteLoginCommand()
        {
            try
            {
                var appShell = Shell.Current as AppShell;
                if (appShell != null)
                {
                    appShell.NavigateToLogin();
                }
                else
                {
                    // Fallback si le casting échoue
                    Shell.Current.GoToAsync("//login");
                }
            }
            catch (Exception ex)
            {
                // Log l'exception ou afficher un message d'erreur
                Console.WriteLine($"Erreur lors de la navigation: {ex.Message}");
            }
        }

        private void ExecuteRegisterCommand()
        {
            try
            {
                var appShell = Shell.Current as AppShell;
                if (appShell != null)
                {
                    appShell.NavigateToRegister();
                }
                else
                {
                    // Fallback si le casting échoue
                    Shell.Current.GoToAsync("//register");
                }
            }
            catch (Exception ex)
            {
                // Log l'exception ou afficher un message d'erreur
                Console.WriteLine($"Erreur lors de la navigation: {ex.Message}");
            }
        }
    }
}