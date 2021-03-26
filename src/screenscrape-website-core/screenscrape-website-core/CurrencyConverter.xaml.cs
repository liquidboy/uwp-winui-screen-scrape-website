using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace screenscrape_website_core
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
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
                Margin = new Thickness() { Left = 0, Top = 155, Right = 0, Bottom = 0 }
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
            lblConversionAmount.Text = msg;
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

            var from = "AUD";
            var to = "USD";
            var url = $"https://www.xe.com/currencyconverter/convert/?Amount={ tbAmount.Text }&From={ ((ComboBoxItem)cbFrom.SelectedValue).Content }&To={ ((ComboBoxItem)cbTo.SelectedValue).Content }";
            _wv.Source = new Uri(url);
        }
    }
}
