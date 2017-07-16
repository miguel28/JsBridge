using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JsBridge
{
	public class JsTelegram
	{
		public string ObjName = "";
		public string MethodName = "";
		public List<object> Parameters = new List<object>();

		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}

		public static JsTelegram DeserializeObject(string data)
		{
			return JsonConvert.DeserializeObject<JsTelegram>(data);
		}
	}

	public class JsReturn
	{
		public object Value = null;

		public static JsReturn Return(object o)
		{
			JsReturn ret = new JsReturn();
			ret.Value = o;
			return ret;
		}

		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}

		public static JsReturn DeserializeObject(string data)
		{
			return JsonConvert.DeserializeObject<JsReturn>(data);
		}
	}
}
