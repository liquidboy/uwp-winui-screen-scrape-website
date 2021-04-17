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
        WebviewService webviewService = new WebviewService();
        string[] _coins = { "THETA", "THETA-FUEL", "OMG", "flamingo", "stellar", "cardano", "hedera-hashgraph", "bitcoin", "ethereum" };

        private class SearchResult : IWebViewResult
        {
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
            webviewService.SetupWebView(
                (o) => {
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
                    return result;
                }, 
                ()=> {
                    UpdateUI();
                },
                "do-cryptomarket-search.js"
            );
            layoutRoot.Children.Add(webviewService.CurrentWebView);
            lbResults.ItemsSource = webviewService._results;
        }

        private void butDoConversion_Click(object sender, RoutedEventArgs e)
        {
            if (webviewService.IsProcessingCall) return;

            ClearAll();
            lblProcessing.Text = "processing ...";

            foreach (var cur in _coins)
            {
                var url = $"https://coinmarketcap.com/currencies/{ cur }/";
                webviewService.AddJob(url);
            }

            UpdateUI();
            webviewService.ProcessJob(0);
        }

        private void UpdateUI() {
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
