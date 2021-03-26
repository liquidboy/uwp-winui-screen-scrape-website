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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT;

namespace screenscrape_website_core
{
    public sealed partial class CurrencyConverter : Window
    {
        private WebView2 _wv;


        public CurrencyConverter()
        {
            this.InitializeComponent();
            SetupWebViewScraper();
        }

        void SetupWebViewScraper() {
            _wv = new WebView2()
            {
                Margin = new Thickness() { Left = -1000, Top = 0, Right = 0, Bottom = 0 },
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 150,
            };
            _wv.WebMessageReceived += _wv_WebMessageReceived;
            _wv.NavigationCompleted += _wv_NavigationCompleted;
            layoutRoot.Children.Add(_wv);
        }

        private async void _wv_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            if (!args.IsSuccess) return;
            var json = LoadJsonFromEmbeddedResource("do-currency-conversion.js");
            try
            {
                await _wv.ExecuteScriptAsync(json);
            }
            catch (Exception ex)
            {
                // todo: handle exceptions
            }
        }

        private void _wv_WebMessageReceived(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs args)
        {
            var msg = args.TryGetWebMessageAsString();
            JObject o = JObject.Parse(msg);
            lblConversionAmount.Text = o["result"].Value<string>();
        }


        private string LoadJsonFromEmbeddedResource(string siteUrl)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith($"do-currency-conversion.js"));

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
            lblConversionAmount.Text = "processing ....";

            var url = $"https://www.xe.com/currencyconverter/convert/?Amount={ tbAmount.Text }&From={ ((ComboBoxItem)cbFrom.SelectedValue).Content }&To={ ((ComboBoxItem)cbTo.SelectedValue).Content }";
            _wv.Source = new Uri(url);
        }
    }
}
