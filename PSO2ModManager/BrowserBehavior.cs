using System.Windows;
using System.Windows.Controls;

namespace PSO2ModManager
{
    public class BrowserBehavior
    {
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
                "Html",
                typeof(string),
                typeof(BrowserBehavior),
                new FrameworkPropertyMetadata(OnHtmlChanged));

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static string GetHtml(WebBrowser d) {
            return (string)d.GetValue(HtmlProperty);
        }

        public static void SetHtml(WebBrowser d, string value) {
            d.SetValue(HtmlProperty, value);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Controls.WebBrowser.NavigateToString(System.String)")]
        private static void OnHtmlChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) {
            WebBrowser webBrowser = dependencyObject as WebBrowser;
            var value = "&nbsp;";
            if (System.String.IsNullOrEmpty(e.NewValue as string))
                value = e.NewValue as string;
            if (webBrowser != null)
                webBrowser.NavigateToString(value);
        }
    }
}