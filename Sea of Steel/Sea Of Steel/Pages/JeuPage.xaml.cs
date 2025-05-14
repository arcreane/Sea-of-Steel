using SeaOfSteel.Models;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeaOfSteel.Pages;

public partial class JeuPage : ContentPage
{
    private const int GridSize = 10;
    private readonly Button[,] _grillePlacement = new Button[GridSize, GridSize];
    private readonly Button[,] _grilleTir = new Button[GridSize, GridSize];

    private readonly Grid _grillePlacementUI = new();
    private readonly Grid _grilleTirUI = new();
    private readonly Label _tourLabel;

    private readonly bool _isHost;
    private readonly bool _modeSolo;
    private bool _phasePlacement = true;
    private bool _estMonTour = true;

    private readonly List<Bateau> _bateauxJoueur = new();
    private readonly List<Bateau> _bateauxAdverses = new();

    private readonly List<(int Row, int Col)> _positionsTemp = new();
    private readonly List<(int Row, int Col)> _casesDejaTirees = new();

    private int _bateauEnCoursIndex = 0;
    private readonly List<Bateau> _listeBateauxAJouer = new()
    {
        new Bateau { Nom = "Porte-avion", Taille = 5 },
        new Bateau { Nom = "Croiseur", Taille = 4 },
        new Bateau { Nom = "Contre-torpilleur", Taille = 3 },
        new Bateau { Nom = "Sous-marin", Taille = 3 },
        new Bateau { Nom = "Torpilleur", Taille = 2 }
    };

    private enum Direction { None, Horizontal, Vertical }
    private Direction _currentDirection = Direction.None;

    public JeuPage(bool isHost, bool modeSolo = false)
    {
        InitializeComponent();
        _isHost = isHost;
        _modeSolo = modeSolo;

        _tourLabel = new Label
        {
            Text = "À vous de jouer",
            FontSize = 18,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10)
        };

        CreateGrid(_grillePlacement, _grillePlacementUI);
        CreateGrid(_grilleTir, _grilleTirUI);

        Content = new VerticalStackLayout
        {
            Children =
            {
                _tourLabel,
                new Label { Text = "Ma flotte", FontSize = 20, HorizontalOptions = LayoutOptions.Center },
                _grillePlacementUI
            }
        };

        if (_modeSolo)
        {
            GénérerBateauxAdverses();
        }

