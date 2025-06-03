using SeaOfSteel.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeaOfSteel.Pages;

public partial class JeuPage : ContentPage
{
    private const int GridSize = 10;

    // ——— UI ———
    private readonly Button[,] _grillePlacement = new Button[GridSize, GridSize];
    private readonly Button[,] _grilleTir = new Button[GridSize, GridSize];

    // ——— État de jeu ———
    private readonly bool _isHost;
    private readonly bool _modeSolo;
    private bool _phasePlacement = true;
    private bool _estMonTour = true;

    private readonly List<Bateau> _bateauxJoueur = new();
    private readonly List<Bateau> _bateauxAdverses = new();

    private readonly List<(int Row, int Col)> _positionsTemp = new();
    private readonly List<(int Row, int Col)> _casesJoueurTirees = new();
    private readonly List<(int Row, int Col)> _casesAdversaireTirees = new();

    private int _bateauEnCoursIndex = 0;
    private readonly List<Bateau> _listeBateauxAJouer = new()
    {
        new Bateau { Nom = "Porte-avion",       Taille = 5 },
        new Bateau { Nom = "Croiseur",          Taille = 4 },
        new Bateau { Nom = "Contre-torpilleur", Taille = 3 },
        new Bateau { Nom = "Sous-marin",        Taille = 3 },
        new Bateau { Nom = "Torpilleur",        Taille = 2 }
    };

    private enum Direction { None, Horizontal, Vertical }
    private Direction _currentDirection = Direction.None;

    public JeuPage(bool isHost, bool modeSolo = false)
    {
        InitializeComponent();
        _isHost = isHost;
        _modeSolo = modeSolo;

        // Construction des deux grilles
        CreateGrid(_grillePlacement, PlacementGrid);
        CreateGrid(_grilleTir, TirGrid);

        if (_modeSolo)
            GénérerBateauxAdverses();

        // Premier message de placement
        _ = DisplayAlert("Placement",
                         $"Placez votre {_listeBateauxAJouer[_bateauEnCoursIndex].Nom} " +
                         $"({_listeBateauxAJouer[_bateauEnCoursIndex].Taille} cases)",
                         "OK");
    }

