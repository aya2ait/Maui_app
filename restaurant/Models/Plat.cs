using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace restaurant.Models
{
    public class Plat : INotifyPropertyChanged
    {
        public int PlatID { get; set; }
        public int CategorieID { get; set; }
        public string Nom { get; set; }
        public string Description { get; set; }
        public decimal Prix { get; set; }
        public string ImageUrl { get; set; }
        
        private bool _estDisponible;
        public bool EstDisponible 
        { 
            get => _estDisponible;
            set
            {
                if (_estDisponible != value)
                {
                    _estDisponible = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusText));
                    OnPropertyChanged(nameof(StatusColor));
                    OnPropertyChanged(nameof(DisponibiliteButtonText));
                    OnPropertyChanged(nameof(DisponibiliteButtonColor));
                }
            }
        }

        // Propriétés calculées pour remplacer les convertisseurs
        public string StatusText => EstDisponible ? "Oui" : "Non";
        public Color StatusColor => EstDisponible ? Colors.Green : Colors.Red;
        public string DisponibiliteButtonText => EstDisponible ? "Rendre indisponible" : "Rendre disponible";
        public Color DisponibiliteButtonColor => EstDisponible ? Colors.Orange : Colors.Green;

        // Navigation property
        public Categorie Categorie { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}