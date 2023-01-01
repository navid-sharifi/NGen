namespace NGen
{
	public class FileManager : AModule
	{
		public override string GetReactPage(Type pageType, Type moduleType)
		{
			return @$"
import React from 'react';


const {GetReactModuleName(pageType, moduleType)} = () => {{

    return (
<div>
<h1>im react FileManager</h1>
</div>);
}}

export default {GetReactModuleName(pageType, moduleType)};";

		}

		public override string GetViewModel(Type pageType, Type moduleType)
		{
			return "";
			throw new NotImplementedException();
		}




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

	}

}