        DisplayAlert("Placement", $"Placez votre {_listeBateauxAJouer[_bateauEnCoursIndex].Nom} ({_listeBateauxAJouer[_bateauEnCoursIndex].Taille} cases)", "OK");
    }

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
                    Margin = new Thickness(1),
                    CornerRadius = 5,
                    BindingContext = new Position(row, col)
                };

                button.Clicked += OnGridButtonClicked;
                grille[row, col] = button;
                grilleUI.Add(button, col, row);
            }
        }
    }

    private async void OnGridButtonClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not Position position)
            return;

        if (_phasePlacement)
        {
            if (_bateauEnCoursIndex >= _listeBateauxAJouer.Count || _positionsTemp.Contains((position.Row, position.Col)) || _bateauxJoueur.Any(b => b.Positions.Contains((position.Row, position.Col))))
                return;

            if (_positionsTemp.Count == 1)
            {
                var first = _positionsTemp[0];
                if (!IsAdjacent(position.Row, position.Col))
                    return;

                if (first.Row == position.Row)
                    _currentDirection = Direction.Horizontal;
                else if (first.Col == position.Col)
                    _currentDirection = Direction.Vertical;
                else
                    return;
            }
            else if (_positionsTemp.Count > 1)
            {
                if (!IsInLineWithDirection(position.Row, position.Col) || !IsAdjacent(position.Row, position.Col))
                    return;
            }

            button.BackgroundColor = Colors.Gray;
            _positionsTemp.Add((position.Row, position.Col));

            var bateauActuel = _listeBateauxAJouer[_bateauEnCoursIndex];

            if (_positionsTemp.Count == bateauActuel.Taille)
            {
                bateauActuel.Positions = new List<(int Row, int Col)>(_positionsTemp);
                _bateauxJoueur.Add(bateauActuel);
                _positionsTemp.Clear();
                _currentDirection = Direction.None;
                _bateauEnCoursIndex++;

                if (_bateauEnCoursIndex < _listeBateauxAJouer.Count)
                {
                    await DisplayAlert("Placement", $"Placez votre {_listeBateauxAJouer[_bateauEnCoursIndex].Nom} ({_listeBateauxAJouer[_bateauEnCoursIndex].Taille} cases)", "OK");
                }
                else
                {
                    await DisplayAlert("Placement terminé", "Tous les bateaux sont placés !", "Commencer le jeu");
                    _phasePlacement = false;
                    AfficherGrillesDeJeu();
                    DémarrerTourParTour();
                }
            }
        }
        else
        {
            TirerSurCase(button, position);
        }
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

    private bool IsAdjacent(int row, int col)
    {
        return _positionsTemp.Any(p => Math.Abs(p.Row - row) + Math.Abs(p.Col - col) == 1);
    }

    private void TirerSurCase(Button caseCible, Position position)
    {
        if (!_estMonTour || _casesDejaTirees.Contains((position.Row, position.Col)))
            return;

        _casesDejaTirees.Add((position.Row, position.Col));
        bool touche = false;

        foreach (var bateau in _bateauxAdverses)
        {
            if (bateau.Positions.Remove((position.Row, position.Col)))
            {
                caseCible.BackgroundColor = Colors.Red;
                touche = true;
                if (bateau.EstCoule)
                    DisplayAlert("Bateau coulé", $"Vous avez coulé le {bateau.Nom} !", "OK");
                break;
            }

        if (!touche)
            caseCible.BackgroundColor = Colors.LightGray;
        }
        if (_bateauxAdverses.All(b => b.Positions.Count == 0))
        {
            Dispatcher.Dispatch(async () =>
            {
                await DisplayAlert("Victoire", "Tous les bateaux ennemis ont été coulés !", "OK");
                await Navigation.PushAsync(new ResultatsPage("Victoire !"));
            });
            return;
        }

        _estMonTour = false;
        _tourLabel.Text = "Tour de l'adversaire";

        if (_modeSolo)
        {
            Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(1), SimulerTourAdverse);
        }
        else
        {
            // Future implémentation Bluetooth/Wifi P2P ici
        }
    }

    private void SimulerTourAdverse()
    {
        var rand = new Random();
        (int Row, int Col) tir;

        do
        {
            tir = (rand.Next(0, GridSize), rand.Next(0, GridSize));
        } while (_casesDejaTirees.Contains(tir));

        _casesDejaTirees.Add(tir);
        bool touche = false;

        foreach (var bateau in _bateauxJoueur)
        {
            if (bateau.Positions.Remove(tir))
            {
                _grillePlacement[tir.Row, tir.Col].BackgroundColor = Colors.Orange;
                touche = true;

                if (bateau.EstCoule)
                    DisplayAlert("Adversaire", $"L'adversaire a coulé votre {bateau.Nom} !", "OK");
                break;
            }
        }

        

        if (!touche)
            _grillePlacement[tir.Row, tir.Col].BackgroundColor = Colors.LightSlateGray;
        if (_bateauxJoueur.All(b => b.Positions.Count == 0))
        {
            Dispatcher.Dispatch(async () =>
            {
                await DisplayAlert("Défaite", "Tous vos bateaux ont été coulés !", "OK");
                await Navigation.PushAsync(new ResultatsPage("Défaite..."));
            });
            return;
        }
        _estMonTour = true;
        _tourLabel.Text = "À vous de jouer";
    }

    private void DémarrerTourParTour()
    {
        _estMonTour = true;
        _tourLabel.Text = "À vous de jouer";
    }

    private void AfficherGrillesDeJeu()
    {
        var placementContainer = new VerticalStackLayout
        {
            Children =
            {
                new Label { Text = "Ma flotte", FontSize = 20, HorizontalOptions = LayoutOptions.Center },
                _grillePlacementUI
            }
        };

        var tirContainer = new VerticalStackLayout
        {
            Children =
            {
                new Label { Text = "Grille de tir", FontSize = 20, HorizontalOptions = LayoutOptions.Center },
                _grilleTirUI
            }
        };

        var gameLayout = new HorizontalStackLayout
        {
            HorizontalOptions = LayoutOptions.Center,
            Spacing = 20,
            Padding = 10,
            Children = { placementContainer, tirContainer }
        };

        Content = new VerticalStackLayout
        {
            Children =
            {
                _tourLabel,
                gameLayout
            }
        };
    }

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

                if (positions.Count == modele.Taille && !_bateauxAdverses.Any(b => b.Positions.Intersect(positions).Any()))
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

    private record Position(int Row, int Col);
}
