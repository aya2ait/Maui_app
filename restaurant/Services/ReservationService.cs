using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using restaurant.Models;

namespace restaurant.Services
{
    public class ReservationService
    {
        private readonly DatabaseService _dbService;
        private readonly AuthService _authService;

        public ReservationService(DatabaseService dbService, AuthService authService)
        {
            _dbService = dbService;
            _authService = authService;
        }

        // Récupérer toutes les tables disponibles
        public async Task<List<Table>> GetAvailableTablesAsync(DateTime dateHeure, int nombrePersonnes)
        {
            try
            {
                string query = @"
                    SELECT t.TableID, t.Numero, t.Capacite, t.Statut
                    FROM Tables t
                    WHERE t.Capacite >= @NombrePersonnes
                    AND t.TableID NOT IN (
                        SELECT r.TableID
                        FROM Reservations r
                        WHERE r.DateHeure BETWEEN @DateDebutPlage AND @DateFinPlage
                        AND r.Statut = 'Confirmée'
                    )
                    ORDER BY t.Capacite ASC";

                // Plage de 2 heures pour une réservation
                DateTime dateDebutPlage = dateHeure.AddHours(-1);
                DateTime dateFinPlage = dateHeure.AddHours(1);

                var parameters = new Dictionary<string, object>
                {
                    { "@NombrePersonnes", nombrePersonnes },
                    { "@DateDebutPlage", dateDebutPlage },
                    { "@DateFinPlage", dateFinPlage }
                };

                return await _dbService.ExecuteReaderAsync(query, reader => new Table
                {
                    TableID = reader.GetInt32("TableID"),
                    Numero = reader.GetInt32("Numero"),
                    Capacite = reader.GetInt32("Capacite"),
                    Statut = reader.GetString("Statut")
                }, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des tables disponibles: {ex.Message}");
                return new List<Table>();
            }
        }

        // Créer une nouvelle réservation
        public async Task<int> CreateReservationAsync(Reservation reservation)
        {
            try
            {
                string query = @"
                    INSERT INTO Reservations (UtilisateurID, TableID, DateHeure, NombrePersonnes, Statut, Notes)
                    VALUES (@UtilisateurID, @TableID, @DateHeure, @NombrePersonnes, @Statut, @Notes);
                    SELECT LAST_INSERT_ID();";

                var parameters = new Dictionary<string, object>
                {
                    { "@UtilisateurID", reservation.UtilisateurID },
                    { "@TableID", reservation.TableID },
                    { "@DateHeure", reservation.DateHeure },
                    { "@NombrePersonnes", reservation.NombrePersonnes },
                    { "@Statut", reservation.Statut ?? "Confirmée" },
                    { "@Notes", reservation.Notes }
                };

                return await _dbService.ExecuteScalarAsync<int>(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la création de la réservation: {ex.Message}");
                return 0;
            }
        }

        // Récupérer les réservations de l'utilisateur connecté
        public async Task<List<ReservationDetail>> GetUserReservationsAsync()
        {
            try
            {
                if (!_authService.IsAuthenticated)
                    return new List<ReservationDetail>();

                string query = @"
                    SELECT r.ReservationID, r.UtilisateurID, r.TableID, r.DateHeure, 
                           r.NombrePersonnes, r.Statut, r.Notes,
                           t.Numero, t.Capacite
                    FROM Reservations r
                    JOIN Tables t ON r.TableID = t.TableID
                    WHERE r.UtilisateurID = @UtilisateurID
                    ORDER BY r.DateHeure DESC";

                var parameters = new Dictionary<string, object>
                {
                    { "@UtilisateurID", _authService.CurrentUser.UtilisateurID }
                };

                return await _dbService.ExecuteReaderAsync(query, reader => new ReservationDetail
                {
                    ReservationID = reader.GetInt32("ReservationID"),
                    UtilisateurID = reader.GetInt32("UtilisateurID"),
                    TableID = reader.GetInt32("TableID"),
                    DateHeure = reader.GetDateTime("DateHeure"),
                    NombrePersonnes = reader.GetInt32("NombrePersonnes"),
                    Statut = reader.GetString("Statut"),
                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString("Notes"),
                    NumeroTable = reader.GetInt32("Numero"),
                    CapaciteTable = reader.GetInt32("Capacite")
                }, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des réservations: {ex.Message}");
                return new List<ReservationDetail>();
            }
        }

        // Annuler une réservation
        public async Task<bool> CancelReservationAsync(int reservationId)
        {
            try
            {
                string query = @"
                    UPDATE Reservations
                    SET Statut = 'Annulée'
                    WHERE ReservationID = @ReservationID
                    AND UtilisateurID = @UtilisateurID
                    AND DateHeure > @DateHeure";

                var parameters = new Dictionary<string, object>
                {
                    { "@ReservationID", reservationId },
                    { "@UtilisateurID", _authService.CurrentUser.UtilisateurID },
                    { "@DateHeure", DateTime.Now }  // Empêcher l'annulation des réservations passées
                };

                int rowsAffected = await _dbService.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'annulation de la réservation: {ex.Message}");
                return false;
            }
        }
    }
}