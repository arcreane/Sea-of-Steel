namespace SeaOfSteel
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("AccueilPage", typeof(Pages.AccueilPage));
            Routing.RegisterRoute("WikiPage", typeof(Pages.WikiPage));
            Routing.RegisterRoute("LobbyPage", typeof(Pages.LobbyPage));
            Routing.RegisterRoute("JeuPage", typeof(Pages.JeuPage));
            Routing.RegisterRoute("ResultatsPage", typeof(Pages.ResultatsPage));
        }
    }
}