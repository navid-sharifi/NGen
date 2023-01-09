using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGen
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NRoute : Attribute
    {
        public NRoute(string text)
        {
            Text = text;
        }
        public string Text { get; set; }   
    }
}
