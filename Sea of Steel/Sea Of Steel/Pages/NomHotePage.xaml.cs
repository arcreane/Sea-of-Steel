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

            // Simule une annonce - remplacer par Bluetooth server plus tard
            var annonce = $"SeaOfSteel-{nom}";
            Console.WriteLine($"Annonce simulée : {annonce}");

            // Simulation d'un client qui se connecte après 3 sec
            await Task.Delay(3000);
            await Navigation.PushAsync(new JeuPage(true, false));
        }

    }
}
