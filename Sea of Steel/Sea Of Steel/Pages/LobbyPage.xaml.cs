using SeaOfSteel.Pages;
using Microsoft.Maui.Controls;
using System;

namespace SeaOfSteel.Pages
{
    public partial class LobbyPage : ContentPage
    {
        public LobbyPage()
        {
            InitializeComponent();
        }

        private async void JouerContreBot_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new JeuPage(false, true)); // Mode solo
        }

        private async void CreerPartie_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new JeuPage(true, false)); // Hôte multijoueur
        }

        private async void OnRetourClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
