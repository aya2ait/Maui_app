namespace restaurant.Models;

public class Reservation
{
    public int ReservationID { get; set; }
    public int UtilisateurID { get; set; }
    public int TableID { get; set; }
    public DateTime DateHeure { get; set; }
    public int NombrePersonnes { get; set; }
    public string Statut { get; set; } // Confirmée, Annulée, Terminée
    public string Notes { get; set; }
        
    // Navigation properties
    public Utilisateur Utilisateur { get; set; }
    public Table Table { get; set; }
    // Propriétés de formatage pour l'affichage
    public string DateHeureFormatted => DateHeure.ToString("dd/MM/yyyy HH:mm");
    public string StatusColor => Statut == "Confirmée" ? "Green" : Statut == "Annulée" ? "Red" : "Orange";
  
}