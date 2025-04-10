using SeaOfSteel.Pages;

namespace Sea_of_Steel
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new AccueilPage());
        }
    }
}
