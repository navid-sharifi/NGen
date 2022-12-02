

namespace NSharp
{
    public abstract class ListRenderModule<T> : ListModule<T> where T : class
    {
        protected bool _search = false;

        public void Search()
        {
            _search = true;
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

            if (_search)
            {
                module = module.InsertAfter("<>", "\n\t<div><input onChange={(e)=>AutoSearch(e)} /></div>\n");
                module = module.InsertBefor("return", @"
    function AutoSearch(event){
        var value = event.target.value;

        if(!value)
        {
            setRows(data)
        }

        var newList = [];

        data.forEach(item => {
            var next = false;
            Object.keys(item).map((k , i)=>{

                if (String(item[k]).includes(value) && !next) {
                    next = true;
                    newList.push(item)
                }
            })
        });
         setRows(newList)

    }
");

            }

            module = buttonReactImports + module;
            module = module.InsertBefor("return", buttonReactbody);
            module = module.InsertAfter("<>", buttonReact);
            module = module.RemoveBetweens("<thead>", " </thead>");

            //module = module.InsertAfter("<thead>", ListModuleColumn.TableHeader(Columns));
            module = module.RemoveBetweens("<tbody>", " </tbody>");
            module = module.InsertAfter("<tbody>", ListModuleColumn.TableBody(Columns, moduleType, pageType, "<br/>"));



            module = module.InsertBefor("return",@$"
    const [rows , setRows] = React.useState([]);
    const [data , setData] = React.useState([]);
    React.useEffect(() => {{

         NPost('/{pageType.Name}/{moduleType.Name}{pageType.Name}Source').then(function(response) {{
                    if (response) {{
                        setRows(response);
                        setData(response);
                    }}
                }})

    }}, []);" + '\n' + '\n');


            module = module.Insert(0, "import { NPost } from './../../../Tools/Extentions';\n");
            return module;
        }
    }

}
