using Microsoft.Maui.Controls;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.iOS;
using System;
using System.Collections.Generic;

namespace SeaOfSteel.Pages
{
    public partial class RecherchePartiePage : ContentPage
    {
        private readonly IAdapter _adapter = CrossBluetoothLE.Current.Adapter;
        private readonly IBluetoothLE _ble = CrossBluetoothLE.Current;
        public class PartieDispo
        {
            public string Nom { get; set; }
            public string Adresse { get; set; } // Pour Bluetooth plus tard
        }

        private List<PartieDispo> _partiesDetectees = new();

        public RecherchePartiePage()
        {
            InitializeComponent();
            DémarrerRecherche();
        }

        private async void DémarrerRecherche()
        {
            if (!_ble.IsAvailable || !_ble.IsOn)
            {
                await DisplayAlert("Bluetooth", "Bluetooth désactivé ou non disponible.", "OK");
                return;
            }

            _adapter.DeviceDiscovered += (s, a) =>
            {
                var nom = a.Device.Name;

                if (!string.IsNullOrWhiteSpace(nom) && nom.StartsWith("SeaOfSteel-"))
                {
                    _partiesDetectees.Add(new PartieDispo
                    {
                        Nom = nom.Replace("SeaOfSteel-", ""),
                        Adresse = a.Device.Id.ToString()
                    });

                    ListeParties.ItemsSource = null;
                    ListeParties.ItemsSource = _partiesDetectees;
                }
            };

            try
            {
                await _adapter.StartScanningForDevicesAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Erreur scan : {ex.Message}", "OK");
            }

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            ListeParties.IsVisible = true;
        }

        private async void ListeParties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0)
                return;

            var partie = e.CurrentSelection[0] as PartieDispo;
            if (partie == null)
                return;

            bool confirmé = await DisplayAlert("Rejoindre la partie", $"Rejoindre \"{partie.Nom}\" ?", "Oui", "Non");
            if (confirmé)
            {
                // TODO : implémenter la connexion réelle
                await Navigation.PushAsync(new JeuPage(false, false)); // client, multi
            }
        }

        private async void OnAnnulerClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
