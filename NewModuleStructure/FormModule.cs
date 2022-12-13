using Microsoft.Identity.Client;
using NSharp;
using System.Linq.Expressions;
using System.Security.AccessControl;

namespace NGen
{
    public class FormModule<T> : AModule where T : class
    {
        public override string GetActions(Type pageType, Type moduleType)
        {

            foreach (var button in ReactButtons)
            {
                if (button.Actions(pageType, moduleType, typeof(T), ViewModelName(pageType, moduleType)).HasValue())
                    Actions.Add((button.Name(), button.Actions(pageType, moduleType, typeof(T), ViewModelName(pageType, moduleType))));
            }

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
            return ReactImport.GroupBy(c => c.Import).Select(c => "//" + c.Select(z => z.Name).Join(',') + "\n" + c.Key).Join('\n');
        }


        public override string GetReactPage(Type pageType, Type moduleType)
        {

            foreach (var item in Fields)
            {
                ReactHTML += item.ReactHtml() + '\n';
            }

            foreach (var button in ReactButtons)
            {
                if (button.ReactBody(pageType, moduleType).Any())
                    ReactBodys.AddRange(button.ReactBody(pageType, moduleType).Select(c => (button.Name(), c)));

                if (button.ReactImports().Any())
                    ReactImport.AddRange(button.ReactImports().Select(c => (button.Name(), c)));

                ReactHTML += $"\t{{/* {button.Name()} */}}\n\t{button.ReactHtml()}\n";

                //ReactButtonHtmls.Add((button.Name(), button.ReactHtml()));
            }


            return @$"
import React from 'react';
{GetReactImports(pageType, moduleType)}

{GetReactBeforMethod(pageType, moduleType)}

const {GetReactModuleName(pageType, moduleType)} = () => {{
    
    const [formState, SetForm] = React.useState({{}});

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
            var field = new FormModuleField(expression.MemberName(), expression.TypeName());
            Fields.Add(field);
            return field;
        }

        private List<FormModuleButton> ReactButtons = new List<FormModuleButton>();

        public FormModuleButton Button(string name)
        {
            var button = new FormModuleButton(name);
            ReactButtons.Add(button);
            return button;
        }
    }

    public class FormModuleField
    {
        private string _name = string.Empty;
        private string _type = string.Empty;
        public FormModuleField(string name, string type)
        {

            type = type.PlaceIf(type != "String", "string");
            type = type.PlaceIf(type != "Int32", "int");


            _name = name;
            _type = type;
        }

        internal string renderProperty()
        {
            return $"public {_type} {_name} {{ get; set; }}";
        }


        internal string ReactHtml()
        {
            if (_type == "string")
                return @$"    <div class=""form-group"">
        <label>{_name}</label>
        <input  
            onChange={{(e) => SetForm( (f) =>{{return {{...f, ['{_name.FirstCharToLower()}']: e.target.value}}; }}) }}
            class=""form-control"" placeholder=""{_name}"" />
    </div>";

            //////////////////////////////////////////////

            if (_type == "int")
                return @$"    <div class=""form-group"">
        <label>{_name}</label>
        <input
        onChange={{(e) => SetForm( (f) =>{{return {{...f, ['{_name.FirstCharToLower()}']: e.target.value}}; }}) }}
        type=""number"" class=""form-control"" placeholder=""{_name}"" />
    </div>";

            //////////////////////////////////////////////
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

    public class FormModuleButton
    {
        private string _name = string.Empty;
        private string _route = string.Empty;
        private bool _save;

        public FormModuleButton(string Name)
        {
            _name = Name;
        }

        public void Go<T>() where T : Page
        {
            _route = Page.GetRoute(typeof(T)).EnsureStartWith('/');
        }

        public string ReactHtml()
        {
            return $"<button type=\"button\" class=\"btn btn-primary\"{$" onClick={{()=>navigate(`{_route}`)}}".OnlyWhen(_route.HasValue() && !_save)} " +
                $"{$"onClick={{(e)=> {Name()}(e)}}".OnlyWhen(_save)} >{_name}</button>";
        }

        public List<string> ReactBody(Type pageType, Type moduleType)
        {
            var list = new List<string>();

            if (_route.HasValue() || _save)
            {
                list.Add("let navigate = useNavigate();");
            }

            if (_save)
            {
                list.Add($@"const {_name} = (event) => {{
        NPostData('/{pageType.Name}/{moduleType.Name + _name}', formState)
            .then(function (response) {{
                {$"navigate('{_route}');".OnlyWhen(_route.HasValue())}
            }})
    }}");
            }

            return list;
        }

        public string ReactBeforMethod()
        {
            return $"<button type=\"button\" class=\"btn btn-primary\">{_name}</button>";
        }

        public List<string> ReactImports()
        {
            var list = new List<string>();

            if (_save)
            {
                list.Add("import { NPostData } from '../../../Tools/Extentions';");
            }

            if (_route.HasValue())
            {
                list.Add("import { useNavigate } from \"react-router-dom\";");
            }

            return list;
        }

        public string Actions(Type pageType, Type moduleType, Type entity, string viewModel)
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
                var item = await Database.FirstOrDefaultAsync<{entity.FullName}>(c => c.Id == data.Id);
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

            return action;
        }


        internal string Name()
        {
            return _name;
        }

        public FormModuleButton Save()
        {
            _save = true;
            return this;
        }

    }

}
