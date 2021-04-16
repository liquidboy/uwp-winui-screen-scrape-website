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
    class WebviewService<T>
    {
        WebView2 _wv;

        Queue<string> _calls = new Queue<string>();
        public ObservableCollection<T> _results = new ObservableCollection<T>();
        bool _isProcessingCall = false;
        public int msTillNextCall = 500;

        public WebView2 CurrentWebView => _wv;
        public bool IsProcessingCall => _isProcessingCall;
        private Func<JObject, T> _dataConverter;
        private Action _onCompletedRecievingUpdate;

        public void SetupWebView(Func<JObject, T> dataConverter, Action onCompletedRecievingUpdate) {
            _onCompletedRecievingUpdate = onCompletedRecievingUpdate;
            _dataConverter = dataConverter;
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
