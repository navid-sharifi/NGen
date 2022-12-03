using NSharp;
using System.Linq.Expressions;

namespace NGen.NewModuleStructure
{
    public class FormModule<T> : AModule where T : class
    {
        public override string GetActions(Type pageType, Type moduleType)
        {
            return "";
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
            return ReactImport.GroupBy(c => c.Import).Select(c => "{/*" + c.Select(z => z.Name).Join(',') + "*/}\n" + c.Key).Join('\n');
        }


        public override string GetReactPage(Type pageType, Type moduleType)
        {

            foreach (var item in Fields)
            {
                ReactHTML += item.ReactHtml() + '\n';
            }

            foreach (var button in ReactButtons)
            {
                if (button.ReactBody().HasValue())
                    ReactBodys.Add((button.Name(), button.ReactBody()));

                ReactButtonHtmls.Add((button.Name(), button.ReactHtml()));

                if (button.ReactImports().HasValue())
                    ReactImport.Add((button.Name(), button.ReactImports()));

                ReactHTML += $"\t{{/* {button.Name()} */}}\n\t{button.ReactHtml()}\n";
            }


            return @$"
import React from 'react';
{GetReactImports(pageType, moduleType)}

{GetReactBeforMethod(pageType, moduleType)}

const {GetReactModuleName(pageType, moduleType)} = () => {{

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
            return "";
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
            _name = name;
            _type = type;
        }

        internal string ReactHtml()
        {
            if (_type == "String")
                return @$"    <div class=""form-group"">
        <label>{_name}</label>
        <input  class=""form-control"" placeholder=""{_name}"" />
    </div>";

            //////////////////////////////////////////////

            if (_type == "Int32")
                return @$"    <div class=""form-group"">
        <label>{_name}</label>
        <input type=""number"" class=""form-control"" placeholder=""{_name}"" />
    </div>";

            //////////////////////////////////////////////



            return "";
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
            _route = NPag.GetRoute(typeof(T)).EnsureStartWith('/');
        }

        public string ReactHtml()
        {
            return $"<button type=\"button\" class=\"btn btn-primary\"{$" onClick={{()=>navigate(`{_route}`)}}".OnlyWhen(_route.HasValue() && !_save)}>{_name}</button>";
        }

        public string ReactBody()
        {
            return $"let navigate = useNavigate();".OnlyWhen(_route.HasValue());
        }
        public string ReactBeforMethod()
        {
            return $"<button type=\"button\" class=\"btn btn-primary\">{_name}</button>";
        }

        public string ReactImports()
        {
            return $"import {{ useNavigate }} from \"react-router-dom\";".OnlyWhen(_route.HasValue());
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
