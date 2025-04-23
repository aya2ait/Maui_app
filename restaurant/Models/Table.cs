namespace restaurant.Models;

public class Table
{
    public int TableID { get; set; }
    public int Numero { get; set; }
    public int Capacite { get; set; }
    public string Statut { get; set; } // Disponible, Réservée, Occupée
    public string DisplayInfo => $"Table {Numero} (Capacité: {Capacite} pers.)";

}