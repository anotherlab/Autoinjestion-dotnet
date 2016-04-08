using Autoinjestion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() => MainAsync(args));
        }

        static async void MainAsync(string[] args)
        {
            var appName = "YOURAPPNAMEHERE";

            // Get current year
            var yyyy = DateTime.Now.Year;

            var ai = new AutoInjestion("YourItuneConnectAccount", "YourPassword", "YourVendorNumber");

            for (int year = 2015; year < yyyy; year++)
            {
                var da1 = await ai.GetAppDataByWeek(appName, AppleiTunesReportDateType.Yearly, new DateTime(year, 1, 1));

                if (da1 != null)
                {
                    var d2 = da1.Where(x => x.SKU.Equals(appName)).Where(d => d.ProductTypeIdentifier.Contains("1")).Sum(ms => int.Parse(ms.Units));

                    Console.WriteLine("{0}: {1}", year, d2);
                }
            }
        }
    }
}
