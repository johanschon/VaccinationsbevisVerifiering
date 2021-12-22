using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using VaccinbevisVerifiering.Views;
using VaccinbevisVerifiering.Services.CWT.Certificates;
using VaccinbevisVerifiering.Services;
using Xamarin.Essentials;

namespace VaccinbevisVerifiering
{
    public partial class App : Application
    {
        public static CertificateManager CertificateManager { get; private set; }

        public App()
        {
            InitializeComponent();
            CertificateManager = new CertificateManager(new RestService());
            MainPage = new NavigationPage(new MainPage ()){ BarBackgroundColor = Color.White };
            Xamarin.Essentials.Preferences.Set("NoVerificationMode", false);
            Xamarin.Essentials.Preferences.Set("ProductionMode", true);
        }

        protected override async void OnStart()
        {
            CertificateManager.LoadCertificates();
            CertificateManager.LoadValueSets();
            await CertificateManager.LoadVaccineRules();

            var deviceVersion = AppInfo.Version;
            var latestVersion = Device.RuntimePlatform switch
            {
                Device.Android => CertificateManager.VaccinRules.AppVersion.Android,
                Device.iOS => CertificateManager.VaccinRules.AppVersion.iOS,
                _ => null
            };

            if (latestVersion == null) return;
            var latestVersionArray = latestVersion.Split('.');
            var latestMajorVersion = int.Parse(latestVersionArray[0]);
            var latestMinorVersion = int.Parse(latestVersionArray[1]);

            if (latestMajorVersion > deviceVersion.Major || latestMinorVersion > deviceVersion.Minor)
                await Current.MainPage.Navigation.PushAsync(new UpdatePage());
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
            CertificateManager.LoadCertificates();
            CertificateManager.LoadValueSets();
            CertificateManager.LoadVaccineRules();
        }
    }
}
