namespace restaurant.Models;

public class Utilisateur
{
    public int UtilisateurID { get; set; }
    public string Nom { get; set; }
    public string Prenom { get; set; }
    public string Email { get; set; }
    public string MotDePasse { get; set; } // En production, ne jamais stocker en clair
    public string Telephone { get; set; }
    public string TypeUtilisateur { get; set; } // Client, Serveur, Gérant, Admin
    public DateTime DateInscription { get; set; }
    public string NomComplet => $"{Prenom} {Nom}";

}