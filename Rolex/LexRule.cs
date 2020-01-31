using QUT.Gplex.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rolex
{
	// used to hold the results of reading the input document
	class LexRule
	{
		public int Id;
		public string Symbol;
		public KeyValuePair<string, object>[] Attributes;
		public RuleDesc Desc;
		public int Line;
		public int Column;
		public long Position;
		public object GetAttr(string name, object @default = null)
		{
			var attrs = Attributes;
			if (null != attrs)
			{
				for (var i = 0; i < attrs.Length; i++)
				{
					var attr = attrs[i];
					if (0 == string.Compare(attr.Key, name))
						return attr.Value;
				}
			}
			return @default;
		}
	}
}
