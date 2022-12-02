using NSharp;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Claims;

namespace NGen.Module
{
    public abstract class AModule
    {

        #region Backend
        private protected List<(string Name, string Action)> Actions = new List<(string Name, string Action)>();
        private protected List<(string Type, string Name)> Props = new List<(string Type, string Name)>();
        #endregion

        #region React
        private protected List<(string Name, string Import)> ReactImport = new List<(string Name, string Import)>();
        private protected List<(string Name, string Method)> ReactBodys = new List<(string Name, string Method)>();
        private protected string ReactHTML = string.Empty;
        private protected List<(string Name, string Method)> ReactBeforMethod = new List<(string Name, string Method)>();
        private protected List<(string Name, string html)> ReactButtonHtmls = new List<(string Name, string html)>();

        #endregion

        public abstract string GetActions(Type pageType, Type moduleType);
        public abstract string GetViewModel(Type pageType, Type moduleType);
        public abstract string GetReactImports(Type pageType, Type moduleType);

        public virtual string GetReactBody(Type pageType, Type moduleType)
        {
            return @$"{ReactBodys.GroupBy(c => c.Method).Select(c => $"\t{{/* {c.Select(c => c.Name).Join(',')} */}}\n\t{c.Key}").Join('\n')}";
        }

        public abstract string GetReactHTML(Type pageType, Type moduleType);
        public abstract string GetReactBeforMethod(Type pageType, Type moduleType);
        public abstract string GetReactPage(Type pageType, Type moduleType);
        public virtual string GetReactModuleName(Type pageType, Type moduleType) => moduleType.Name + "Module";

    }

    public abstract class ListModule<T> : AModule where T : class
    {
        public override string GetViewModel(Type pageType, Type moduleType)
        {
            return "// this is view model";
        }

        public override string GetActions(Type pageType, Type moduleType)
        {
            return "//this is action";
        }

        public override string GetReactBeforMethod(Type pageType, Type moduleType)
        {
            return "///this is beforMethod";
        }

        public override string GetReactHTML(Type pageType, Type moduleType)
        {

            return @$"  <div>
{ReactButtonHtmls.Select(c => $"\t\t{{/* {c.Name} */}}\n\t\t{c.html}").Join('\n')}
  </div>
  <table class=""table table-bordered"">
    <thead>
      <tr>
        <th scope=""col"">#</th>
        <th scope=""col"">First</th>
        <th scope=""col"">Last</th>
        <th scope=""col"">Handle</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <th scope=""row"">1</th>
        <td>Mark</td>
        <td>Otto</td>
        <td>@mdo</td>
      </tr>
      <tr>
        <th scope=""row"">2</th>
        <td>Jacob</td>
        <td>Thornton</td>
        <td>@fat</td>
      </tr>
    </tbody>
  </table> ";

        }

        public override string GetReactImports(Type pageType, Type moduleType)
        {
            return ReactImport.GroupBy(c => c.Import).Select(c => "{/*" + c.Select(z => z.Name).Join(',') + "*/}\n" + c.Key).Join('\n');
        }

        public override string GetReactPage(Type pageType, Type moduleType)
        {
            foreach (var button in ReactButtons)
            {
                if (button.ReactBody().HasValue())
                    ReactBodys.Add((button.Name(), button.ReactBody()));

                ReactButtonHtmls.Add((button.Name(), button.ReactHtml()));

                if (button.ReactImports().HasValue())
                    ReactImport.Add((button.Name(), button.ReactImports()));
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

        public void Column<U>(Expression<Func<T, U>> expression)
        {
            var column = new ListColumn(expression.MemberName(), expression.TypeName());
            this.Props.Add((expression.TypeName(), expression.MemberName()));
        }

        private List<ListButton> ReactButtons = new List<ListButton>();

        public ListButton Button(string name)
        {
            var button = new ListButton(name);
            ReactButtons.Add(button);
            return button;
        }

    }

    public class ListColumn
    {
        public ListColumn(string Name, string type)
        {

        }

    }

    public class ListButton
    {
        private string _name = string.Empty;
        public ListButton(string Name)
        {
            _name = Name;
        }

        private string _route = string.Empty;

        public void Go<T>() where T : Page
        {
            _route = NPag.GetRoute(typeof(T)).EnsureStartWith('/');
        }

        public string ReactHtml()
        {
            return $"<button type=\"button\" class=\"btn btn-primary\"{$" onClick={{()=>navigate(`{_route}`)}}".OnlyWhen(_route.HasValue())}>{_name}</button>";
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
    }

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
            this._name = name;
            this._type = type;
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
            _save= true;
            return this;
        }

    }

    public abstract class Page : NCommon
    {
        internal List<AModule> Modules = new List<AModule>();
        string _securityAttr = string.Empty;
        string _class { get; set; }


        public void Add<T>() where T : AModule => Modules.Add(Activator.CreateInstance<T>());

        public void Buid(Type Page)
        {

            #region Controller

            var template = TemplatesDirectory.ReadFile("Controller.cs");
            if (_securityAttr.HasValue())
                template = template.InsertBefor("public class", _securityAttr + "\n\t");
            var controller = template.AsController();

            controller.Name(Page.Name + "Controller");

            foreach (var module in Modules)
            {
                controller.AddAction(module.GetActions(Page, module.GetType()));
                controller.AddViewModle(module.GetViewModel(Page, module.GetType()));
            }

            WebSiteBasePath.SubDirectory("Controllers").SubDirectory("Pages").WriteFile($"{Page.Name}Controller.cs", controller.ToString());
            #endregion

            #region React

            var reactPageTemplate = TemplatesDirectory.ReadFile("ReactPage.js");
            var reactPageFile = reactPageTemplate.AsReactFile().Name(Page.Name + "Page").PageClass(_class);

            var ReactworkingDirextory = ReactPagesBaseDirectory.SubDirectory(Page.Name).EnsureExsit().CleanDirectory();
            ReactworkingDirextory.SubDirectory("Modules").EnsureExsit().CleanDirectory();

            foreach (var item in Modules)
            {
                ReactworkingDirextory.WriteFile(item.GetReactModuleName(Page, item.GetType()) + ".js", item.GetReactPage(Page, item.GetType()));
                reactPageFile.ImportModule(item.GetReactModuleName(Page, item.GetType())).AddModuleToBody(item.GetReactModuleName(Page, item.GetType()));
            }
            ReactPagesBaseDirectory.SubDirectory(Page.Name).WriteFile($"{Page.Name}Page.js", reactPageFile.ToString());
            React.AddPageImportToAppJs(Page.Name);
            React.AddRouteToAppJs(Page.Name, NPag.GetRoute(Page));

            #endregion
        }
    }

}
