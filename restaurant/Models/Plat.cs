namespace restaurant.Models;

public class Plat
{
    public int PlatID { get; set; }
    public int CategorieID { get; set; }
    public string Nom { get; set; }
    public string Description { get; set; }
    public decimal Prix { get; set; }
    public string ImageUrl { get; set; }
    public bool EstDisponible { get; set; }
        
    // Navigation property
    public Categorie Categorie { get; set; }
}