using Plugin.Maui.Audio;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Dispatching;
using SeaOfSteel.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeaOfSteel.Pages
{
    public partial class AccueilPage : ContentPage
    {
        private readonly IAudioManager _audioManager;
        private IAudioPlayer? _player;
        private bool easterEggTriggered = false;

        public AccueilPage()
        {
            InitializeComponent();
            _audioManager = AudioManager.Current;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!Accelerometer.IsMonitoring)
            {
                Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
                Accelerometer.Start(SensorSpeed.UI);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (Accelerometer.IsMonitoring)
            {
                Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
                Accelerometer.Stop();
            }
        }

        private void Accelerometer_ReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;

            if (!easterEggTriggered &&
                (Math.Abs(data.Acceleration.X) > 2 ||
                 Math.Abs(data.Acceleration.Y) > 2 ||
                 Math.Abs(data.Acceleration.Z) > 2))
            {
                easterEggTriggered = true;
                MainThread.BeginInvokeOnMainThread(TriggerEasterEgg);
            }
        }

        private async void TriggerEasterEgg()
        {
            try
            {
                BackgroundImageView.Source = "sousmarin.png";
                await LogoImage.RotateTo(180, 500, Easing.CubicInOut);

                Bubble1.TranslationY = 0;
                Bubble1.IsVisible = true;
                await Bubble1.TranslateTo(0, -600, 3000, Easing.CubicOut);
                Bubble1.IsVisible = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur dans l'easter egg : {ex.Message}");
            }
        }

        private async void OnIntroTapped(object sender, EventArgs e)
        {
            try
            {
                var stream = await FileSystem.OpenAppPackageFileAsync("Skeleton_Ambush.wav");
                _player = _audioManager.CreatePlayer(stream);
                _player.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur audio : {ex.Message}");
            }

            await LogoImage.ScaleTo(0.5, 400, Easing.CubicIn);
            await IntroTitle.ScaleTo(0.5, 400, Easing.CubicIn);

            await IntroLayer.FadeTo(0, 400, Easing.CubicOut);
            IntroLayer.IsVisible = false;

            MainContent.IsVisible = true;
            await MainContent.FadeTo(1, 500, Easing.CubicIn);
        }

        private async void OnJoinClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LobbyPage()); // Ajoute un paramètre si besoin
        }

        private async void OnCreateClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LobbyPage()); // Ajoute un paramètre si besoin
        }

        private void OnQuitClicked(object sender, EventArgs e)
        {
#if ANDROID
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#elif WINDOWS
            Application.Current.Quit();
#else
            System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
#endif
        }
    }
}
