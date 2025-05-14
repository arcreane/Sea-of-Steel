using Microsoft.Maui.Controls;

namespace SeaOfSteel.Pages;

public class ResultatsPage : ContentPage
{
    public ResultatsPage(string message)
    {
        Title = "Résultats";

        var label = new Label
        {
            Text = message,
            FontSize = 24,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.CenterAndExpand
        };

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
            Children =
            {
                label,
                rejouerButton,
                accueilButton
            }
        };
    }
}
