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
            var iTunesConnectAccount = "YourItunesConnectAccount";
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
                    iTunesConnectAccount = section.Settings["itunesConnectAccount"].Value;
                    vendorId = section.Settings["vendorId"].Value;
                }
            }

            var ai = new AutoInjestion(iTunesConnectAccount, password, vendorId);

            Console.WriteLine("Annual totals since 2014");
            // Get current year
            var yyyy = DateTime.Now.Year;

            for (int year = 2014; year < yyyy; year++)
            {
                var da1 = await ai.GetAppData(appName, AppleiTunesReportDateType.Yearly, new DateTime(year, 1, 1));

                if (da1 != null)
                {
                    var d2 = da1.Where(x => x.SKU.Equals(appName))              // Match on the app
                        .Where(d => ai.IsNewDownload(d.ProductTypeIdentifier))  // Match on new installs
                        .Sum(ms => int.Parse(ms.Units));                        // Sum up the downloads

                    Console.WriteLine("{0}: {1}", year, d2);
                }
            }

            Console.WriteLine("Monthly totals for this year");
            var mm = DateTime.Now.Month;

            for (int i = 1; i < mm; i++)
            {
                var da1 = await ai.GetAppDataByMonth(appName, yyyy, i);

                if (da1 != null)
                {
                    var d2 = da1.Where(x => x.SKU.Equals("appName"))
                        .Where(d => ai.IsNewDownload(d.ProductTypeIdentifier))
                        .Sum(ms => int.Parse(ms.Units));

                    Console.WriteLine("{0}-{1}: {2}", yyyy, i, d2);
                }
            }

        }
    }
}
