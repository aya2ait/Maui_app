namespace restaurant.Models;
// Modèle étendu pour afficher les détails des réservations
public class ReservationDetail : Reservation
{
    public int NumeroTable { get; set; }
    public int CapaciteTable { get; set; }
        
    // Propriétés calculées pour l'affichage
    public string DateFormatee => DateHeure.ToString("dd/MM/yyyy");
    public string HeureFormatee => DateHeure.ToString("HH:mm");
    public string InfoTable => $"Table {NumeroTable} (Capacité: {CapaciteTable})";
    public bool EstAnnulable => DateHeure > DateTime.Now && Statut == "Confirmée";
    public string StatusColor => Statut == "Confirmée" ? "Green" : 
        Statut == "Annulée" ? "Red" : "Orange";
}