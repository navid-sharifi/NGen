using RepoDb.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGen
{
	public class BoxesModule : AModule
	{
		public override string GetReactBeforMethod(Type pageType, Type moduleType)
		{
			throw new NotImplementedException();
		}

		public override string GetReactHTML(Type pageType, Type moduleType)
		{
			throw new NotImplementedException();
		}

		public override string GetReactImports(Type pageType, Type moduleType)
		{
			throw new NotImplementedException();
		}

		public override string GetReactPage(Type pageType, Type moduleType)
		{

			return @$"import React from 'react';
import {{ useNavigate }} from ""react-router-dom"";

const {GetReactModuleName(pageType, moduleType)} = () => {{
let navigate = useNavigate();

  return (
<div>
	{items.Select(c=>c.ReactHtml()).Join("\n\t")} 
</div>);
}}

export default {GetReactModuleName(pageType, moduleType)};";

		}

		public override string GetViewModel(Type pageType, Type moduleType)
		{
			return "";
		}

		List<BoxesItem> items = new List<BoxesItem>();

		public BoxesItem Item(string name)
		{
			var item = new BoxesItem(name);
			items.Add(item);
			return item;
		}
	}


	public class BoxesItem
	{
		private string _name = string.Empty;
		public BoxesItem(string Name)
		{
			_name = Name;
		}

		protected string _route = string.Empty;

		public void Go<T>() where T : Page
		{
			_route = Page.GetRoute(typeof(T)).EnsureStartWith('/');
		}

		public string ReactHtml()
		{
			return $"<button type=\"button\" class=\"btn btn-primary\"{$" onClick={{()=>navigate(`{_route}`)}}".OnlyWhen(_route.HasValue())}>{_name}</button>";
		}
		
		internal string Name()
		{
			return _name;
		}
	}

}
