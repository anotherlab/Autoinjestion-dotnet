using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Autoinjestion
{
    public class AutoInjestion
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string AccountNumber { get; set; }

        public AutoInjestion(string userName, string password, string accountNumber)
        {
            UserName = userName;
            Password = password;
            AccountNumber = accountNumber;
        }

        public async Task<List<AppleiTunesSalesReport>> GetAppDataByWeek(string appName, AppleiTunesReportDateType appleiTunesReportDateType)
        {
            var date = DateTime.Now;
            var sunday = date.AddDays(-(int)date.DayOfWeek - 0);

            return await GetAppDataByWeek(appName, appleiTunesReportDateType, sunday);
        }

        public async Task<List<AppleiTunesSalesReport>> GetAppDataByWeek(string appName, AppleiTunesReportDateType appleiTunesReportDateType, DateTime date)
        {
            const string url = "https://reportingitc.apple.com/autoingestion.tft";

            var httpClient = new HttpClient();

            var response = await httpClient.PostAsync(url, new StringContent(GetWeeklyReportUrlParams(appleiTunesReportDateType, date), Encoding.UTF8, "application/x-www-form-urlencoded"));

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                responseStream.Position = 0;

                using (var outStream = new System.IO.MemoryStream())
                {
                    using (System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(responseStream, System.IO.Compression.CompressionMode.Decompress))
                    {
                        int bufferSize = 8192, bytesRead = 0;
                        byte[] buffer = new byte[bufferSize];
                        while ((bytesRead = gz.Read(buffer, 0, bufferSize)) > 0)
                        {
                            outStream.Write(buffer, 0, bytesRead);
                        }
                    }
                    outStream.Position = 0;

                    var MyStop = GetAppDataFromStream(() => outStream, appName);

                    return MyStop;
                }
            }
        }

        private string GetSafeIndexValue(string[] apps, int index)
        {
            if (apps.Length > index)
                return apps[index];

            return null;
        }

        public List<AppleiTunesSalesReport> GetAppDataFromStream(Func<Stream> streamProvider, string appName)
        {
            using (var stream = streamProvider())
            {
                var lines = ReadLines(() => stream, Encoding.UTF8).ToList();

                try
                {
                    var sales = from line in lines
                                let apps = line.Split('\t')
                                select new AppleiTunesSalesReport
                                {
                                    Provider = apps[0],
                                    ProviderCountry = apps[1],
                                    SKU = apps[2],
                                    Developer = apps[3],
                                    Title = apps[4],
                                    Version = apps[5],
                                    ProductTypeIdentifier = apps[6],
                                    Units = apps[7],
                                    DeveloperProceeds = apps[8],
                                    BeginDate = apps[9],
                                    EndDate = apps[10],
                                    CustomerCurrency = apps[11],
                                    CountryCode = apps[12],
                                    CurrencyofProceeds = apps[13],
                                    AppleIdentifier = apps[14],
                                    CustomerPrice = apps[15],
                                    PromoCode = apps[16],
                                    ParentIdentifier = apps[17],
                                    Subscription = apps[18],
                                    Period = apps[19],
                                    Category = apps[20],
                                    CMB = apps[21],
                                    Device = GetSafeIndexValue(apps, 22),
                                    SupportedPlatforms = GetSafeIndexValue(apps, 23)
                                };

                    var returnValue = sales.Where(x => x.SKU.Equals(appName, StringComparison.CurrentCultureIgnoreCase) || appName.Equals("")).ToList();

                    return returnValue;
                }
                catch
                {
                    throw;
                }
            }
        }

        private IEnumerable<string> ReadLines(Func<Stream> streamProvider,
                                             Encoding encoding)
        {
            using (var stream = streamProvider())
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        private string GetWeeklyReportUrlParams(AppleiTunesReportDateType appleiTunesReportDateType, DateTime date)
        {
            var postData = new PostData
            {
                Username = UserName,
                Password = Password,
                VndNumber = AccountNumber,
                TypeOfReport = "Sales",
                DateType = appleiTunesReportDateType.ToString(),
                ReportType = "Summary"
            };

            switch (appleiTunesReportDateType)
            {
                case AppleiTunesReportDateType.Daily:
                case AppleiTunesReportDateType.Weekly:
                    postData.ReportDate = date.ToString("yyyyMMdd");
                    break;
                case AppleiTunesReportDateType.Monthly:
                    postData.ReportDate = date.ToString("yyyyMM");
                    break;
                case AppleiTunesReportDateType.Yearly:
                    postData.ReportDate = date.ToString("yyyy");
                    break;
                default:
                    postData.ReportDate = "";
                    break;
            }

            return GetParamsAsUrl(postData);
        }

        private string GetParamsAsUrl(PostData postData)
        {
            var sb = new StringBuilder();

            sb.Append("USERNAME=" + HttpUtility.UrlEncode(postData.Username));
            sb.Append("&PASSWORD=" + HttpUtility.UrlEncode(postData.Password));
            sb.Append("&VNDNUMBER=" + HttpUtility.UrlEncode(postData.VndNumber));
            sb.Append("&TYPEOFREPORT=" + HttpUtility.UrlEncode(postData.TypeOfReport));
            sb.Append("&DATETYPE=" + HttpUtility.UrlEncode(postData.DateType));
            sb.Append("&REPORTTYPE=" + HttpUtility.UrlEncode(postData.ReportType));
            sb.Append("&REPORTDATE=" + HttpUtility.UrlEncode(postData.ReportDate));

            return sb.ToString();
        }
    }

    internal class PostData
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string VndNumber { get; set; }
        public string TypeOfReport { get; set; }
        public string DateType { get; set; }
        public string ReportType { get; set; }
        public string ReportDate { get; set; }
    }
}
