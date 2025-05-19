using Microsoft.Maui.Controls;
using System;

namespace SeaOfSteel.Pages
{
    public partial class NomHotePage : ContentPage
    {
        public NomHotePage()
        {
            InitializeComponent();
        }

        private async void OnValiderClicked(object sender, EventArgs e)
        {
            string nom = NomEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(nom))
            {
                await DisplayAlert("Erreur", "Veuillez entrer un nom.", "OK");
                return;
            }

            AttenteLabel.IsVisible = true;
            Loading.IsVisible = true;
            Loading.IsRunning = true;

            // Simule une attente de connexion d’un autre joueur
            await Task.Delay(3000); // À remplacer plus tard par la vraie détection Bluetooth

            // Quand un joueur est connecté :
            await Navigation.PushAsync(new JeuPage(true, false));
        }
    }
}
