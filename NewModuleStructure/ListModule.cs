using NSharp;
using RepoDb;
using System.Linq;
using System.Linq.Expressions;

namespace NGen
{
    public abstract class ListModule<T> : AModule where T : class
    {
        public override string GetViewModel(Type pageType, Type moduleType)
        {
            return "\t" + IListItem.RenderClass(Columns, ViewModelName(pageType, moduleType) + ":NViewModel");
        }

        public override string GetActions(Type pageType, Type moduleType)
        {
            this.Actions.Add(("LoadSource", @$"        [HttpPost]
        [Route(""[action]"")]
        public async Task<IActionResult> {moduleType.Name + "GetSource"}()
        {{
             var rows = await Database.GetList<{typeof(T).FullName}>();
             return Ok(rows.Select(c => (new {this.ViewModelName(pageType, moduleType)}()).MapFrom(c)));
        }}"));

            return base.GetActions(pageType, moduleType);

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
  {{data && Object.keys(data).length > 0 ?
    (<table class=""table table-bordered"">
      {{console.log(data)}}
        <thead>
          <tr>
            {Columns.Select(c => $"<th scope=\"col\">{c.HeaderName()}</th>").Join("\n\t\t\t\t\t\t")}
          </tr>
        </thead>
        <tbody>
      {{data.map((k , i)=> <tr key={{i}}>
            {Columns.Where(c => c is ListColumn).Select(c => $"<td>{{k.{c.Name().FirstCharToLower()}}}</td>").Join("\n\t\t\t\t\t\t")}
          </tr> )}}
        </tbody>
      </table>): null}}";

        }

        public override string GetReactImports(Type pageType, Type moduleType)
        {
            ReactImport.Add(("GetSource", "import { NPost } from '../../../Tools/Extentions';"));

            return ReactImport.GroupBy(c => c.Import).Select(c => "//" + c.Select(z => z.Name).Join(',') + "\n" + c.Key).Join('\n');
        }

        public override string GetReactBody(Type pageType, Type moduleType)
        {
            this.ReactBodys.Add(("Get source", @$"const [data , setData] = React.useState({{}});
  React.useEffect(()=>{{
          NPost(`{pageType.Name.EnsureStartWith('/')}/{moduleType.Name + "GetSource"}`)
          .then((res)=>{{
            if(res){{
              setData(res)
            }}
          }})
    }} ,[])"));

            return base.GetReactBody(pageType, moduleType);
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

        private List<IListItem> Columns = new List<IListItem>();
        public IListItem Column<U>(Expression<Func<T, U>> expression)
        {
            var column = new ListColumn(expression.MemberName(), expression.TypeName());
            Columns.Add(column);
            Props.Add((expression.TypeName(), expression.MemberName()));
            return column;
        }

        public ListItemButton ColumnButton(string name)
        {
            var column = new ListItemButton(name);
            Columns.Add(column);
            return column;
        }

        private List<ListButton> ReactButtons = new List<ListButton>();

        public ListButton Button(string name)
        {
            var button = new ListButton(name);
            ReactButtons.Add(button);
            return button;
        }

    }

    public interface IListItem
    {
        string _name { get; }

        internal virtual string HeaderName()
        {
            return _name;
        }
        internal virtual string Name()
        {
            return _name;
        }
        public static string RenderClass(IEnumerable<IListItem> columns, string Name)
        {
            var newList = columns.Where(c => c is ListColumn).Select(c => c as ListColumn);
            return @$"public class {Name}
    {{
{"\t\t" + newList.Select(c => c.renderProperty()).Join("\n\t\t")}
    }}";
        }
    }


    public class ListColumn : IListItem
    {
        internal string _type = string.Empty;

        public ListColumn(string Name, string type)
        {
            type = type.PlaceIf(type != "String", "string");
            type = type.PlaceIf(type != "Int32", "int");
            this._type = type;
            _name = Name;
        }

        public string _name { get; private set; }

        internal string renderProperty()
        {
            return $"public {_type} {_name} {{ get; set; }}";
        }
    }

    public class ListItemButton : ListButton , IListItem
    {
        public ListItemButton(string Name) : base(Name)
        {
            this._name = Name;
        }
        public string _name { get; private set; }
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
            _route = Page.GetRoute(typeof(T)).EnsureStartWith('/');
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
