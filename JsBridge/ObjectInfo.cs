using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JsBridge
{
	public class ObjectInfo
	{
		public String ObjName = "";
		public List<MethodInfo> Methods = new List<MethodInfo>();

		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}

		public static ObjectInfo DeserializeObject(string data)
		{
			return JsonConvert.DeserializeObject<ObjectInfo>(data);
		}
	}

	public class MethodInfo
	{
		public string Name = "";
		public List<ParameterInfo> Parameters = new List<ParameterInfo>();
	}

	public class ParameterInfo
	{
		public string Name = "";
		public string Type = "";
		public object Value = 0;
	}
}
