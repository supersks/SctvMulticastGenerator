using System.Data;
using System.Text;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

namespace SctvMulticastGenerator
{
    internal class Generator
    {
        private const string _baseAddress = "http://epg.51zmt.top:8000";
        private const string _sctvMulticastaddress = $"{_baseAddress}/sctvmulticast.html";
        private const string _uploadAddress = $"{_baseAddress}/api/upload/";
        private string outputfilePath;
        private readonly HttpClient client;

        public Generator(string outputfilePath)
        {
            this.client = new HttpClient();
            this.outputfilePath = Path.GetFullPath(outputfilePath);
        }

        internal async Task Run()
        {
            Console.WriteLine("Start Generate Sctv Multicast.");

            Console.WriteLine("Start Get Sctv Multicast DataTable.");
            DataSet dataSet = await GetMulticastDataTable();
            Console.WriteLine("Done Get Sctv Multicast DataTable.");

            StringBuilder sb = new StringBuilder();
            foreach (DataTable table in dataSet.Tables)
            {
                Console.WriteLine($"Table {table.TableName} has {table.Rows.Count} channels.");
                foreach (DataRow row in table.Rows)
                {
                    sb.AppendLine(row[1].ToString());
                    sb.AppendLine($"rtp://{row[2].ToString()}");
                    sb.AppendLine();
                }
            }

            Console.WriteLine("Start Get Download Address.");
            var downloadAddress = await ConvertToFormal(sb);
            Console.WriteLine("Done Get Download Address.");

            Console.WriteLine("Start Download.");
            await DownloadFile(downloadAddress);
            Console.WriteLine("Downloaded.");
        }

        private async Task DownloadFile(string downloadAddress)
        {
            var download = await this.client.GetAsync(downloadAddress);
            if (download.IsSuccessStatusCode)
            {
                using FileStream fs = File.Create(this.outputfilePath);
                var streamFromRemote = await download.Content.ReadAsStreamAsync();
                await streamFromRemote.CopyToAsync(fs);
            }
        }

        private async Task<string> ConvertToFormal(StringBuilder sb)
        {
            string tempFile = "temp.m3u";
            try
            {
                using (FileStream fileStream = File.Open(tempFile, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using StreamWriter writer = new StreamWriter(fileStream);
                    writer.Write(sb.ToString());
                    writer.Flush();
                }
                using (FileStream fileStream = File.Open(tempFile, FileMode.Open, FileAccess.Read))
                {
                    MultipartFormDataContent httpContent = new MultipartFormDataContent
                {
                    { new StreamContent(fileStream), "myfile", tempFile }
                };
                    var result = await this.client.PostAsync(_uploadAddress, httpContent);
                    if (!result.IsSuccessStatusCode)
                    {
                        return string.Empty;
                    }
                    var address = await result.Content.ReadAsStringAsync();
                    string reg = @"<a[^>]*href=([""'])?(?<href>[^'""]+)\1[^>]*>";
                    var item = Regex.Match(address, reg, RegexOptions.IgnoreCase);
                    return item.Groups["href"].Value;
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        private async Task<DataSet> GetMulticastDataTable()
        {
            var result = await this.client.GetStringAsync(_sctvMulticastaddress);
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(result);

            DataSet dataSet = new DataSet();
            foreach (HtmlNode table in htmlDocument.DocumentNode.SelectNodes("//table"))
            {
                DataTable dataTable = new DataTable(table.Id);
                foreach (HtmlNode row in table.SelectNodes("tr"))
                {
                    HtmlNodeCollection list = row.SelectNodes("th|td");
                    if (dataTable.Columns.Count == 0)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            HtmlNode cell = list[i];
                            dataTable.Columns.Add(cell.InnerText);
                        }
                    }
                    else
                    {
                        DataRow dataRow = dataTable.NewRow();
                        for (int i = 0; i < list.Count; i++)
                        {
                            HtmlNode cell = list[i];
                            dataRow[i] = cell.InnerText;
                        }
                        dataTable.Rows.Add(dataRow);
                    }
                }
                dataSet.Tables.Add(dataTable);
            }
            return dataSet;
        }
    }
}
