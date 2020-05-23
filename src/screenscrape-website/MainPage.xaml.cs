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
            layoutRoot.Children.Add(_wv);
            tbSearch.Text = "https://flatuicolors.com";

            // js code is explicitly written to scrape flatuicolors.com
            var json = @" // injected js from uwp host into webview2
if(!!window['injectedFunction'] === false) {
    function injectedFunction() {
        var foundColors = document.getElementsByClassName('color'); 
        var foundAuthors = document.getElementsByClassName('author');
        var foundAuthor = undefined;

        alert(`${foundColors.length} colors found`); 
        if(foundAuthors !== undefined && foundAuthors.length === 1) {
            foundAuthor = foundAuthors[0].text;
        }
        
        window.chrome.webview.postMessage(`clear-textbox`);
        window.chrome.webview.postMessage(`sending ${foundColors.length} colors from webview2 to uwp host \n\r`);
        if(foundAuthor !== undefined) window.chrome.webview.postMessage(`author ${foundAuthor} \n\r`);

        var foundElements = Array.prototype.filter.call(foundColors, function(xe){
            var colorName = '';
            if(xe.children !== undefined && xe.children.length > 0) {
                colorName = xe.children[1].innerText;
            }
            window.chrome.webview.postMessage(`${xe.style.background}; ${colorName} \n`);
            return xe.style;
        });

    };
    alert('js injected from UWP & function created in webview2');
};

// execute injected function
if(!!window['injectedFunction'] === true) {
    window['injectedFunction']();
};
";

            tbScript.Text = json;
        }

        private void _wv_WebMessageReceived(WebView2 sender, WebView2WebMessageReceivedEventArgs args)
        {
            if (args.WebMessageAsString == "clear-textbox") tbCallback.Text = "";
            else tbCallback.Text += args.WebMessageAsString;
        }

        private void butSearch_Click(object sender, RoutedEventArgs e)
        {
            _wv.Source = new Uri(tbSearch.Text);
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
    }
}
