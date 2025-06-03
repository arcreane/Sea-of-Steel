using Microsoft.Maui.Controls;

namespace SeaOfSteel.Pages;

public class ResultatsPage : ContentPage
{
    public ResultatsPage(string message, int nombreTirs)
    {
        Title = "Résultats";

        // Label principal pour victoire/défaite
        var resultatLabel = new Label
        {
            Text = message,
            FontSize = 24,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        // Label pour le nombre de tirs
        var tirsLabel = new Label
        {
            Text = $"Nombre de tirs : {nombreTirs}",
            FontSize = 18,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10, 0, 20)
        };

        // Bouton Rejouer
        var rejouerButton = new Button
        {
            Text = "Rejouer",
            FontSize = 18,
            Margin = new Thickness(0, 10)
        };
        rejouerButton.Clicked += async (s, e) =>
        {
            await Navigation.PushAsync(new LobbyPage());
        };

        // Bouton Accueil
        var accueilButton = new Button
        {
            Text = "Accueil",
            FontSize = 18,
            Margin = new Thickness(0, 10)
        };
        accueilButton.Clicked += async (s, e) =>
        {
            await Navigation.PopToRootAsync();
        };

        Content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 15,
            Children =
            {
                resultatLabel,
                tirsLabel,
                rejouerButton,
                accueilButton
            }
        };
    }
}
