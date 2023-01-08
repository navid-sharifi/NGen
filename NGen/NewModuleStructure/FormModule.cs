using AutoMapper.Execution;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using NSharp;
using RepoDb.Extensions;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.AccessControl;

namespace NGen
{
	public class FormModule<T> : AModule where T : class
	{
		public override string GetActions(Type pageType, Type moduleType)
		{

			foreach (var button in ReactButtons)
			{
				if (button.Actions(pageType, moduleType, ViewModelName(pageType, moduleType)).HasValue())
					Actions.Add((button.Name(), button.Actions(pageType, moduleType, ViewModelName(pageType, moduleType))));
			}

			foreach (var Field in this.Fields)
			{
				if (Field.Action(pageType, moduleType, ViewModelName(pageType, moduleType)).HasValue())
					Actions.Add((Field._name, Field.Action(pageType, moduleType, ViewModelName(pageType, moduleType))));
			}

			Actions.Add(("data source", $@"

        [HttpPost]
        [Route(""[action]/{{Id?}}"")]
        public async Task<IActionResult> {moduleType.Name}Source({"System.Guid"}? {"Id"})
        {{
            var rows =  await Database.GetListAsync<{typeof(T).FullName}>(c=>c.{"Id"} == {"Id"});
            return Ok(rows.FirstOrDefault());
        }}"));




			return Actions.Select(c => "//" + c.Name + "\n" + c.Action).Join('\n');
		}

		public override string GetReactBeforMethod(Type pageType, Type moduleType)
		{
			return "";
		}

		public override string GetReactHTML(Type pageType, Type moduleType)
		{
			return ReactHTML;
		}

		public override string GetReactImports(Type pageType, Type moduleType)
		{
			this.ReactImport.Add(("", "import { GetParam } from '../../../Tools/Url';"));
			this.ReactImport.Add(("", "import { NPost } from '../../../Tools/Extentions';"));
			this.ReactImport.Add(("", "import JoditEditor from 'jodit-react';"));

			return ReactImport.GroupBy(c => c.Import).Select(c => "//" + c.Select(z => z.Name).Join(',') + "\n" + c.Key).Join('\n');
		}

		public override string GetReactBody(Type pageType, Type moduleType)
		{
			ReactBodys.Add(("load data", $@"
    React.useEffect(()=>{{
        if(GetParam('item')){{
          NPost(`{pageType.Name.EnsureStartWith('/')}/{moduleType.Name}Source/${{GetParam('item')}}`)
          .then((res)=>{{
            if(res){{
              SetForm(res)
            }}
          }})
        }}
    }} ,[]);
"));


			foreach (var Field in Fields)
			{
				if (Field.ReactBody(pageType, moduleType).HasValue())
					ReactBodys.Add((Field._name, Field.ReactBody(pageType, moduleType)));
			}



			return base.GetReactBody(pageType, moduleType);
		}

		public override string GetReactPage(Type pageType, Type moduleType)
		{
			foreach (var item in Fields)
			{
				ReactHTML += item.ReactHtml(pageType, moduleType, ViewModelName(pageType, moduleType)) + '\n';
			}

			foreach (var button in ReactButtons)
			{
				if (button.ReactBody(pageType, moduleType).Any())
					ReactBodys.AddRange(button.ReactBody(pageType, moduleType).Select(c => (button.Name(), c)));

				if (button.ReactImports().Any())
					ReactImport.AddRange(button.ReactImports().Select(c => (button.Name(), c)));

				ReactHTML += $"\t{{/* {button.Name()} */}}\n\t{button.ReactHtml()}\n";

			}


			return @$"
import React from 'react';
{GetReactImports(pageType, moduleType)}

{GetReactBeforMethod(pageType, moduleType)}

const {GetReactModuleName(pageType, moduleType)} = () => {{
    
    const [form, SetForm] = React.useState({{}});

{GetReactBody(pageType, moduleType)}

    return (
<div>
{GetReactHTML(pageType, moduleType)} 
</div>);
}}

export default {GetReactModuleName(pageType, moduleType)};";

		}

		public override string GetViewModel(Type pageType, Type moduleType)
		{
			return Fields.RenderClass(ViewModelName(pageType, moduleType) + ":NViewModel");
		}


		private List<FormModuleField> Fields = new List<FormModuleField>();

		public FormModuleField Field<U>(Expression<Func<T, U>> expression)
		{
			var attrs = (expression.Body as MemberExpression).Member.CustomAttributes;
			var field = new FormModuleField(expression.MemberName(), expression.TypeName(), expression.ReturnType, attrs);
			Fields.Add(field);
			return field;
		}

		private List<FormModuleButton> ReactButtons = new List<FormModuleButton>();

		public FormModuleButton Button(string name)
		{
			var button = new FormModuleButton(name, typeof(T));
			ReactButtons.Add(button);
			return button;
		}
	}

	public class FormModuleField
	{
		internal string _name = string.Empty;
		private string _type = string.Empty;
		Type PropertyType;
		IEnumerable<CustomAttributeData> Attributes;

		public FormModuleField(string name, string type, Type propertyType, IEnumerable<CustomAttributeData> attributes)
		{
			Attributes = attributes;
			PropertyType = propertyType;
			type = type.PlaceIf(type != "String", "string");
			type = type.PlaceIf(type != "Int32", "int");

			_name = name;
			_type = type;
		}

		internal string renderProperty()
		{
			if (PropertyType?.BaseType?.Name == nameof(BaseEntity))
			{
				return $"public System.Guid {_name}Id {{ get; set; }}";
			}
			return $"public {_type} {_name} {{ get; set; }}";
		}

		internal string ReactHtml(Type pageType, Type moduleType, string viewModel)
		{

			if(Attributes.Any(c => c.AttributeType.FullName == typeof(AsHtmlEditor).FullName))
			{
				return $@"
<div class=""form-group"">
	<label>{_name}</label>
	<JoditEditor 
		tabIndex={{1}}
		value={{form.{_name.FirstCharToLower()}}}
		onChange={{(e) => SetForm( (f) =>{{return {{...f, ['{_name.FirstCharToLower()}']: e}}; }}) }}
/>
</div>
";
			}


			if (_type == "string")
				return @$"    <div class=""form-group"">
        <label>{_name}</label>
        <input  
            onChange={{(e) => SetForm( (f) =>{{return {{...f, ['{_name.FirstCharToLower()}']: e.target.value}}; }}) }}
            value={{form.{_name.FirstCharToLower()}}}
            class=""form-control"" placeholder=""{_name}"" />
    </div>";

			//////////////////////////////////////////////

			if (_type == "int")
				return @$"    <div class=""form-group"">
        <label>{_name}</label>
        <input
          value={{form.{_name.FirstCharToLower()}}}
          onChange={{(e) => SetForm( (f) =>{{return {{...f, ['{_name.FirstCharToLower()}']: e.target.value}}; }}) }}
          type=""number"" class=""form-control"" placeholder=""{_name}"" />
    </div>";

			//////////////////////////////////////////////

			if (PropertyType?.BaseType?.Name == nameof(BaseEntity))
			{
				return @$"    <div class=""form-group"">
        <label>{_name}</label>
            <select class=""form-select form-control"" aria-label=""Default select example"" onChange={{(e) => SetForm( (f) =>{{return {{...f, ['{(_name + "Id").FirstCharToLower()}']: e.target.value}}; }}) }} >
              <option >Open this select menu</option>
                {{{_name}Source && {_name}Source.length > 0 ?
                    {_name}Source.map((k,i)=> {{
                                            if (form.{(_name + "Id").FirstCharToLower()} == k.value) {{
                                                return <option key={{i}} value={{k.value}} selected >{{k.name}}</option>
                                              }} else {{
                                                return <option key={{i}} value={{k.value}} >{{k.name}}</option>
                                              }}
                                            }}) 
                    : null}}
            </select>
    </div>";
			}

			return "";
		}

		internal string Action(Type pageType, Type moduleType, string viewModel)
		{
			
			if (PropertyType?.BaseType?.Name == nameof(BaseEntity))
			{
				return $@"
        [HttpPost]
        [Route(""[action]"")]
        public async Task<IActionResult> {moduleType.Name}{_name}Source()
        {{
            var rows =  await Database.Of<{PropertyType.FullName}>().GetListAsync();
            return Ok(rows.Select(c => new {{value = c.Id,name = c.ToString()}}));
        }}";
			}

			return "";
		}

		internal string ReactBody(Type pageType, Type moduleType)
		{
			if (PropertyType?.BaseType?.Name == nameof(BaseEntity))
			{
				return $@"
    const [{_name}Source, Set{_name}Source] = React.useState({{}});
    React.useEffect(()=>{{
         NPost(`/{pageType.Name}/{moduleType.Name}{_name}Source`)
         .then((res)=>{{
           if(res){{
            
             Set{_name}Source(res);
             console.log(res);
           }}
         }})
    }} ,[]);";
			}
			return "";
		}

	}
	public static partial class Extentions
	{
		public static string RenderClass(this List<FormModuleField> @this, string Name)
		{
			return @$"public class {Name}
    {{
        {@this.Select(c => c.renderProperty()).Join('\n').Replace("\n", "\n\t")}
    }}";
		}
	}

	public class FormModuleButton : IFormModuleButtonSave
	{
		private string _name = string.Empty;
		private string _route = string.Empty;
		private bool _save;
		private bool _onClick;
		internal Type entity;
		private bool _visibleIfNotNew;
		internal string _class = string.Empty;


		internal List<string> _onClickActionCodes = new List<string>();
		internal List<string> _reactImports = new List<string>();

		public FormModuleButton(string Name, Type entity)
		{
			_name = Name;
			this.entity = entity;
		}

		public void Go<T>() where T : Page
		{
			_route = Page.GetRoute(typeof(T)).EnsureStartWith('/');
		}

		internal string ReactHtml()
		{
			var html = string.Empty;

			if (_save)
			{
				html += $"<button type=\"button\" class=\"btn btn-{_class.PlaceIf(_class.None(), "primary")}\" " +
				$"{$"onClick={{(e)=> {Name()}(e)}}"}>{_name}</button>";
			}
			else if (_onClick)
			{
				html += $"<button type=\"button\" class=\"btn btn-{_class.PlaceIf(_class.None(), "primary")}\" " +
				$"{$"onClick={{(e)=> {Name()}(e)}}"}>{_name}</button>";
			}
			else if (_route.HasValue())
			{
				html += $"<button type=\"button\" class=\"btn btn-{_class.PlaceIf(_class.None(), "primary")}\"{$" onClick={{()=>navigate(`{_route}`)}}"} " +
			   $">{_name}</button>";
			}

			return "{form && form.id ?".OnlyWhen(_visibleIfNotNew) + html + ": null }".OnlyWhen(_visibleIfNotNew);

		}

		internal List<string> ReactBody(Type pageType, Type moduleType)
		{
			var list = new List<string>();

			if (_route.HasValue() || _save)
			{
				list.Add("let navigate = useNavigate();");
			}

			if (_save)
			{
				list.Add($@"const {_name} = (event) => {{
        NPostData('/{pageType.Name}/{moduleType.Name + _name}', form)
            .then(function (response) {{
                {$"navigate('{_route}');".OnlyWhen(_route.HasValue())}
            }})
    }}");
			}

			if (_onClick)
			{
				list.Add($@"const {_name} = (event) => {{
        NPostData('/{pageType.Name}/{moduleType.Name + _name}', form)
            .then(function (response) {{
                
            }})
    }}");
			}

			return list;
		}

		internal string ReactBeforMethod()
		{
			return $"<button type=\"button\" class=\"btn btn-primary\">{_name}</button>";
		}

		internal List<string> ReactImports()
		{
			if (_save || _onClick)
			{
				_reactImports.Add("import { NPostData } from '../../../Tools/Extentions';");
			}

			if (_route.HasValue())
			{
				_reactImports.Add("import { useNavigate } from \"react-router-dom\";");
			}

			return _reactImports;
		}

		internal string Actions(Type pageType, Type moduleType, string viewModel)
		{
			string action = string.Empty;

			if (_save)
			{
				action += @$"
        [HttpPost]
        [Route(""[action]"")]
        public async Task<IActionResult> {moduleType.Name + Name()}({viewModel} data)
        {{

            if (data.Id.HasValue())
            {{
                var item = await Database.Of<{entity.FullName}>().FirstOrDefaultAsync(c => c.Id == data.Id);
                if (item == null) return NotFound();
                item = item?.UpdateFrom(data);
                await Database.UpdateAsync(item);
                return Ok();
            }}

            await Database.InsertAsync((new {entity.FullName}()).MapFrom(data));
            return Ok();
        }}
            ";
			}

			if (_onClick)
			{
				action += @$"
        [HttpPost]
        [Route(""[action]"")]
        public async Task<IActionResult> {moduleType.Name + Name()}({viewModel} data)
        {{
            {_onClickActionCodes.Join('\n').Replace("\n", "\n\t\t\t")}
            return Ok();
        }}
            ";
			}


			return action;
		}

		public void OnClick(Action<FormModuleButtonOnClick> actions)
		{
			var OnClickObj = new FormModuleButtonOnClick(this);

			actions.Invoke(OnClickObj);

			if (OnClickObj._onClick_HaveCodeBlock)
				_onClickActionCodes.Add("}");
			_onClick = true;
		}

		internal string Name()
		{
			return _name;
		}

		public FormModuleButton AddClass(string @class)
		{
			_class = @class;
			return this;
		}

		public IFormModuleButtonSave Save()
		{
			_save = true;
			return this;
		}

		public FormModuleButton VisibleIfNotNew()
		{
			_visibleIfNotNew = true;
			return this;

		}

	}

	public interface IFormModuleButtonSave
	{
		IFormModuleButtonSave Save();
		void Go<T>() where T : Page;
	}

	public class FormModuleButtonOnClick : IFormModuleButtonOnClickIf
	{
		internal bool _onClick_HaveCodeBlock;
		private FormModuleButton Button;

		public FormModuleButtonOnClick(FormModuleButton FormModuleButton)
		{
			Button = FormModuleButton;
		}
		public IFormModuleButtonOnClickStepTwo Csharp(string code)
		{
			Button._onClickActionCodes.Add("\t".OnlyWhen(_onClick_HaveCodeBlock) + code);
			return this;
		}

		public IFormModuleButtonOnClickStepTwo Delete()
		{
			Button._onClickActionCodes.Add("\t".OnlyWhen(_onClick_HaveCodeBlock) + $"await Database.DeleteAsync<{Button.entity.FullName}>(c => c.Id == data.Id);");
			return this;
		}

		public IFormModuleButtonOnClickStepTwo Go<T>() where T : Page
		{
			var route = Page.GetRoute(typeof(T)).EnsureStartWith('/');
			Button._onClickActionCodes.Add("\t".OnlyWhen(_onClick_HaveCodeBlock) + $"return Ok(new {{redirect = \"{route}\"}});");
			return this;
		}
		public void EndIf()
		{
			Button._onClickActionCodes.Add("}");
			_onClick_HaveCodeBlock = false;
		}

		public IFormModuleButtonOnClickStepTwo If(string condition)
		{
			_onClick_HaveCodeBlock = true;
			Button._onClickActionCodes.Add($"if({condition}){{");
			return this;
		}
		public IFormModuleButtonOnClickStepTwo ReturnToastMessage(string message)
		{
			Button._onClickActionCodes.Add("\t".OnlyWhen(_onClick_HaveCodeBlock) + $"return Ok(\"{message}\");");
			return this;
		}
	}

	public interface IFormModuleButtonOnClickIf : IFormModuleButtonOnClickStepTwo
	{
		IFormModuleButtonOnClickStepTwo If(string condition);
		void EndIf();

	}

	public interface IFormModuleButtonOnClickStepTwo
	{
		IFormModuleButtonOnClickStepTwo Csharp(string code);
		IFormModuleButtonOnClickStepTwo ReturnToastMessage(string code);
		IFormModuleButtonOnClickStepTwo Go<T>() where T : Page;
	}
}
