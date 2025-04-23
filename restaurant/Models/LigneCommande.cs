namespace restaurant.Models;

public class LigneCommande
{
    public int LigneCommandeID { get; set; }
    public int CommandeID { get; set; }
    public int PlatID { get; set; }
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public string Notes { get; set; }
        
    // Navigation properties
    public Commande Commande { get; set; }
    public Plat Plat { get; set; }
    public decimal SousTotal => Quantite * PrixUnitaire;

}