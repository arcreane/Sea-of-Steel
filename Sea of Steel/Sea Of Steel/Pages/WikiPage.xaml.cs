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
    { "États-Unis", "United_States" },
    { "Royaume-Uni", "United_Kingdom" },
    { "France", "France" },
    { "Japon", "Japan" },
    { "Allemagne", "Germany" },
    { "URSS", "Soviet_Union" }
};

    public WikiPage()
    {
        InitializeComponent();
        PaysPicker.ItemsSource = nations.Keys.ToList();
    }

    public class NavireWiki
    {
        public string Titre { get; set; }
        public string ImageUrl { get; set; }
        public string DescriptionCourte { get; set; }
        public string PageId { get; set; }
        public string Annee { get; set; }
    }
    private async Task ChargerNaviresDepuisWikipedia(string pays)
    {
        try
        {
            using var client = new HttpClient();
            string url = $"https://{(pays == "Japon" ? "ja" : "en")}.wikipedia.org/w/api.php" +
                         "?action=query&format=json&origin=*&prop=pageimages|extracts" +
                         "&exintro=true&explaintext=true&generator=categorymembers" +
                         $"&gcmtitle=Category:Naval_ships_of_{pays.Replace(" ", "_")}" +
                         "&gcmlimit=20&piprop=thumbnail&pithumbsize=200";

            var response = await client.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);

            var pages = doc.RootElement.GetProperty("query").GetProperty("pages");
            var navires = new List<NavireWiki>();

            foreach (var page in pages.EnumerateObject())
            {
                var props = page.Value;
                string titre = props.GetProperty("title").GetString() ?? "Sans nom";
                string pageId = props.GetProperty("pageid").ToString();
                string extrait = props.TryGetProperty("extract", out var ext) ? ext.GetString() ?? "" : "";
                string thumb = props.TryGetProperty("thumbnail", out var tn) && tn.TryGetProperty("source", out var src) ? src.GetString() ?? "" : "";

                string annee = System.Text.RegularExpressions.Regex.Match(extrait, @"\b(18|19|20)\d{2}\b").Value;

                navires.Add(new NavireWiki
                {
                    Titre = titre,
                    PageId = pageId,
                    DescriptionCourte = extrait.Length > 100 ? extrait.Substring(0, 100) + "..." : extrait,
                    ImageUrl = thumb,
                    Annee = annee
                });
            }

            NaviresListView.ItemsSource = navires;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Erreur Wikipedia : {ex.Message}", "OK");
        }
    }
    private async void OnPaysChanged(object sender, EventArgs e)
    {
        if (PaysPicker.SelectedIndex == -1) return;

        string selectedKey = PaysPicker.SelectedItem.ToString();
        if (nations.TryGetValue(selectedKey, out string countryCode))
        {
            await ChargerNaviresDepuisWikipedia(countryCode);
        }
    }

    private async void NaviresListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is NavireWiki navire)
        {
            await Navigation.PushAsync(new NavireDetailPage(navire));
        }
    }
    public class NavireDetailPage : ContentPage
    {
        public NavireDetailPage(NavireWiki navire)
        {
            Title = navire.Titre;

            var image = new Image { Source = navire.ImageUrl, HeightRequest = 200 };
            var label = new Label { Text = navire.DescriptionCourte, FontSize = 16, Margin = new Thickness(10) };
            var annee = new Label { Text = $"Année : {navire.Annee}", FontSize = 14, Margin = new Thickness(10, 0) };

            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Children = { image, annee, label }
                }
            };
        }
    }

}
