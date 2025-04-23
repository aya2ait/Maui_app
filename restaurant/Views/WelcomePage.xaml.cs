using Microsoft.Maui.Controls;
using restaurant.ViewModels;

namespace restaurant.Views
{
    public partial class WelcomePage : ContentPage
    {
        public WelcomePage()
        {
            InitializeComponent();
            
            BindingContext = new WelcomeViewModel();
        }
    }
}