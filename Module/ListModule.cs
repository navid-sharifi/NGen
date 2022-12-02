using System.Linq.Expressions;

namespace NSharp
{
    public abstract class ListModule<T> : BaseModule where T : class 
    {

        protected List<ListModuleColumn> Columns = new List<ListModuleColumn>();

        public ListModuleColumn column<U>(Expression<Func<T, U>> expression)
        {
            var columnName = expression.MemberName();
            var columnType = expression.TypeName();

            var column = new ListModuleColumn()
            {
                Name = columnName,
                Type = columnType
            };
            Columns.Add(column);
            return column;
        }

        public Button<T> columnButton(string name)
        {
            var button = new Button<T>(name);
            var Button = new ListModuleColumn()
            {
                Button = button
            };

            Columns.Add(Button);
            return button;
        }

        public override Task<string> ControllerActions(System.Type moduleType, System.Type pageType)
        {
            var action = ListModuleColumn.ControllerAction(moduleType, pageType, typeof(T));
            return Task.FromResult(action);
        }

        public override async Task<string> ReactBuid(System.Type moduleType, System.Type pageType)
        {
            var template = await TemplatesDirectory.ReadFileAsync("ReactListModule.js");
            var module = template.AsReactFile().Name(moduleType.Name + "Module").ToString();

            var buttonReact = string.Empty;
            var buttonReactImports = string.Empty;
            var buttonReactbody = string.Empty;

            foreach (var button in Buttons)
            {
                buttonReact += button.React(moduleType, pageType);
                if (!buttonReactImports.Contains(button.ReactImport()))
                    buttonReactImports += button.ReactImport() + '\n';
                if (!buttonReactbody.Contains(button.ReactBody(moduleType, pageType)))
                    buttonReactbody += button.ReactBody(moduleType, pageType) + '\n';
            }

            module = buttonReactImports + module;
            module = module.InsertBefor("return", buttonReactbody);
            module = module.InsertAfter("<>", buttonReact);
            module = module.RemoveBetweens("<thead>", " </thead>");

            module = module.InsertAfter("<thead>", ListModuleColumn.TableHeader(Columns));
            module = module.RemoveBetweens("<tbody>", " </tbody>");
            module = module.InsertAfter("<tbody>", ListModuleColumn.TableBody(Columns , moduleType, pageType));

            module = module.InsertBefor("return", ListModuleColumn.ReactBody(pageType: pageType, ModuleType: moduleType));
            module = module.Insert(0, ListModuleColumn.ReactImports());

            return module;
        }

        public override Task<string> ViewModels(Type moduleType, Type pageType)
        {
            var model = ListModuleColumn.GetViewModel(this.Columns, moduleType, pageType);
            return Task.FromResult(model);
        }
    }

    public class ListModuleColumn
    {
        public string Name { get; init; }
        public string Type { get; init; }

        public Button Button { get; init; }


        public static string ReactImports()
        {
            return "import { NPost } from './../../../Tools/Extentions';\n";
        }

        public static string ReactBody(System.Type pageType, System.Type ModuleType)
        {
            return @$"
    const [rows , setRows] = React.useState([]);
    React.useEffect(() => {{

         NPost('/{pageType.Name}/{ModuleType.Name}{pageType.Name}Source').then(function(response) {{
                    if (response) {{
                        setRows(response)
                    }}
                }})

    }}, []);" + '\n' + '\n';
        }

        public static string GetViewModel(List<ListModuleColumn> Columns, System.Type moduleType, System.Type pageType)
        {
            var props = string.Empty;

            foreach (var Column in Columns.Where(c => c.Name.HasValue()))
            {
                props += $"public {Column.Type} {Column.Name} {{ get; init; }}" + '\n';
            }

            return @$"public class {moduleType.Name}{pageType.Name}ViewModel
    {{
        {props}
    }}";
        }

        public static string ControllerAction(System.Type moduleType, System.Type pageType, System.Type entityType)
        {
            return @$"
        [HttpPost]
        [Route(""[action]"")]
        public async Task<IActionResult> {moduleType.Name}{pageType.Name}Source()
        {{
            var data =  await Database.GetList<{entityType.FullName}>();
            return Ok(data);
        }}
            ";

        }

        public static string TableHeader(List<ListModuleColumn> @this)
        {
            var header = "<tr>";

            foreach (var column in @this)
                header += $"<th>{column.Name.OnlyWhen(column.Name.HasValue())}" +
                    $"{column.Button?._Header.OnlyWhen(column.Button != null)} " +
                    $"</th>";

            header += "</tr>";
            return header;
        }

        public static string TableBody(List<ListModuleColumn> @this , System.Type moduleType, System.Type pageType , string seprator = "" )
        {
            var header = @"
           {rows && rows.length > 0 ? rows.map((k, i) => {
                return (<tr key={i}> " + '\n';

            foreach (var column in @this)
            {
                header += "\t\t\t\t\t\t\t";

                if (column.Name.HasValue())
                {
                    header += $"{"<td>".PlaceIf(seprator.None() , "")}{{k.{column.Name.FirstCharToLower()}}} {"</td>".PlaceIf(seprator.None(), seprator)}\n";
                }
                else if (column.Button != null)
                {
                    header += $"{"<td>".PlaceIf(seprator.None(), "")}\n\t\t\t\t\t\t\t\t{column.Button.React(moduleType, pageType)} \t\t\t\t\t\t\t{"</td>".PlaceIf(seprator.None(), seprator)}\n";
                }
            }

            header += "\t\t\t\t\t\t</tr>);\n\n\t\t\t\t}) : null}\n\t\t\t";


            //foreach (var column in @this)
            //    header += $" <td>{column.Name.OnlyWhen(column.Name.HasValue())}" +
            //        $"{column.Button?._Header.OnlyWhen(column.Button != null)} " +
            //        $"</td>";

            //header += "</tr>";

            return header;
        }
    }

}
