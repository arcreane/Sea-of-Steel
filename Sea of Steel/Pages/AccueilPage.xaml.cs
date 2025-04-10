using System;
using Microsoft.Maui.Controls;

namespace SeaOfSteel.Pages
{
    public partial class AccueilPage : ContentPage
    {
        public AccueilPage()
        {
            InitializeComponent();
        }

        private async void OnIntroTapped(object sender, EventArgs e)
        {
            // Animation de réduction
            await LogoImage.ScaleTo(0.5, 400, Easing.CubicIn);
            await IntroTitle.ScaleTo(0.5, 400, Easing.CubicIn);

            // Fade out de l’intro
            await IntroLayer.FadeTo(0, 400, Easing.CubicOut);
            IntroLayer.IsVisible = false;

            // Affiche le contenu principal
            MainContent.IsVisible = true;
            await MainContent.FadeTo(1, 500, Easing.CubicIn);
        }

        private async void OnJoinClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LobbyPage(joinMode: true));
        }

        private async void OnCreateClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LobbyPage(joinMode: false));
        }

        private void OnQuitClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
        }
    }
}
