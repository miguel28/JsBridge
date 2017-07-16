using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace JsBridge
{
    public static class ReflectionHelper
    {
		public static ObjectInfo GetObjectInfo(string name, object obj)
		{
			Type t = obj.GetType();
			ObjectInfo info = new ObjectInfo();
			info.ObjName = name;

			foreach (System.Reflection.MethodInfo m in t.GetMethods())
			{
				MethodInfo minfo = new MethodInfo();
				minfo.Name = m.Name;

				if (m.Name.Contains("Equals"))
					continue;
				if (m.Name.Contains("GetType"))
					continue;
				if (m.Name.Contains("GetHashCode"))
					continue;


				foreach (System.Reflection.ParameterInfo p in m.GetParameters())
				{
					ParameterInfo pinfo = new ParameterInfo();
					pinfo.Name = p.Name;
					pinfo.Type = p.ParameterType.ToString().Split('.').ToList().Last();
					minfo.Parameters.Add(pinfo);
				}

				info.Methods.Add(minfo);
			}

			return info;
		}
    }
}
