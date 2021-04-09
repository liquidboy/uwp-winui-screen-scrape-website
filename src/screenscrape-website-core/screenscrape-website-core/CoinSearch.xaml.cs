using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT;

namespace screenscrape_website_core
{
    public sealed partial class CoinSearch : Window
    {
        WebviewService<SearchResult> webviewService = new WebviewService<SearchResult>();
        string[] _currencies = { "THETA", "THETA-FUEL", "OMG", "flamingo", "stellar", "cardano", "hedera-hashgraph", "bitcoin", "ethereum" };

        private class SearchResult {
            public string Name { get; set; }
            public string Code { get; set; }
            public string Logo { get; set; }
            public decimal Price { get; set; }
            public decimal CirculatingSupply { get; set; }
            public decimal MaxSupply { get; set; }
            public decimal TotalSupply { get; set; }
            public decimal VolumeLast24Hrs { get; set; }
        }

        public CoinSearch()
        {
            this.InitializeComponent();
            SetupWebViewScraper();
        }

        void ClearAll() {
            webviewService.ClearAll();
            lblProcessing.Text = "";
        }

        void SetupWebViewScraper() {
            webviewService.SetupWebView();
            webviewService.CurrentWebView.WebMessageReceived += _wv_WebMessageReceived;
            webviewService.CurrentWebView.NavigationCompleted += _wv_NavigationCompleted;
            layoutRoot.Children.Add(webviewService.CurrentWebView);

            lbResults.ItemsSource = webviewService._results;
        }

        private async void _wv_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            if (!args.IsSuccess) return;
            var json = LoadJsonFromEmbeddedResource("do-currency-conversion.js");
            try
            {
                await webviewService.CurrentWebView.ExecuteScriptAsync(json);
            }
            catch (Exception ex)
            {
                // todo: handle exceptions
            }
        }

        private void _wv_WebMessageReceived(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs args)
        {
            webviewService.StopProcessingCall();

            var msg = args.TryGetWebMessageAsString();
            JObject o = JObject.Parse(msg);

            var result = new SearchResult()
            {
                Name = o["name"].Value<string>(),
                Code = o["code"].Value<string>(),
                Logo = o["logo"].Value<string>(),
                Price = Convert.ToDecimal(o["price"].Value<string>()),
                CirculatingSupply = Convert.ToDecimal(o["circulatingSupply"].Value<string>()), //{0:C}
                MaxSupply = Convert.ToDecimal(o["maxSupply"].Value<string>()),
                TotalSupply = Convert.ToDecimal(o["totalSupply"].Value<string>()),
                VolumeLast24Hrs = Convert.ToDecimal(o["volume24hr"].Value<string>()),
            };

            webviewService._results.Add(result);

            ProcessCalls(webviewService._msTillNextCall);
        }

        private string LoadJsonFromEmbeddedResource(string siteUrl)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith($"do-cryptomarket-search.js"));

            var json = string.Empty;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }

            return json;
        }

        private void butDoConversion_Click(object sender, RoutedEventArgs e)
        {
            if (webviewService.IsProcessingCall) return;

            ClearAll();
            lblProcessing.Text = "processing ...";

            foreach (var cur in _currencies)
            {
                var url = $"https://coinmarketcap.com/currencies/{ cur }/";
                webviewService.AddJob(url);
            }

            ProcessCalls();
        }

        private void ProcessCalls(int waitMillisecondsBeforeNextCall = 0) 
        {
            UpdateUI();
            webviewService.ProcessJob(waitMillisecondsBeforeNextCall);
        }

        private void UpdateUI() {
            // update ui to let user know its processing
            if (!webviewService.HasJobs())
            {
                lblProcessing.Text = "";
            }
            else if (webviewService.HasJobs())
            {
                lblProcessing.Text += ".";
            }
        }


    }
}
