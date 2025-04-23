using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using restaurant.Models;

namespace restaurant.Services
{
    public class AuthService
    {
        private readonly DatabaseService _dbService;
        private Utilisateur _currentUser;

        public Utilisateur CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;

        public event EventHandler AuthenticationChanged;

        public AuthService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        // Hachage de mot de passe simple avec SHA256
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Inscription d'un nouvel utilisateur
        public async Task<bool> RegisterAsync(Utilisateur utilisateur, string password)
        {
            try
            {
                // Vérifier si l'email existe déjà
                var existingUser = await GetUserByEmailAsync(utilisateur.Email);
                if (existingUser != null)
                {
                    return false; // Email déjà utilisé
                }

                // Hacher le mot de passe
                string hashedPassword = HashPassword(password);
                
                string query = @"
                    INSERT INTO Utilisateurs (Nom, Prenom, Email, MotDePasse, Telephone, TypeUtilisateur, DateInscription)
                    VALUES (@Nom, @Prenom, @Email, @MotDePasse, @Telephone, @TypeUtilisateur, @DateInscription);
                    SELECT LAST_INSERT_ID();";

                var parameters = new Dictionary<string, object>
                {
                    { "@Nom", utilisateur.Nom },
                    { "@Prenom", utilisateur.Prenom },
                    { "@Email", utilisateur.Email },
                    { "@MotDePasse", hashedPassword },
                    { "@Telephone", utilisateur.Telephone },
                    { "@TypeUtilisateur", utilisateur.TypeUtilisateur ?? "Client" },
                    { "@DateInscription", DateTime.Now }
                };

                int userId = await _dbService.ExecuteScalarAsync<int>(query, parameters);
                utilisateur.UtilisateurID = userId;
                
                // Connecter automatiquement l'utilisateur après inscription
                _currentUser = utilisateur;
                AuthenticationChanged?.Invoke(this, EventArgs.Empty);
                
                return userId > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'inscription: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> LoginAsync(string email, string password)
{
    try
    {
        string hashedPassword = HashPassword(password);
        
        // Première tentative avec le mot de passe haché (pour les clients)
        string query = @"
            SELECT UtilisateurID, Nom, Prenom, Email, Telephone, TypeUtilisateur, DateInscription 
            FROM Utilisateurs 
            WHERE Email = @Email AND (MotDePasse = @HashedPassword OR (TypeUtilisateur = 'Admin' AND MotDePasse = @PlainPassword))";

        var parameters = new Dictionary<string, object>
        {
            { "@Email", email },
            { "@HashedPassword", hashedPassword },
            { "@PlainPassword", password } // Pour les administrateurs avec mot de passe en texte brut
        };

        var users = await _dbService.ExecuteReaderAsync(query, reader => new Utilisateur
        {
            UtilisateurID = reader.GetInt32("UtilisateurID"),
            Nom = reader.GetString("Nom"),
            Prenom = reader.GetString("Prenom"),
            Email = reader.GetString("Email"),
            Telephone = reader.IsDBNull(reader.GetOrdinal("Telephone")) ? null : reader.GetString("Telephone"),
            TypeUtilisateur = reader.GetString("TypeUtilisateur"),
            DateInscription = reader.GetDateTime("DateInscription")
        }, parameters);

        if (users.Count > 0)
        {
            _currentUser = users[0];
            AuthenticationChanged?.Invoke(this, EventArgs.Empty);
            
            // Si c'est un admin avec mot de passe en texte brut, mettre à jour son mot de passe
            if (_currentUser.TypeUtilisateur == "Admin" && _currentUser.MotDePasse == password)
            {
                await UpdateAdminPasswordAsync(_currentUser.UtilisateurID, password);
            }
            
            return true;
        }
        
        return false;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erreur lors de la connexion: {ex.Message}");
        return false;
    }
}

// Méthode pour mettre à jour le mot de passe d'un administrateur
private async Task UpdateAdminPasswordAsync(int adminId, string plainPassword)
{
    try
    {
        string hashedPassword = HashPassword(plainPassword);
        
        string updateQuery = "UPDATE Utilisateurs SET MotDePasse = @MotDePasse WHERE UtilisateurID = @UtilisateurID";
        var parameters = new Dictionary<string, object>
        {
            { "@UtilisateurID", adminId },
            { "@MotDePasse", hashedPassword }
        };
        
        await _dbService.ExecuteNonQueryAsync(updateQuery, parameters);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erreur lors de la mise à jour du mot de passe admin: {ex.Message}");
    }
}

        // Connexion d'un utilisateur
        // public async Task<bool> LoginAsync(string email, string password)
        // {
        //     try
        //     {
        //         string hashedPassword = HashPassword(password);
        //         
        //         string query = @"
        //             SELECT UtilisateurID, Nom, Prenom, Email, Telephone, TypeUtilisateur, DateInscription 
        //             FROM Utilisateurs 
        //             WHERE Email = @Email AND MotDePasse = @MotDePasse";
        //
        //         var parameters = new Dictionary<string, object>
        //         {
        //             { "@Email", email },
        //             { "@MotDePasse", hashedPassword }
        //         };
        //
        //         var users = await _dbService.ExecuteReaderAsync(query, reader => new Utilisateur
        //         {
        //             UtilisateurID = reader.GetInt32("UtilisateurID"),
        //             Nom = reader.GetString("Nom"),
        //             Prenom = reader.GetString("Prenom"),
        //             Email = reader.GetString("Email"),
        //             Telephone = reader.IsDBNull(reader.GetOrdinal("Telephone")) ? null : reader.GetString("Telephone"),
        //             TypeUtilisateur = reader.GetString("TypeUtilisateur"),
        //             DateInscription = reader.GetDateTime("DateInscription")
        //         }, parameters);
        //
        //         if (users.Count > 0)
        //         {
        //             _currentUser = users[0];
        //             AuthenticationChanged?.Invoke(this, EventArgs.Empty);
        //             return true;
        //         }
        //         
        //         return false;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Erreur lors de la connexion: {ex.Message}");
        //         return false;
        //     }
        // }

        // Déconnexion
        public void Logout()
        {
            _currentUser = null;
            AuthenticationChanged?.Invoke(this, EventArgs.Empty);
        }

        // Obtenir un utilisateur par son email
        public async Task<Utilisateur> GetUserByEmailAsync(string email)
        {
            try
            {
                string query = @"
                    SELECT UtilisateurID, Nom, Prenom, Email, Telephone, TypeUtilisateur, DateInscription 
                    FROM Utilisateurs 
                    WHERE Email = @Email";

                var parameters = new Dictionary<string, object>
                {
                    { "@Email", email }
                };

                var users = await _dbService.ExecuteReaderAsync(query, reader => new Utilisateur
                {
                    UtilisateurID = reader.GetInt32("UtilisateurID"),
                    Nom = reader.GetString("Nom"),
                    Prenom = reader.GetString("Prenom"),
                    Email = reader.GetString("Email"),
                    Telephone = reader.IsDBNull(reader.GetOrdinal("Telephone")) ? null : reader.GetString("Telephone"),
                    TypeUtilisateur = reader.GetString("TypeUtilisateur"),
                    DateInscription = reader.GetDateTime("DateInscription")
                }, parameters);

                return users.Count > 0 ? users[0] : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la recherche d'utilisateur: {ex.Message}");
                return null;
            }
        }

        // Mise à jour du profil utilisateur
        public async Task<bool> UpdateProfileAsync(Utilisateur utilisateur)
        {
            try
            {
                string query = @"
                    UPDATE Utilisateurs 
                    SET Nom = @Nom, 
                        Prenom = @Prenom, 
                        Telephone = @Telephone 
                    WHERE UtilisateurID = @UtilisateurID";

                var parameters = new Dictionary<string, object>
                {
                    { "@UtilisateurID", utilisateur.UtilisateurID },
                    { "@Nom", utilisateur.Nom },
                    { "@Prenom", utilisateur.Prenom },
                    { "@Telephone", utilisateur.Telephone }
                };

                int rowsAffected = await _dbService.ExecuteNonQueryAsync(query, parameters);
                
                if (rowsAffected > 0 && _currentUser?.UtilisateurID == utilisateur.UtilisateurID)
                {
                    // Mettre à jour l'utilisateur actuel si c'est lui qui est modifié
                    _currentUser.Nom = utilisateur.Nom;
                    _currentUser.Prenom = utilisateur.Prenom;
                    _currentUser.Telephone = utilisateur.Telephone;
                    AuthenticationChanged?.Invoke(this, EventArgs.Empty);
                }
                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la mise à jour du profil: {ex.Message}");
                return false;
            }
        }

        // Changement de mot de passe
        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                // Vérifier le mot de passe actuel
                string query = "SELECT MotDePasse FROM Utilisateurs WHERE UtilisateurID = @UtilisateurID";
                var parameters = new Dictionary<string, object>
                {
                    { "@UtilisateurID", userId }
                };

                string storedHash = await _dbService.ExecuteScalarAsync<string>(query, parameters);
                string currentHash = HashPassword(currentPassword);

                if (storedHash != currentHash)
                {
                    return false; // Le mot de passe actuel est incorrect
                }

                // Mettre à jour le mot de passe
                string updateQuery = "UPDATE Utilisateurs SET MotDePasse = @MotDePasse WHERE UtilisateurID = @UtilisateurID";
                var updateParams = new Dictionary<string, object>
                {
                    { "@UtilisateurID", userId },
                    { "@MotDePasse", HashPassword(newPassword) }
                };

                int rowsAffected = await _dbService.ExecuteNonQueryAsync(updateQuery, updateParams);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du changement de mot de passe: {ex.Message}");
                return false;
            }
        }

        // Obtenir tous les utilisateurs (pour l'administrateur)
        public async Task<List<Utilisateur>> GetAllUsersAsync()
        {
            try
            {
                string query = @"
                    SELECT UtilisateurID, Nom, Prenom, Email, Telephone, TypeUtilisateur, DateInscription 
                    FROM Utilisateurs";

                return await _dbService.ExecuteReaderAsync(query, reader => new Utilisateur
                {
                    UtilisateurID = reader.GetInt32("UtilisateurID"),
                    Nom = reader.GetString("Nom"),
                    Prenom = reader.GetString("Prenom"),
                    Email = reader.GetString("Email"),
                    Telephone = reader.IsDBNull(reader.GetOrdinal("Telephone")) ? null : reader.GetString("Telephone"),
                    TypeUtilisateur = reader.GetString("TypeUtilisateur"),
                    DateInscription = reader.GetDateTime("DateInscription")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des utilisateurs: {ex.Message}");
                return new List<Utilisateur>();
            }
        }
    }
}