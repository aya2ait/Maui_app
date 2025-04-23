using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using restaurant.Models;

namespace restaurant.Services
{
    public class PanierService
    {
        public ObservableCollection<LigneCommande> Items { get; } = new ObservableCollection<LigneCommande>();
        
        public event EventHandler PanierChanged;
        
        public decimal Total => Items.Sum(i => i.PrixUnitaire * i.Quantite);
        
        public int ItemCount => Items.Sum(i => i.Quantite);
        
        public void AjouterPlat(Plat plat, int quantite = 1, string notes = null)
        {
            var existingItem = Items.FirstOrDefault(i => i.PlatID == plat.PlatID);
            
            if (existingItem != null)
            {
                existingItem.Quantite += quantite;
                // Déclencher l'événement pour notifier les abonnés
                PanierChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                var newItem = new LigneCommande
                {
                    PlatID = plat.PlatID,
                    Plat = plat,
                    Quantite = quantite,
                    PrixUnitaire = plat.Prix,
                    Notes = notes
                };
                
                Items.Add(newItem);
                // Déclencher l'événement pour notifier les abonnés
                PanierChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        public void SupprimerPlat(int platId)
        {
            var item = Items.FirstOrDefault(i => i.PlatID == platId);
            if (item != null)
            {
                Items.Remove(item);
                // Déclencher l'événement pour notifier les abonnés
                PanierChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        public void ModifierQuantite(int platId, int quantite)
        {
            var item = Items.FirstOrDefault(i => i.PlatID == platId);
            if (item != null)
            {
                if (quantite <= 0)
                {
                    Items.Remove(item);
                }
                else
                {
                    item.Quantite = quantite;
                }
                // Déclencher l'événement pour notifier les abonnés
                PanierChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        public void ViderPanier()
        {
            Items.Clear();
            // Déclencher l'événement pour notifier les abonnés
            PanierChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}