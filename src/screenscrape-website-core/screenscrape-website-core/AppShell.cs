using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace screenscrape_website_core
{
    public sealed class AppShell : Control
    {
        public AppShell()
        {
            this.DefaultStyleKey = typeof(AppShell);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var layoutRoot = GetTemplateChild("layoutRoot") as Border;
            layoutRoot.DataContext = this;
        }

        public ContentControl HeaderContent
        {
            get { return (ContentControl)GetValue(HeaderContentProperty); }
            set { SetValue(HeaderContentProperty, value); }
        }

        public ContentControl BodyContent
        {
            get { return (ContentControl)GetValue(BodyContentProperty); }
            set { SetValue(BodyContentProperty, value); }
        }

        public ContentControl FooterContent
        {
            get { return (ContentControl)GetValue(FooterContentProperty); }
            set { SetValue(FooterContentProperty, value); }
        }

        public static readonly DependencyProperty FooterContentProperty =
            DependencyProperty.Register("FooterContent", typeof(ContentControl), typeof(AppShell), new PropertyMetadata(0));

        public static readonly DependencyProperty BodyContentProperty =
            DependencyProperty.Register("BodyContent", typeof(ContentControl), typeof(AppShell), new PropertyMetadata(0));

        public static readonly DependencyProperty HeaderContentProperty =
            DependencyProperty.Register("HeaderContent", typeof(ContentControl), typeof(AppShell), new PropertyMetadata(0));
    }
}
