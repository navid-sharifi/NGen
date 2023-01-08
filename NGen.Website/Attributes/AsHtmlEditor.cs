using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGen
{

	[AttributeUsage(AttributeTargets.Property, Inherited = false)]
	public class AsHtmlEditor : Attribute
	{
		public AsHtmlEditor()
		{

		}

	}
}
