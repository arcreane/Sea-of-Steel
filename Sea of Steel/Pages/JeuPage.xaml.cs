using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace SeaOfSteel.Pages
{
    public partial class JeuPage : ContentPage
    {
        public JeuPage(ObservableCollection<Joueur> joueurs)
        {
            InitializeComponent();

            // Pour l'instant on affiche juste les noms des joueurs dans la console
            foreach (var joueur in joueurs)
            {
                Console.WriteLine($"Joueur : {joueur.Nom}");
            }
        }
    }
}
