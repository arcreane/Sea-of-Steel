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
