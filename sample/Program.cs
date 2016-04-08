using Autoinjestion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var task = Task.Run(() => MainAsync(args));

            task.Wait();
        }

        static async void MainAsync(string[] args)
        {
            var appName = "YOURAPPNAMEHERE";
            var itunesConnectAccount = "YourItunesConnectAccount";
            var password = "YourPassword";
            var vendorId = "YourVendorNumber";

            // Keeping my credentials out of github....
            var map = new ExeConfigurationFileMap { ExeConfigFilename = "settings.user" };
            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            if (config.HasFile)
            {
                var section = (config.GetSection("appSettings") as AppSettingsSection);
                if (section != null)
                {
                    password = section.Settings["password"].Value;
                    appName = section.Settings["appName"].Value;
                    itunesConnectAccount = section.Settings["itunesConnectAccount"].Value;
                    vendorId = section.Settings["vendorId"].Value;
                }
            }

            // Get current year
            var yyyy = DateTime.Now.Year;

            var ai = new AutoInjestion(itunesConnectAccount, password, vendorId);

            for (int year = 2015; year <= yyyy; year++)
            {
                var da1 = await ai.GetAppDataByWeek(appName, AppleiTunesReportDateType.Yearly, new DateTime(year, 1, 1));

                if (da1 != null)
                {
                    var d2 = da1.Where(x => x.SKU.Equals(appName))              // Match on the app
                        .Where(d => ai.IsNewDownload(d.ProductTypeIdentifier))  // Match on new installs
                        .Sum(ms => int.Parse(ms.Units));                        // Sum up the downloads

                    Console.WriteLine("{0}: {1}", year, d2);
                }
            }
        }
    }
}
