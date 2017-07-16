using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsBridgeTest
{
    public class TestClass
    {
        public string Property = "";
        public bool RunMethod(Int64 param1, bool param2)
        {
            Console.WriteLine(param1.ToString() + param2.ToString());
            return param1 == 100;
        }

        public int Read()
        {
            return 7;
        }

        public int ReadZero()
        {
            return 0;
        }

        public bool ReadFalse()
        {
            return false;
        }
    }
}
