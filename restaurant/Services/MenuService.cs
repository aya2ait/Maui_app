using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using restaurant.Models;

namespace restaurant.Services
{
    public class MenuService
    {
        private readonly DatabaseService _databaseService;

        public MenuService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // Services pour les catégories
        public async Task<List<Categorie>> GetAllCategoriesAsync()
        {
            return await _databaseService.GetAllCategoriesAsync();
        }

        public async Task<Categorie> GetCategorieByIdAsync(int categorieId)
        {
            string query = "SELECT CategorieID, Nom, Description, ImageUrl FROM Categories WHERE CategorieID = @CategorieID";
            
            var parameters = new Dictionary<string, object>
            {
                { "@CategorieID", categorieId }
            };

            var categories = await _databaseService.ExecuteReaderAsync(query, reader => new Categorie
            {
                CategorieID = reader.GetInt32("CategorieID"),
                Nom = reader.GetString("Nom"),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? null : reader.GetString("ImageUrl")
            }, parameters);

            return categories.Count > 0 ? categories[0] : null;
        }

        public async Task<int> AddCategorieAsync(Categorie categorie)
        {
            string query = @"
                INSERT INTO Categories (Nom, Description, ImageUrl)
                VALUES (@Nom, @Description, @ImageUrl);
                SELECT LAST_INSERT_ID();
            ";
            
            var parameters = new Dictionary<string, object>
            {
                { "@Nom", categorie.Nom },
                { "@Description", categorie.Description },
                { "@ImageUrl", categorie.ImageUrl }
            };
            
            return await _databaseService.ExecuteScalarAsync<int>(query, parameters);
        }

        public async Task<int> UpdateCategorieAsync(Categorie categorie)
        {
            string query = @"
                UPDATE Categories 
                SET Nom = @Nom, Description = @Description, ImageUrl = @ImageUrl
                WHERE CategorieID = @CategorieID;
            ";
            
            var parameters = new Dictionary<string, object>
            {
                { "@CategorieID", categorie.CategorieID },
                { "@Nom", categorie.Nom },
                { "@Description", categorie.Description },
                { "@ImageUrl", categorie.ImageUrl }
            };
            
            return await _databaseService.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<int> DeleteCategorieAsync(int categorieId)
        {
            // Vérifier d'abord s'il y a des plats liés à cette catégorie
            string checkQuery = "SELECT COUNT(*) FROM Plats WHERE CategorieID = @CategorieID";
            
            var checkParams = new Dictionary<string, object>
            {
                { "@CategorieID", categorieId }
            };
            
            int count = await _databaseService.ExecuteScalarAsync<int>(checkQuery, checkParams);
            
            if (count > 0)
            {
                throw new InvalidOperationException("Impossible de supprimer la catégorie car elle contient des plats.");
            }
            
            string query = "DELETE FROM Categories WHERE CategorieID = @CategorieID";
            
            var parameters = new Dictionary<string, object>
            {
                { "@CategorieID", categorieId }
            };
            
            return await _databaseService.ExecuteNonQueryAsync(query, parameters);
        }

        // Services pour les plats
        public async Task<List<Plat>> GetAllPlatsAsync()
        {
            string query = "SELECT PlatID, CategorieID, Nom, Description, Prix, ImageUrl, EstDisponible FROM Plats";

            return await _databaseService.ExecuteReaderAsync(query, reader => new Plat
            {
                PlatID = reader.GetInt32("PlatID"),
                CategorieID = reader.GetInt32("CategorieID"),
                Nom = reader.GetString("Nom"),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                Prix = reader.GetDecimal("Prix"),
                ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? null : reader.GetString("ImageUrl"),
                EstDisponible = reader.GetBoolean("EstDisponible")
            });
        }

        public async Task<Plat> GetPlatByIdAsync(int platId)
        {
            string query = "SELECT PlatID, CategorieID, Nom, Description, Prix, ImageUrl, EstDisponible FROM Plats WHERE PlatID = @PlatID";
            
            var parameters = new Dictionary<string, object>
            {
                { "@PlatID", platId }
            };

            var plats = await _databaseService.ExecuteReaderAsync(query, reader => new Plat
            {
                PlatID = reader.GetInt32("PlatID"),
                CategorieID = reader.GetInt32("CategorieID"),
                Nom = reader.GetString("Nom"),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                Prix = reader.GetDecimal("Prix"),
                ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? null : reader.GetString("ImageUrl"),
                EstDisponible = reader.GetBoolean("EstDisponible")
            }, parameters);

            return plats.Count > 0 ? plats[0] : null;
        }

        public async Task<List<Plat>> GetPlatsByCategorieAsync(int categorieId)
        {
            return await _databaseService.GetPlatsByCategorieAsync(categorieId);
        }

        public async Task<int> AddPlatAsync(Plat plat)
        {
            string query = @"
                INSERT INTO Plats (CategorieID, Nom, Description, Prix, ImageUrl, EstDisponible)
                VALUES (@CategorieID, @Nom, @Description, @Prix, @ImageUrl, @EstDisponible);
                SELECT LAST_INSERT_ID();
            ";
            
            var parameters = new Dictionary<string, object>
            {
                { "@CategorieID", plat.CategorieID },
                { "@Nom", plat.Nom },
                { "@Description", plat.Description },
                { "@Prix", plat.Prix },
                { "@ImageUrl", plat.ImageUrl },
                { "@EstDisponible", plat.EstDisponible }
            };
            
            return await _databaseService.ExecuteScalarAsync<int>(query, parameters);
        }

        public async Task<int> UpdatePlatAsync(Plat plat)
        {
            string query = @"
                UPDATE Plats 
                SET CategorieID = @CategorieID, Nom = @Nom, Description = @Description, 
                    Prix = @Prix, ImageUrl = @ImageUrl, EstDisponible = @EstDisponible
                WHERE PlatID = @PlatID;
            ";
            
            var parameters = new Dictionary<string, object>
            {
                { "@PlatID", plat.PlatID },
                { "@CategorieID", plat.CategorieID },
                { "@Nom", plat.Nom },
                { "@Description", plat.Description },
                { "@Prix", plat.Prix },
                { "@ImageUrl", plat.ImageUrl },
                { "@EstDisponible", plat.EstDisponible }
            };
            
            return await _databaseService.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<int> UpdatePlatDisponibiliteAsync(int platId, bool estDisponible)
        {
            string query = "UPDATE Plats SET EstDisponible = @EstDisponible WHERE PlatID = @PlatID";
            
            var parameters = new Dictionary<string, object>
            {
                { "@PlatID", platId },
                { "@EstDisponible", estDisponible }
            };
            
            return await _databaseService.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<int> DeletePlatAsync(int platId)
        {
            // Vérifier si le plat est utilisé dans des commandes
            string checkQuery = "SELECT COUNT(*) FROM LignesCommande WHERE PlatID = @PlatID";
            
            var checkParams = new Dictionary<string, object>
            {
                { "@PlatID", platId }
            };
            
            int count = await _databaseService.ExecuteScalarAsync<int>(checkQuery, checkParams);
            
            if (count > 0)
            {
                throw new InvalidOperationException("Impossible de supprimer le plat car il est utilisé dans des commandes.");
            }
            
            string query = "DELETE FROM Plats WHERE PlatID = @PlatID";
            
            var parameters = new Dictionary<string, object>
            {
                { "@PlatID", platId }
            };
            
            return await _databaseService.ExecuteNonQueryAsync(query, parameters);
        }
    }
}