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

    interface IWebViewResult { 
    }

    class WebviewService
    {
        WebView2 _wv;

        Queue<string> _calls = new Queue<string>();
        public ObservableCollection<IWebViewResult> _results = new ObservableCollection<IWebViewResult>();
        bool _isProcessingCall = false;
        public int msTillNextCall = 500;

        public WebView2 CurrentWebView => _wv;
        public bool IsProcessingCall => _isProcessingCall;
        private Func<JObject, IWebViewResult> _dataConverter;
        private Action _onCompletedRecievingUpdate;
        private string _scriptToCall;

        public void SetupWebView(Func<JObject, IWebViewResult> dataConverter, Action onCompletedRecievingUpdate, string scriptToCall) {
            _onCompletedRecievingUpdate = onCompletedRecievingUpdate;
            _dataConverter = dataConverter;
            _scriptToCall = scriptToCall;
            _wv = new WebView2()
            {
                Margin = new Thickness() { Left = 0, Top = 60, Right = 0, Bottom = 0 },
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch,
                //Width = 150,
                Height = 600,
                Visibility = Visibility.Collapsed
            };
            _wv.WebMessageReceived += _wv_WebMessageReceived;
            _wv.NavigationCompleted += _wv_NavigationCompleted;
        }

        private async void _wv_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            if (!args.IsSuccess) return;
            var json = LoadJsonFromEmbeddedResource(_scriptToCall);
            try
            {
                await _wv.ExecuteScriptAsync(json);
            }
            catch (Exception ex)
            {
                // todo: handle exceptions
            }
        }

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

        private void _wv_WebMessageReceived(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs args)
        {
            StopProcessingCall();

            var msg = args.TryGetWebMessageAsString();
            JObject o = JObject.Parse(msg);
            var result = _dataConverter(o);
            _results.Add(result);

            _onCompletedRecievingUpdate?.Invoke();
            ProcessJob(msTillNextCall);
        }

        public void StopProcessingCall()
        {
            _isProcessingCall = false;
        }

        public void ClearAll()
        {
            _results.Clear();
            _calls.Clear();
            _isProcessingCall = false;
        }

        public void AddJob(string url) => _calls.Enqueue(url);

        public void ProcessJob(int waitMillisecondsBeforeNextCall = 0) {
            // do job
            if (_calls.Count > 0)
            {
                if (waitMillisecondsBeforeNextCall > 0) System.Threading.Thread.Sleep(waitMillisecondsBeforeNextCall);
                _isProcessingCall = true;
                var url = _calls.Dequeue();
                _wv.Source = new Uri(url);
            }
        }

        public bool HasJobs() => _calls.Count > 0;
    }
}
