using Microsoft.Maui.Controls;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SeaOfSteel.Pages
{
    public partial class RecherchePartiePage : ContentPage
    {
        private static readonly Guid SERVICE_UUID = Guid.Parse("0000180D-0000-1000-8000-00805F9B34FB");
        private static readonly Guid CHARACTERISTIC_UUID = Guid.Parse("00002A37-0000-1000-8000-00805F9B34FB");
        private readonly IAdapter _adapter = CrossBluetoothLE.Current.Adapter;
        private readonly IBluetoothLE _ble = CrossBluetoothLE.Current;
        private IDevice _selectedDevice;
        private ICharacteristic _communicationCharacteristic;
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
            if (confirmé && partie != null)
            {
                try
                {
                    var device = _adapter.DiscoveredDevices.FirstOrDefault(d => d.Id.ToString() == partie.Adresse);
                    if (device == null)
                    {
                        await DisplayAlert("Erreur", "Périphérique introuvable", "OK");
                        return;
                    }

                    _selectedDevice = device;
                    await _adapter.ConnectToDeviceAsync(device);

                    var services = await device.GetServicesAsync();
                    var service = services.FirstOrDefault(s => s.Id == SERVICE_UUID);
                    if (service == null)
                    {
                        await DisplayAlert("Erreur", "Service Bluetooth non trouvé", "OK");
                        return;
                    }

                    _communicationCharacteristic = await service.GetCharacteristicAsync(CHARACTERISTIC_UUID);
                    await _communicationCharacteristic.StartUpdatesAsync();
                    _communicationCharacteristic.ValueUpdated += OnMessageReceived;

                    // Exemple : envoyer un tir
                    var tir = new { type = "tir", x = 4, y = 2 };
                    var json = JsonSerializer.Serialize(tir);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                    await _communicationCharacteristic.WriteAsync(bytes);

                    await Navigation.PushAsync(new JeuPage(false, false, _communicationCharacteristic)); // client, multi
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erreur connexion", ex.Message, "OK");
                }
            }

        }
        private void OnMessageReceived(object sender, CharacteristicUpdatedEventArgs e)
        {
            var message = System.Text.Encoding.UTF8.GetString(e.Characteristic.Value);
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(message);

            if (data?["type"]?.ToString() == "resultat")
            {
                bool touche = Convert.ToBoolean(data["touche"]);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Résultat", touche ? "Touché !" : "Manqué", "OK");
                });
            }
        }


        private async void OnAnnulerClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
