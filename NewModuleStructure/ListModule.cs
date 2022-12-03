using NSharp;
using System.Linq.Expressions;

namespace NGen.NewModuleStructure
{
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
            Props.Add((expression.TypeName(), expression.MemberName()));
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
        {}
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

}
