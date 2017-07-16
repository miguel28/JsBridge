using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

#if __UNIFIED__
using UIKit;
using Foundation;
using WebView = UIKit.UIWebView;
using Class = ObjCRuntime.Class;

// Mappings Unified CoreGraphic classes to MonoTouch classes
using CGRect = global::System.Drawing.RectangleF;
using CGSize = global::System.Drawing.SizeF;
using CGPoint = global::System.Drawing.PointF;

// Mappings Unified types to MonoTouch types
using nfloat = global::System.Single;
using nint = global::System.Int32;
using nuint = global::System.UInt32;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.WebKit;
using WebView = MonoMac.WebKit.WebView;
using Class = MonoMac.ObjCRuntime.Class;
#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using WebView = MonoTouch.UIKit.UIWebView;
using Class = MonoTouch.ObjCRuntime.Class;
#endif

namespace JsBridge
{
    public class AppProtocolHandler : NSUrlProtocol
    {
		[Export("canInitWithRequest:")]
		public static bool canInitWithRequest(NSUrlRequest request)
		{
			return request.Url.Scheme == "jsb";
		}

		[Export("canonicalRequestForRequest:")]
		public static new NSUrlRequest GetCanonicalRequest(NSUrlRequest forRequest)
		{
			return forRequest;
		}

		public AppProtocolHandler(IntPtr ptr) : base(ptr)
		{
		}

#if __UNIFIED__
		public AppProtocolHandler(NSUrlRequest request, NSCachedUrlResponse cachedResponse, INSUrlProtocolClient client)
			: base(request, cachedResponse, client)
		{
		}
#else

#if !MONOMAC
        [Export ("initWithRequest:cachedResponse:client:")]
#endif
        public AppProtocolHandler (NSUrlRequest request, NSCachedUrlResponse cachedResponse, NSUrlProtocolClient client) 
            : base (request, cachedResponse, client)
        {
        }

#endif

		public override void StartLoading()
		{
            try
            {
				string objName = Request.Url.Host;
				string dataraw = Request.Url.Query.Split('=')[1];
				string json = System.Web.HttpUtility.UrlDecode(dataraw).Replace("&_", "");

				JsReturn result = JSBridge.RaiseEvent(JsTelegram.DeserializeObject(json));

				// indicate success.
				var data = NSData.FromString(result.Serialize());
                Console.WriteLine(data);
				using (var response = new NSUrlResponse(Request.Url, "application/json", Convert.ToInt32(data.Length), "utf-8"))
				{
					Client.ReceivedResponse(this, response, NSUrlCacheStoragePolicy.NotAllowed);
					Client.DataLoaded(this, data);
					Client.FinishedLoading(this);
				}
			}
            catch(Exception)
            {
				Client.FailedWithError(this, NSError.FromDomain(new NSString("AppProtocolHandler"), Convert.ToInt32(NSUrlError.ResourceUnavailable)));
				Client.FinishedLoading(this);
            }
		}

		public override void StopLoading()
		{
		}
    }
}
