using System.Net.Http;
using System.Text.Json;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeaOfSteel.Pages;

public partial class WikiPage : ContentPage
{
    private readonly Dictionary<string, string> paysCategories = new()
    {
        {"États-Unis", "Liste_des_navires_de_l'US_Navy"},
        {"France", "Liste_des_navires_de_la_Marine_nationale_française"},
        {"Royaume-Uni", "Liste_des_navires_de_la_Royal_Navy"},
        {"Japon", "Liste_des_navires_de_la_Marine_impériale_japonaise"}
    };

    public WikiPage()
    {
        InitializeComponent();
        PaysPicker.ItemsSource = paysCategories.Keys.ToList();
    }

    private async void OnPaysChanged(object sender, EventArgs e)
    {
        if (PaysPicker.SelectedIndex == -1) return;

        string selectedKey = PaysPicker.SelectedItem.ToString();
        if (selectedKey != null && paysCategories.TryGetValue(selectedKey, out string pageTitre))
        {
            await ChargerNaviresDepuisWikipedia(pageTitre);
        }
    }

    private async Task ChargerNaviresDepuisWikipedia(string pageTitre)
    {
        try
        {
            using var client = new HttpClient();
            string url = $"https://fr.wikipedia.org/w/api.php?action=parse&page={Uri.EscapeDataString(pageTitre)}&format=json&prop=sections";

            var response = await client.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);

            var sections = doc.RootElement.GetProperty("parse").GetProperty("sections");

            var navires = new List<WikiNavire>();
            foreach (var section in sections.EnumerateArray())
            {
                if (section.TryGetProperty("line", out var line))
                {
                    string titre = line.GetString();
                    if (!string.IsNullOrEmpty(titre))
                        navires.Add(new WikiNavire { Title = titre });
                }
            }

            NaviresListView.ItemsSource = navires;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de charger les navires : {ex.Message}", "OK");
        }
    }

    public class WikiNavire
    {
        public string Title { get; set; }
    }
}
