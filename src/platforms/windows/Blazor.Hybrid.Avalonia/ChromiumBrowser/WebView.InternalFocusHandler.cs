using Xilium.CefGlue;
using Xilium.CefGlue.Common.Handlers;

namespace Blazor.Hybrid.Avalonia;

partial class WebView {

    private class InternalFocusHandler : FocusHandler {

        private WebView OwnerWebView { get; }

        public InternalFocusHandler(WebView webView) {
            OwnerWebView = webView;
        }

        protected override void OnGotFocus(CefBrowser browser) {
            OwnerWebView.OnGotFocus();
        }

        protected override bool OnSetFocus(CefBrowser browser, CefFocusSource source) {
            //todo
            return false;
        }

        protected override void OnTakeFocus(CefBrowser browser, bool next) {
            OwnerWebView.OnLostFocus();
        }
    }
}
