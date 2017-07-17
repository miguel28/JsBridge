using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using Foundation;
using WebView = UIKit.UIWebView;
using Class = ObjCRuntime.Class;

namespace JsBridge
{
	public static class JSBridge
	{
		private static IWebBrowser m_browser;
		private static Dictionary<string, ObjectInfo> m_info_objects = new Dictionary<string, ObjectInfo>();
		private static Dictionary<string, object> m_objects = new Dictionary<string, object>();
        private static bool ProtocolRegistered = false;

        private static string JSDeclareObject = @"
        var CSObject = function(jsonstring)
        {
          this.object = JSON.parse(jsonstring);
          this.ObjName = this.object.ObjName;
        };
        CSObject.prototype.callMethod = function(methodData, parameters)
        {
          var method = JSON.parse(methodData);
          var ret = false;
        
          var telegram = 
          {
            ObjName: this.ObjName,
            MethodName: method.Name,
            Parameters: parameters
          };
          
          var url = 'jsb://' +this.ObjName + '/?&data=' 
            + encodeURIComponent(JSON.stringify(telegram)) + '&_=' + Math.random();

            $.ajax({
                url: url,
                async: false,
                beforeSend: function(xhr)
                {
                    xhr.setRequestHeader('accept', 'application/json');
                    xhr.setRequestHeader('Content-Type', 'application/json');  
                },
                success: function(data) {
                    if (data.Value)
                    {
                        ret = data.Value;
                    }
                },
                error: function (jqXHR, exception) {
                    var msg = '';
                    if (jqXHR.status === 0) {
                        msg = 'Not connect.\n Verify Network.';
                    } else if (jqXHR.status == 404) {
                        msg = 'Requested page not found. [404]';
                    } else if (jqXHR.status == 500) {
                        msg = 'Internal Server Error [500].';
                    } else if (exception === 'parsererror') {
                        msg = 'Requested JSON parse failed.';
                    } else if (exception === 'timeout') {
                        msg = 'Time out error.';
                    } else if (exception === 'abort') {
                        msg = 'Ajax request aborted.';
                    } else {
                        msg = 'Uncaught Error.\n' + jqXHR.responseText;
                    }
                    console.log(msg);
                },
                dataType:'json',
                type: 'POST',
                crossDomain: true
            });
          return ret;
        };

        CSObject.prototype.create = function()
        {
          for (var i = 0; i < this.object.Methods.length; i++ )
          {
            var method = this.object.Methods[i];
            var methodcreate = 'CSObject.prototype.' +method.Name + ' = function(';
            var appendpar = 'parameters = []; ';

            var j = 0;
            for (j = 0; j < method.Parameters.length - 1; j++ )
            {
              methodcreate += method.Parameters[j].Name + ', ';
              appendpar += 'parameters.push(' + method.Parameters[j].Name + ');';
            }

            if (method.Parameters.length > 0)
            {
                appendpar += 'parameters.push(' + method.Parameters[j].Name + ');'; 
                methodcreate += method.Parameters[j].Name + '){ ' + appendpar;
            }
            else
            {
                methodcreate += '){ ' + appendpar;
            }
            
            methodcreate += 'var methodData = \'' + JSON.stringify(method) + '\';'; 
            methodcreate += ' var objmethod = method; return this.callMethod(methodData, parameters);};';
            eval(methodcreate);
          }
        };";

		public static void EnableJSBridge(IWebBrowser browser)
		{
			m_browser = browser;
			if (m_browser == null)
			{
				throw new ArgumentNullException("IWebBrowser cannot be null");
			}
            if (!ProtocolRegistered)
            {
                NSUrlProtocol.RegisterClass(new Class(typeof(AppProtocolHandler)));
                ProtocolRegistered = true;
            }
		}

		public static void RegisterObject(string name, object obj)
		{
			if (!m_info_objects.ContainsKey(name))
			{
				ObjectInfo info = ReflectionHelper.GetObjectInfo(name, obj);
				m_info_objects.Add(name, info);
				m_objects.Add(name, obj);
			}
		}

		public static void DeRegisterObject(string name)
		{
			if (m_info_objects.ContainsKey(name))
			{
				m_info_objects.Remove(name);
				m_objects.Remove(name);
			}
		}


		private static void InjectJSEnvironment()
		{
			if (m_browser.PageIsLoaded())
			{
				m_browser.ExecuteJavaScript("");
				UpdateObjectsInBrowser();
			}
		}

		public static void UpdateObjectsInBrowser()
		{
			foreach (string key in m_info_objects.Keys)
			{
				string objstr = m_info_objects[key].Serialize();
                string js = string.Format("var {0} = new CSObject{0}('{1}'); {0}.create();", key, objstr);
                string totaljs = JSDeclareObject.Replace("CSObject", "CSObject"+key).Replace("\r\n","");
                m_browser.ExecuteJavaScript(totaljs);
                m_browser.ExecuteJavaScript(js);
			}
		}

		public static JsReturn RaiseEvent(JsTelegram telegram)
		{
			object ret = null;
			string name = telegram.ObjName;
			object obj = m_objects[name];
			Type t = obj.GetType();
			ObjectInfo info = m_info_objects[name];

            try
            {
				System.Reflection.MethodInfo method = t.GetMethod(telegram.MethodName);
				ret = method.Invoke(obj, telegram.Parameters.ToArray());
			}
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
			return JsReturn.Return(ret);
		}
	}
}
