using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;

namespace SeaOfSteel.Pages
{
    public partial class RecherchePartiePage : ContentPage
    {
        public class PartieDispo
        {
            public string Nom { get; set; }
            public string Adresse { get; set; } // Pour Bluetooth plus tard
        }

        private List<PartieDispo> _partiesDetectees = new();

        public RecherchePartiePage()
        {
            InitializeComponent();
            DémarrerRecherche();
        }

        private async void DémarrerRecherche()
        {
            // Simulation — à remplacer avec détection Bluetooth réelle
            await Task.Delay(2000);

            _partiesDetectees = new List<PartieDispo>
            {
                new() { Nom = "Partie de Jules", Adresse = "AA:BB:CC:DD:01" },
                new() { Nom = "Naval Battle", Adresse = "AA:BB:CC:DD:02" }
            };

            ListeParties.ItemsSource = _partiesDetectees;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            ListeParties.IsVisible = true;
        }

        private async void ListeParties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0)
                return;

            var partie = e.CurrentSelection[0] as PartieDispo;
            if (partie == null)
                return;

            bool confirmé = await DisplayAlert("Rejoindre la partie", $"Rejoindre \"{partie.Nom}\" ?", "Oui", "Non");
            if (confirmé)
            {
                // TODO : implémenter la connexion réelle
                await Navigation.PushAsync(new JeuPage(false, false)); // client, multi
            }
        }

        private async void OnAnnulerClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
