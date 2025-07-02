using Microsoft.Maui.Controls;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Text.Json;

namespace SeaOfSteel.Pages
{
    public partial class NomHotePage : ContentPage
    {
        private static readonly Guid SERVICE_UUID = Guid.Parse("0000180D-0000-1000-8000-00805F9B34FB");
        private static readonly Guid CHARACTERISTIC_UUID = Guid.Parse("00002A37-0000-1000-8000-00805F9B34FB");
        private readonly IAdapter _adapter = CrossBluetoothLE.Current.Adapter;
        private readonly IBluetoothLE _ble = CrossBluetoothLE.Current;

        private IDevice _clientDevice;
        private ICharacteristic _hostCharacteristic;
        private bool _estConnexionEnCours = false;

        public NomHotePage()
        {
            InitializeComponent();
        }

        private async void OnValiderClicked(object sender, EventArgs e)
        {
            string nom = NomEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(nom))
            {
                await DisplayAlert("Erreur", "Veuillez entrer un nom.", "OK");
                return;
            }

            AttenteLabel.IsVisible = true;
            Loading.IsVisible = true;
            Loading.IsRunning = true;

            var annonce = $"SeaOfSteel-{nom}";
            Console.WriteLine($"Annonce en cours : {annonce}");

            try
            {
                _adapter.DeviceDiscovered += async (s, a) =>
                {
                    if (_estConnexionEnCours) return;

                    if (a.Device.Name?.StartsWith("SeaOfSteel-") == true)
                    {
                        _estConnexionEnCours = true;

                        try
                        {
                            await _adapter.ConnectToDeviceAsync(a.Device);
                            _clientDevice = a.Device;

                            var services = await _clientDevice.GetServicesAsync();
                            var service = services.FirstOrDefault(s => s.Id == SERVICE_UUID);

                            if (service == null)
                            {
                                await DisplayAlert("Erreur", "Service non trouvé", "OK");
                                _estConnexionEnCours = false;
                                return;
                            }

                            _hostCharacteristic = await service.GetCharacteristicAsync(CHARACTERISTIC_UUID);
                            await _hostCharacteristic.StartUpdatesAsync();
                            _hostCharacteristic.ValueUpdated += OnClientMessage;

                            await Navigation.PushAsync(new JeuPage(true, false, _hostCharacteristic));
                        }
                        catch (TaskCanceledException)
                        {
                            await DisplayAlert("Timeout", "Connexion annulée : temps dépassé.", "OK");
                            _estConnexionEnCours = false;
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlert("Erreur", ex.Message, "OK");
                            _estConnexionEnCours = false;
                        }
                    }
                };


                await _adapter.StartScanningForDevicesAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur Bluetooth", ex.Message, "OK");
            }
        }
        private async void OnClientMessage(object sender, CharacteristicUpdatedEventArgs e)
        {
            var msg = System.Text.Encoding.UTF8.GetString(e.Characteristic.Value);
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(msg);

            if (data?["type"]?.ToString() == "tir")
            {
                int x = int.Parse(data["x"].ToString());
                int y = int.Parse(data["y"].ToString());

                Console.WriteLine($"Tir reçu : ({x}, {y})");

                // Simule une logique aléatoire de réponse
                bool touche = new Random().Next(2) == 0;

                var response = new Dictionary<string, object>
                {
                    ["type"] = "resultat",
                    ["touche"] = touche
                };
                var json = JsonSerializer.Serialize(response);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                await _hostCharacteristic.WriteAsync(bytes);
            }
        }


    }
}
