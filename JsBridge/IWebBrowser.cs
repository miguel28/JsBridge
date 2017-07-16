using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsBridge
{
    public interface IWebBrowser
    {
        void ExecuteJavaScript(string code);
        bool PageIsLoaded();
    }
}
