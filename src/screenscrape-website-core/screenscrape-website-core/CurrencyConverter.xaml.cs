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
    public sealed partial class CurrencyConverter : Window
    {
        WebviewService<CurrencyConversionResult> webviewService = new WebviewService<CurrencyConversionResult>();
        string[] _currencies = { "AUD", "CAD", "CNY", "EUR", "JPY", "GBP", "USD" };

        private class CurrencyConversionResult {
            public double Amount { get; set; }
            public string FriendlyCurrency { get; set; }
            public string Result { get; set; }
            public string CurrencyTo { get; set; }
            public string CurrencyFrom { get; set; }
        }

        public CurrencyConverter()
        {
            this.InitializeComponent();
            SetupWebViewScraper();
        }

        void ClearAll() {
            webviewService.ClearAll();
            lblProcessing.Text = "";
        }

        void SetupWebViewScraper()
        {
            webviewService.SetupWebView(
                (o) => {
                    var parts = o["result"].Value<string>().Split(" ");
                    var result = new CurrencyConversionResult()
                    {
                        Result = o["result"].Value<string>(),
                        Amount = double.Parse(parts[0]),
                        CurrencyFrom = o["from"].Value<string>(),
                        CurrencyTo = o["to"].Value<string>()
                    };
                    return result;
                }, 
                () => { },
                "do-currency-conversion.js"
            );
            layoutRoot.Children.Add(webviewService.CurrentWebView);

            foreach (var cur in _currencies) {
                cbFrom.Items.Add(new ComboBoxItem() { Content = cur });
            }
            cbFrom.SelectedIndex = 0;
            lbResults.ItemsSource = webviewService._results;
        }

        private void butDoConversion_Click(object sender, RoutedEventArgs e)
        {
            if (webviewService.IsProcessingCall) return;

            ClearAll();
            lblProcessing.Text = "processing ...";

            foreach (var cur in _currencies)
            {
                var from = ((ComboBoxItem)cbFrom.SelectedValue).Content;
                var to = cur;
                if (!from.Equals(to)) {
                    var url = $"https://www.xe.com/currencyconverter/convert/?Amount={ tbAmount.Text }&From={ from }&To={ to }";
                    webviewService.AddJob(url);
                }
            }

            UpdateUI();
            webviewService.ProcessJob(0);
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
