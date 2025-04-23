namespace restaurant.Models;

public class Paiement
{
    public int PaiementID { get; set; }
    public int CommandeID { get; set; }
    public decimal Montant { get; set; }
    public string MethodePaiement { get; set; } // Carte, Espèces, etc.
    public DateTime DateHeure { get; set; }
    public string Reference { get; set; }
        
    // Navigation property
    public Commande Commande { get; set; }
}