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
                () => { }
            );
            //webviewService.CurrentWebView.WebMessageReceived += _wv_WebMessageReceived;
            webviewService.CurrentWebView.NavigationCompleted += _wv_NavigationCompleted;
            layoutRoot.Children.Add(webviewService.CurrentWebView);

            foreach (var cur in _currencies) {
                cbFrom.Items.Add(new ComboBoxItem() { Content = cur });
            }
            cbFrom.SelectedIndex = 0;
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

        //private void _wv_WebMessageReceived(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs args)
        //{
        //    webviewService.StopProcessingCall();

        //    var msg = args.TryGetWebMessageAsString();
        //    JObject o = JObject.Parse(msg);

        //    var parts = o["result"].Value<string>().Split(" ");

        //    var result = new CurrencyConversionResult()
        //    {
        //        Result = o["result"].Value<string>(),
        //        Amount = double.Parse(parts[0]),
        //        CurrencyFrom = o["from"].Value<string>(),
        //        CurrencyTo = o["to"].Value<string>()
        //    };

        //    for (int i = 1; i < parts.Length; i++)
        //    {
        //        result.FriendlyCurrency += parts[i] + " ";
        //    }

        //    webviewService._results.Add(result);

        //    ProcessCalls(webviewService.msTillNextCall);
        //}

        private string LoadJsonFromEmbeddedResource(string injectJsFile)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(injectJsFile));

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
                var from = ((ComboBoxItem)cbFrom.SelectedValue).Content;
                var to = cur;
                if (!from.Equals(to)) {
                    var url = $"https://www.xe.com/currencyconverter/convert/?Amount={ tbAmount.Text }&From={ from }&To={ to }";
                    webviewService.AddJob(url);
                }
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
