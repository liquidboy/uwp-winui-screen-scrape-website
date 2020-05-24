using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Reflection;
using System.Threading.Tasks;

namespace screenscrape_website
{
    public sealed partial class MainPage : Page
    {
        private WebView2 _wv;

        public MainPage()
        {
            this.InitializeComponent();

            _wv = new WebView2()
            {
                Margin = new Thickness() { Left = 0, Top = 155, Right = 0, Bottom = 0 }
            };
            _wv.WebMessageReceived += _wv_WebMessageReceived;
            _wv.NavigationCompleted += _wv_NavigationCompleted;
            layoutRoot.Children.Add(_wv);

            cbUrls.Items.Add("https://flatuicolors.com");
            cbUrls.SelectionChanged += CbUrls_SelectionChanged;

            cbConversionTargets.Items.Add("unity ColorLibrary");
            cbConversionTargets.Items.Add("uwp ResourceDictionary");
            cbConversionTargets.SelectionChanged += CbConversionTargets_SelectionChanged;
        }

        private void CbConversionTargets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            butConvert.IsEnabled = true;
            tbConversionResult.Text = string.Empty;
        }

        private void CbUrls_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbScript.Text = string.Empty;
            butInject.IsEnabled = false;

            butConvert.IsEnabled = false;
            cbConversionTargets.SelectedIndex = -1;
            tbConversionResult.Text = string.Empty;
        }

        private void _wv_NavigationCompleted(WebView2 sender, WebView2NavigationCompletedEventArgs args)
        {
            if (!args.IsSuccess) return;
            var json = LoadJsonFromEmbeddedResource(_wv.Source.Host);
            tbScript.Text = json;
            butInject.IsEnabled = true;
        }

        private string LoadJsonFromEmbeddedResource(string siteUrl) {

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith($"{siteUrl.ToLower()}.js"));

            var json = string.Empty;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }

            return json;
        }

        private void _wv_WebMessageReceived(WebView2 sender, WebView2WebMessageReceivedEventArgs args)
        {
            if (args.WebMessageAsString == "clear-textbox") tbCallback.Text = "";
            else tbCallback.Text += args.WebMessageAsString;
        }

        private async void butSearch_Click(object sender, RoutedEventArgs e)
        {
            if(cbUrls.SelectedValue != null)
                _wv.Source = new Uri((string)cbUrls.SelectedValue);
        }

        private async void butInject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _wv.ExecuteScriptAsync(tbScript.Text);
            }
            catch (Exception ex)
            {
                // todo: handle exceptions
            }

        }

        private void butConvert_Click(object sender, RoutedEventArgs e)
        {
            DoConversion((string)cbConversionTargets.SelectedValue);
        }

        private void DoConversion(string conversionType)
        {
            var parseString = tbCallback.Text;

            var lines = parseString.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines) {
                var cleanedString = line.Trim().ToLower();
                if (cleanedString.Substring(0, 3) == "rgb") {
                    var parts = cleanedString.Split(";");
                    cleanedString = parts[0].Replace("rgb(", string.Empty).Replace(")",string.Empty);
                    var colorParts = cleanedString.Split(",");

                    var r = int.Parse(colorParts[0]) / 255f;
                    var g = int.Parse(colorParts[1]) / 255f;
                    var b = int.Parse(colorParts[2]) / 255f;

                    var formatColor = $@"  - m_Name: 
    m_Color: {{r: {r}, g: {g}, b: {b}, a: 1}}
";

                    tbConversionResult.Text += formatColor;
                }
            }
        }

    }
}
