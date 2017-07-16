using System;
using System.Threading.Tasks;

using Foundation;
using UIKit;

using JsBridge;

namespace JsBridgeTest
{
    public partial class WebViewController : UIViewController, IWebBrowser
    {
        
        static bool UserInterfaceIdiomIsPhone
        {
            get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
        }

        protected WebViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.BecomeFirstResponder();

            // Intercept URL loading to handle native calls from browser
            WebView.ShouldStartLoad += HandleShouldStartLoad;
            WebView.LoadFinished += M_WebView_LoadFinished;


            JSBridge.EnableJSBridge(this);
            JSBridge.RegisterObject("tobj", new TestClass());

            // Render the view from the type generated from RazorView.cshtml
            var model = new Model1 { Text = "Text goes here" };
            var template = new RazorView { Model = model };
            var page = template.GenerateString();

            // Load the rendered HTML into the view with a base URL 
            // that points to the root of the bundled Resources folder
            WebView.LoadHtmlString(page, NSBundle.MainBundle.BundleUrl);

            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        bool HandleShouldStartLoad(UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            
            // If the URL is not our own custom scheme, just let the webView load the URL as usual
            const string scheme = "jsb";

            if (request.Url.Scheme != null && request.Url.Scheme.Contains(scheme))
            {
				string objName = request.Url.Host;
				string dataraw = request.Url.Query.Split('=')[1];
				string json = System.Web.HttpUtility.UrlDecode(dataraw).Replace("&_", "");

				JsReturn result = JSBridge.RaiseEvent(JsTelegram.DeserializeObject(json));

				return true;
            }
            return true;

			
        }

        public void ExecuteJavaScript(string code)
        {
            this.WebView.EvaluateJavascript(code);
        }

        public bool PageIsLoaded()
        {
            return !this.WebView.IsLoading;
        }

        private async void M_WebView_LoadFinished(object sender, EventArgs e)
        {
            await Task.Delay(1000);
            JSBridge.UpdateObjectsInBrowser();
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
        }
    }
}

