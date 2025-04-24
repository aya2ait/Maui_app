using System;
using Microsoft.Maui.Graphics;

namespace restaurant.Models
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public string Titre { get; set; }
        public string Message { get; set; }
        public DateTime DateCreation { get; set; }
        public bool EstLue { get; set; }
        public TypeNotification Type { get; set; }
        
        // Propriété calculée pour faciliter les conditions dans le XAML
        public bool EstNonLue => !EstLue;
        
        // Propriété pour l'affichage de la date formatée
        public string DateFormatee => DateCreation.ToString("dd/MM/yyyy HH:mm");
        
        // Propriété pour la couleur selon le type
        public Color CouleurNotification => Type switch
        {
            TypeNotification.Information => Colors.Blue,
            TypeNotification.Avertissement => Colors.Orange,
            TypeNotification.Erreur => Colors.Red,
            TypeNotification.Succes => Colors.Green,
            _ => Colors.Gray
        };
        
        // Propriété pour l'icône selon le type
        public string IconeNotification => Type switch
        {
            TypeNotification.Information => "info_icon.png",
            TypeNotification.Avertissement => "warning_icon.png",
            TypeNotification.Erreur => "error_icon.png",
            TypeNotification.Succes => "success_icon.png",
            _ => "notification_icon.png"
        };
        
        // Propriété pour la couleur de fond
        public Color BackgroundColor => EstLue ? Colors.White : Colors.LightGray.WithAlpha(0.3f);
    }
    
    public enum TypeNotification
    {
        Information,
        Avertissement,
        Erreur,
        Succes
    }
}