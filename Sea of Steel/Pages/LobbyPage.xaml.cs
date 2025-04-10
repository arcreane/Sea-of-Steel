using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using SeaOfSteel.Pages;


namespace SeaOfSteel.Pages
{
    public partial class LobbyPage : ContentPage
    {
        public ObservableCollection<Joueur> Joueurs { get; set; } = new();
        public string LobbyTitle { get; set; }
        public string BoutonActionTexte => _isHote ? "Lancer la partie" : "En attente de l'hôte...";
        public bool PeutLancer => _isHote && Joueurs.Count > 1;
        public bool IsRechercheEnCours { get; set; }

        private readonly bool _isHote;

        public LobbyPage(bool joinMode)
        {
            InitializeComponent();
            _isHote = !joinMode;
            LobbyTitle = _isHote ? "Lobby - Hôte" : "Lobby - Client";

            // Pour test : on ajoute le joueur local
            Joueurs.Add(new Joueur { Nom = "Moi (Toi)" });

            BindingContext = this;

            if (!_isHote)
                StartRechercheHote(); // Simulation ou détection
            else
                DémarrerServeur();
        }

        private void DémarrerServeur()
        {
            // Simule des connexions de joueurs (à remplacer par vrai Bluetooth/WiFi P2P)
            Joueurs.Add(new Joueur { Nom = "Joueur 2" });
        }

        private void StartRechercheHote()
        {
            IsRechercheEnCours = true;
            // Code pour trouver et rejoindre un hôte ici
        }

        private async void OnActionClicked(object sender, EventArgs e)
        {
            if (_isHote && PeutLancer)
            {
                await Navigation.PushAsync(new JeuPage(Joueurs));
            }
        }

        private async void OnRetourClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }

    public class Joueur
    {
        public string Nom { get; set; }
    }
}
