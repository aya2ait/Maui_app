using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySqlConnector;
using restaurant.Models;

namespace restaurant.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Méthode générique pour exécuter des commandes SQL (INSERT, UPDATE, DELETE)
        public async Task<int> ExecuteNonQueryAsync(string commandText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (MySqlCommand command = new MySqlCommand(commandText, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }
                        return await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de base de données: {ex.Message}");
                throw;
            }
        }
        // Méthode corrigée pour UpdateCommandeStatutAsync
        public async Task<int> UpdateCommandeStatutAsync(int commandeId, string statut)
        {
            string query = "UPDATE Commandes SET Statut = @Statut WHERE CommandeID = @CommandeID";
    
            var parameters = new Dictionary<string, object>
            {
                { "@CommandeID", commandeId },
                { "@Statut", statut }
            };
    
            return await ExecuteNonQueryAsync(query, parameters);
        }
        // Ajoutez ces méthodes à votre classe DatabaseService existante

        public async Task<int> AddPaiementAsync(Paiement paiement)
        {
            string query = @"
        INSERT INTO Paiements (CommandeID, Montant, MethodePaiement, DateHeure, Reference)
        VALUES (@CommandeID, @Montant, @MethodePaiement, @DateHeure, @Reference);
        SELECT LAST_INSERT_ID();
    ";
    
            var parameters = new Dictionary<string, object>
            {
                { "@CommandeID", paiement.CommandeID },
                { "@Montant", paiement.Montant },
                { "@MethodePaiement", paiement.MethodePaiement },
                { "@DateHeure", paiement.DateHeure },
                { "@Reference", paiement.Reference }
            };
    
            return await ExecuteScalarAsync<int>(query, parameters);
        }

        public async Task<Paiement> GetPaiementByCommandeIdAsync(int commandeId)
        {
            string query = @"
        SELECT PaiementID, CommandeID, Montant, MethodePaiement, DateHeure, Reference 
        FROM Paiements 
        WHERE CommandeID = @CommandeID;
    ";
    
            var parameters = new Dictionary<string, object>
            {
                { "@CommandeID", commandeId }
            };
    
            var paiements = await ExecuteReaderAsync(query, reader => new Paiement
            {
                PaiementID = reader.GetInt32("PaiementID"),
                CommandeID = reader.GetInt32("CommandeID"),
                Montant = reader.GetDecimal("Montant"),
                MethodePaiement = reader.GetString("MethodePaiement"),
                DateHeure = reader.GetDateTime("DateHeure"),
                Reference = reader.IsDBNull(reader.GetOrdinal("Reference")) ? null : reader.GetString("Reference")
            }, parameters);
    
            return paiements.FirstOrDefault();
        }
        public async Task<MySqlConnection> GetConnectionAsync()
        {
            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }


        // Méthode générique pour les requêtes retournant une seule valeur
        public async Task<T> ExecuteScalarAsync<T>(string commandText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (MySqlCommand command = new MySqlCommand(commandText, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }
                        var result = await command.ExecuteScalarAsync();
                        return (T)Convert.ChangeType(result, typeof(T));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de base de données: {ex.Message}");
                throw;
            }
        }

        // Méthode générique pour les requêtes retournant un DataReader
        public async Task<List<T>> ExecuteReaderAsync<T>(string commandText, Func<MySqlDataReader, T> mapper, Dictionary<string, object> parameters = null)
        {
            var results = new List<T>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (MySqlCommand command = new MySqlCommand(commandText, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                results.Add(mapper(reader));
                            }
                        }
                    }
                }
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de base de données: {ex.Message}");
                throw;
            }
        }

        // Exemple d'utilisation pour obtenir toutes les catégories
        public async Task<List<Categorie>> GetAllCategoriesAsync()
        {
            string query = "SELECT CategorieID, Nom, Description, ImageUrl FROM Categories";

            return await ExecuteReaderAsync(query, reader => new Categorie
            {
                CategorieID = reader.GetInt32("CategorieID"),
                Nom = reader.GetString("Nom"),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? null : reader.GetString("ImageUrl")
            });
        }

        // Exemple d'utilisation pour obtenir tous les plats d'une catégorie
        public async Task<List<Plat>> GetPlatsByCategorieAsync(int categorieId)
        {
            string query = "SELECT PlatID, CategorieID, Nom, Description, Prix, ImageUrl, EstDisponible FROM Plats WHERE CategorieID = @CategorieID";

            var parameters = new Dictionary<string, object>
            {
                { "@CategorieID", categorieId }
            };

            return await ExecuteReaderAsync(query, reader => new Plat
            {
                PlatID = reader.GetInt32("PlatID"),
                CategorieID = reader.GetInt32("CategorieID"),
                Nom = reader.GetString("Nom"),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                Prix = reader.GetDecimal("Prix"),
                ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? null : reader.GetString("ImageUrl"),
                EstDisponible = reader.GetBoolean("EstDisponible")
            }, parameters);
        }

        // Ajouter une ligne de commande
        public async Task<int> AddLigneCommandeAsync(LigneCommande ligneCommande)
        {
            string query = @"
                INSERT INTO LignesCommande (CommandeID, PlatID, Quantite, PrixUnitaire, Notes)
                VALUES (@CommandeID, @PlatID, @Quantite, @PrixUnitaire, @Notes);
                SELECT LAST_INSERT_ID();
            ";

            var parameters = new Dictionary<string, object>
            {
                { "@CommandeID", ligneCommande.CommandeID },
                { "@PlatID", ligneCommande.PlatID },
                { "@Quantite", ligneCommande.Quantite },
                { "@PrixUnitaire", ligneCommande.PrixUnitaire },
                { "@Notes", ligneCommande.Notes }
            };

            return await ExecuteScalarAsync<int>(query, parameters);
        }

        public async Task<int> AddCommandeAsync(Commande commande)
        {
            string query = @"
                INSERT INTO Commandes (UtilisateurID, TableID, DateHeure, Statut, Total, Notes)
                VALUES (@UtilisateurID, @TableID, @DateHeure, @Statut, @Total, @Notes);
                SELECT LAST_INSERT_ID();
            ";
            
            var parameters = new Dictionary<string, object>
            {
                { "@UtilisateurID", commande.UtilisateurID },
                { "@TableID", commande.TableID },
                { "@DateHeure", commande.DateHeure },
                { "@Statut", commande.Statut },
                { "@Total", commande.Total },
                { "@Notes", commande.Notes }
            };
            
            return await ExecuteScalarAsync<int>(query, parameters);
        }
    }
}
