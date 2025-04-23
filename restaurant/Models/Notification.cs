
namespace restaurant.Models;

    public class Notification
    {
        public int NotificationID { get; set; }
        public int UtilisateurID { get; set; }
        public string Titre { get; set; }
        public string Message { get; set; }
        public DateTime DateHeure { get; set; }
        public bool EstLue { get; set; }

        // Navigation property
        public Utilisateur Utilisateur { get; set; } 
    }