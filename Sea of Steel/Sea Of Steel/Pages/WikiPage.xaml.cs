using System.Net.Http;
using System.Text.Json;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeaOfSteel.Pages;

public partial class WikiPage : ContentPage
{
    private readonly Dictionary<string, string> nations = new()
    {
        {"États-Unis", "usa"},
        {"Japon", "japan"},
        {"Allemagne", "germany"},
        {"URSS", "ussr"},
        {"France", "france"},
        {"Royaume-Uni", "uk"}
    };

    private const string ApplicationId = "DEMO"; // <-- À remplacer par la vraie clé API
    private const string BaseUrl = "https://api.worldofwarships.eu/wows/encyclopedia/ships/";

    public WikiPage()
    {
        InitializeComponent();
        PaysPicker.ItemsSource = nations.Keys.ToList();
    }

    private async void OnPaysChanged(object sender, EventArgs e)
    {
        if (PaysPicker.SelectedIndex == -1) return;

        string selectedKey = PaysPicker.SelectedItem.ToString();
        if (nations.TryGetValue(selectedKey, out string nationCode))
        {
            await ChargerNaviresDepuisWargaming(nationCode);
        }
    }

    private async Task ChargerNaviresDepuisWargaming(string nationCode)
    {
        try
        {
            using var client = new HttpClient();
            string url = $"{BaseUrl}?application_id={ApplicationId}&nation={nationCode}";

            var response = await client.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);

            var navires = new List<NavireWows>();
            foreach (var item in doc.RootElement.GetProperty("data").EnumerateObject())
            {
                var ship = item.Value;
                if (ship.TryGetProperty("name", out var nameProp))
                {
                    navires.Add(new NavireWows
                    {
                        Nom = nameProp.GetString()
                    });
                }
            }

            NaviresListView.ItemsSource = navires;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Erreur lors du chargement : {ex.Message}", "OK");
        }
    }

    public class NavireWows
    {
        public string Nom { get; set; }
    }
}
