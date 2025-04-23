namespace restaurant.Models;

public class Commande
{
    public int CommandeID { get; set; }
    public int UtilisateurID { get; set; }
    public int? TableID { get; set; } // Nullable pour les commandes à emporter
    public DateTime DateHeure { get; set; }
    public string Statut { get; set; } // En attente, En préparation, Servie, Payée
    public decimal Total { get; set; }
    public string Notes { get; set; }
        
    // Navigation properties
    public Utilisateur Utilisateur { get; set; }
    public Table Table { get; set; }
    public List<LigneCommande> LignesCommande { get; set; }
    public Paiement Paiement { get; set; }
}