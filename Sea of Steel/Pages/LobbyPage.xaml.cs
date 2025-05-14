using SeaOfSteel.Models;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeaOfSteel.Pages;
public partial class LobbyPage : ContentPage
{
    public LobbyPage()
    {
        InitializeComponent();
    }

    private async void JouerContreBot_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new JeuPage(false, true)); // Client, solo
    }

    private async void CreerPartie_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new JeuPage(true, false)); // Hôte, multijoueur
    }

    private async void RejoindrePartie_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new JeuPage(false, false)); // Client, multijoueur
    }
}
