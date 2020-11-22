using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OracRTD
{
    public class Curve
    {
        public String name;
        public int updateCount = 0;
        public object[,] array = new object[1, 1];
        public Dictionary<string, NameValue> map = new Dictionary<string, NameValue>();

        public Curve(string name)
        {
            this.name = name;
        }
    }
}