    // -------------------------------------------------
    //  Construction de grille générique
    // -------------------------------------------------
    private void CreateGrid(Button[,] grille, Grid grilleUI)
    {
        grilleUI.RowDefinitions.Clear();
        grilleUI.ColumnDefinitions.Clear();

        for (int i = 0; i < GridSize; i++)
        {
            grilleUI.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grilleUI.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        }

        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                var button = new Button
                {
                    BackgroundColor = Colors.LightBlue,
                    WidthRequest = 30,
                    HeightRequest = 30,
                    Margin = 1,
                    CornerRadius = 5,
                    BindingContext = new Position(row, col)
                };
                button.Clicked += OnGridButtonClicked;

                grille[row, col] = button;
                grilleUI.Add(button, col, row);
            }
        }
    }

    // -------------------------------------------------
    //  Gestion des clics (placement / tir)
    // -------------------------------------------------
    private async void OnGridButtonClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not Position pos)
            return;

        if (_phasePlacement)
        {
            if (!TraiterPlacement(button, pos))
                return;

            var bateauActuel = _listeBateauxAJouer[_bateauEnCoursIndex];

            if (_positionsTemp.Count == bateauActuel.Taille)
            {
                bateauActuel.Positions = new List<(int, int)>(_positionsTemp);
                _bateauxJoueur.Add(bateauActuel);

                _positionsTemp.Clear();
                _currentDirection = Direction.None;
                _bateauEnCoursIndex++;

                if (_bateauEnCoursIndex < _listeBateauxAJouer.Count)
                {
                    await DisplayAlert("Placement",
                                       $"Placez votre {_listeBateauxAJouer[_bateauEnCoursIndex].Nom} " +
                                       $"({_listeBateauxAJouer[_bateauEnCoursIndex].Taille} cases)",
                                       "OK");
                }
                else
                {
                    await DisplayAlert("Placement terminé",
                                       "Tous les bateaux sont placés !",
                                       "Commencer le jeu");

                    // Passage à la phase de jeu
                    _phasePlacement = false;
                    TirGrid.IsVisible = true;          // Affiche la grille de tir
                    PlacementGrid.IsEnabled = false;     // Empêche de « tirer » sur soi
                    DémarrerTourParTour();
                }
            }
        }
        else
        {
            // On ne doit cliquer que dans la grille de tir
            if (!ReferenceEquals(button.Parent, TirGrid))
                return;

            TirerSurCase(button, pos);
        }
    }

    private bool TraiterPlacement(Button button, Position position)
    {
        if (_bateauEnCoursIndex >= _listeBateauxAJouer.Count ||
            _positionsTemp.Contains((position.Row, position.Col)) ||
            _bateauxJoueur.Any(b => b.Positions.Contains((position.Row, position.Col))))
            return false;

        if (_positionsTemp.Count == 1)
        {
            var first = _positionsTemp[0];
            if (!IsAdjacent(position.Row, position.Col)) return false;

            _currentDirection = first.Row == position.Row ? Direction.Horizontal :
                                 first.Col == position.Col ? Direction.Vertical : Direction.None;
            if (_currentDirection == Direction.None) return false;
        }
        else if (_positionsTemp.Count > 1)
        {
            if (!IsInLineWithDirection(position.Row, position.Col) || !IsAdjacent(position.Row, position.Col))
                return false;
        }

        button.BackgroundColor = Colors.Gray;
        _positionsTemp.Add((position.Row, position.Col));
        return true;
    }

    private bool IsInLineWithDirection(int row, int col)
    {
        var reference = _positionsTemp[0];
        return _currentDirection switch
        {
            Direction.Horizontal => row == reference.Row,
            Direction.Vertical => col == reference.Col,
            _ => false
        };
    }

    private bool IsAdjacent(int row, int col) =>
        _positionsTemp.Any(p => Math.Abs(p.Row - row) + Math.Abs(p.Col - col) == 1);

    private void TirerSurCase(Button caseCible, Position position)
    {
        if (!_estMonTour || _casesJoueurTirees.Contains((position.Row, position.Col)))
            return;

        _casesJoueurTirees.Add((position.Row, position.Col));
        bool touche = false;

        foreach (var bateau in _bateauxAdverses)
        {
            if (!bateau.Positions.Remove((position.Row, position.Col))) continue;

            caseCible.BackgroundColor = Colors.Red;
            touche = true;
            VibrerImpact();

            if (bateau.EstCoule)
            {
                VibrerExplosion();
                DisplayAlert("Bateau coulé", $"Vous avez coulé le {bateau.Nom} !", "OK");
            }
            break;
        }

        if (!touche)
            caseCible.BackgroundColor = Colors.LightGray;

        if (_bateauxAdverses.All(b => b.EstCoule))
        {
            Dispatcher.Dispatch(async () =>
            {
                await DisplayAlert("Victoire", "Tous les bateaux ennemis ont été coulés !", "OK");
                await Navigation.PushAsync(new ResultatsPage("Victoire !", _casesJoueurTirees.Count));
            });
            return;
        }

        _estMonTour = false;
        TourLabel.Text = "Tour de l'adversaire";

        if (_modeSolo)
            Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(1), SimulerTourAdverse);
    }

    private void SimulerTourAdverse()
    {
        var rand = new Random();
        (int Row, int Col) tir;

        do
        {
            tir = (rand.Next(0, GridSize), rand.Next(0, GridSize));
        } while (_casesAdversaireTirees.Contains(tir));

        _casesAdversaireTirees.Add(tir);
        bool touche = false;

        foreach (var bateau in _bateauxJoueur)
        {
            if (bateau.Positions.Remove(tir))
            {
                _grillePlacement[tir.Row, tir.Col].BackgroundColor = Colors.Orange;
                touche = true;
                VibrerImpact();

                if (bateau.EstCoule)
                {
                    VibrerExplosion();
                    DisplayAlert("Adversaire", $"L'adversaire a coulé votre {bateau.Nom} !", "OK");
                }
                break;
            }
        }

        if (!touche)
            _grillePlacement[tir.Row, tir.Col].BackgroundColor = Colors.LightSlateGray;

        if (_bateauxJoueur.All(b => b.EstCoule))
        {
            Dispatcher.Dispatch(async () =>
            {
                await DisplayAlert("Défaite", "Tous vos bateaux ont été coulés !", "OK");
                await Navigation.PushAsync(new ResultatsPage("Défaite...", _casesJoueurTirees.Count));
            });
            return;
        }

        _estMonTour = true;
        TourLabel.Text = "À vous de jouer";
    }

    private void AfficherGrillesDeJeu()
    {
        var placement = new VerticalStackLayout
        {
            Children = {
                new Label { Text = "Ma flotte", FontSize = 20, HorizontalOptions = LayoutOptions.Center },
                PlacementGrid
            }
        };

        var tir = new VerticalStackLayout
        {
            Children = {
                new Label { Text = "Grille de tir", FontSize = 20, HorizontalOptions = LayoutOptions.Center },
                TirGrid
            }
        };

        Content = new VerticalStackLayout
        {
            Children = {
                TourLabel,
                new HorizontalStackLayout
                {
                    Spacing = 20,
                    Padding = 10,
                    HorizontalOptions = LayoutOptions.Center,
                    Children = { placement, tir }
                }
            }
        };
    }


    private void DémarrerTourParTour()
    {
        _estMonTour = true;
        TourLabel.Text = "À vous de jouer";
    }

    private void VibrerImpact()
    {
        try { Vibration.Default.Vibrate(100); } catch { }
    }

    private void VibrerExplosion()
    {
        try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300)); } catch { }
    }

    private record Position(int Row, int Col);

    private void GénérerBateauxAdverses()
    {
        var rand = new Random();
        _bateauxAdverses.Clear();

        foreach (var modele in _listeBateauxAJouer)
        {
            bool placé = false;

            while (!placé)
            {
                int row = rand.Next(0, GridSize);
                int col = rand.Next(0, GridSize);
                bool horizontal = rand.Next(2) == 0;

                var positions = new List<(int, int)>();

                for (int i = 0; i < modele.Taille; i++)
                {
                    int r = horizontal ? row : row + i;
                    int c = horizontal ? col + i : col;

                    if (r >= GridSize || c >= GridSize)
                        break;

                    positions.Add((r, c));
                }

                if (positions.Count == modele.Taille &&
                    !_bateauxAdverses.Any(b => b.Positions.Intersect(positions).Any()))
                {
                    _bateauxAdverses.Add(new Bateau
                    {
                        Nom = modele.Nom,
                        Taille = modele.Taille,
                        Positions = positions
                    });
                    placé = true;
                }
            }
        }
    }
}
