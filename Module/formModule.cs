using NGen;
using System.Linq.Expressions;

namespace NSharp
{
    public abstract class formModule<T> : BaseModule where T : class
    {

        internal (string PropertyName, string PropertyType, string Param) _GetSource = (string.Empty, string.Empty, string.Empty);


        public void GetSource<U>(Expression<Func<T, U>> expression, string FromParam)
        {
            field(expression);

            _GetSource = (expression.MemberName(), expression.TypeName(), FromParam);
        }

       
        private List<formModuleField> Fields = new List<formModuleField>();

        public formModuleField field<U>(Expression<Func<T, U>> expression)
        {
            var fieldName = expression.MemberName();
            var type = expression.TypeName();

            var field = new formModuleField()
            {
                Name = fieldName,
                Type = type
            };

            Fields.Add(field);
            return field;
        }

        public override Task<string> ControllerActions(System.Type moduleType, System.Type pageType)
        {
            var actions = string.Empty;

            foreach (var button in Buttons)
                actions += button.ControllerAction(moduleType, pageType, formModuleField.GetViewModelName(moduleType, pageType), typeof(T));


            if (_GetSource.PropertyName.HasValue() && _GetSource.Param.HasValue())
            {
                actions += $@"

        [HttpPost]
        [Route(""[action]/{{{_GetSource.Param}?}}"")]
        public async Task<IActionResult> {pageType.Name}{moduleType.Name}Source({_GetSource.PropertyType}? {_GetSource.PropertyName})
        {{
            var rows =  await Database.GetListAsync<{typeof(T).FullName}>(c=>c.{_GetSource.Param} == {_GetSource.Param});
            return Ok(rows.FirstOrDefault());
        }}
                ";
            }

            return Task.FromResult(actions);
        }

        public override async Task<string> ReactBuid(System.Type moduleType, System.Type pageType)
        {
            var template = await TemplatesDirectory.ReadFileAsync("ReactFormModule.js");
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
            module = module.InsertBefor("return", "");
            module = module.InsertBeforLast("</Form>", buttonReact);

            var ReactMethodBody = string.Empty;
            var ReactHtml = string.Empty;
            var ReactImports = string.Empty;

            foreach (var field in Fields)
            {
                if (!module.Contains(field.ReactMethodBody()) && !ReactMethodBody.Contains(field.ReactMethodBody()))
                    ReactMethodBody += field.ReactMethodBody();

                if (!module.Contains(field.ReactImports()) && !ReactImports.Contains(field.ReactImports()))
                    ReactImports += field.ReactImports();

                ReactHtml += field.ReactHtml();
            }
            module = module.InsertBefor("return", ReactMethodBody);
            module = module.Insert(0, ReactImports);

            module = module.InsertAfter("<Form>", ReactHtml);

            if (_GetSource.Param.HasValue())
            {
                module = module.Insert(0, "import { NPost } from '../../../Tools/Extentions';\n");
                module = module.Insert(0, "import { GetParam } from '../../../Tools/Url';\n");

                module = module.InsertBefor("return", $@"
    React.useEffect(()=>{{

        if(GetParam('{_GetSource.Param}')){{
          NPost(`{pageType.Name.EnsureStartWith('/')}/{pageType.Name}{moduleType.Name}Source/${{GetParam('{_GetSource.Param}')}}`)
          .then((res)=>{{
            if(res){{
              SetForm(res)
            }}
          }})
        }}
    }} ,[])
");

            }



            return module;
        }

        public override Task<string> ViewModels(Type moduleType, Type pageType)
        {
            var model = formModuleField.GetViewModel(this.Fields, moduleType, pageType);
            return Task.FromResult(model);
        }

    }

    public class FlutterPage
    {
        private string Lines { get; set; }
        private string name = string.Empty;
        public FlutterPage(string lines) => this.Lines = lines;

        public FlutterPage Name(string name)
        {
            this.name = name;
            return this;
        }

        public string ToString()
        {
            return this.Lines.Replace("[#PAGENAME#]", name);
        }
    }

    public class formModuleField
    {
        public static string GetViewModel(IList<formModuleField> list, System.Type moduleType, System.Type pageType)
        {
            var props = string.Empty;

            foreach (var item in list.DistinctBy(c => c.Name).Where(c => c.Name != "Id"))
            {
                props += $"\t\t[Required]\n".OnlyWhen(!item.AllowNull);
                props += $"\t\tpublic {item.Type}{"?".OnlyWhen(item.AllowNull)} {item.Name} {{ get; set; }}" + '\n';
            }


            return "\t" + @$"public class {GetViewModelName(moduleType, pageType)}: NViewModel
    {{
{props}
    }}";
        }

        public static string GetViewModelName(System.Type moduleType, System.Type pageType)
        {
            return $"{moduleType.Name}{pageType.Name}ViewModel";
        }
        public string Name { get; init; }
        public string Type { get; init; }
        public bool AllowNull { get; set; }
        public bool IsPassword = false;

        public formModuleField NullAble()
        {
            AllowNull = true;
            return this;
        }

        public string ReactImports()
        {
            switch (Type)
            {
                case "String":
                    return "import Col from 'react-bootstrap/Col';\nimport Row from 'react-bootstrap/Row';\n";
                default:
                    break;
            }
            return "";
        }

        public string ReactMethodBody()
        {
            return "";
        }

        public string ReactHtml()
        {
            switch (Type)
            {
                case "String":
                    return @$"
                              <Form.Group as={{Row}} className=""mb-3"" controlId=""formHorizontalEmail"" >
                               <Form.Label column sm={{2}}>
                                {Name}
                               </Form.Label>
                               <Col sm={{10}}>
                                 <Form.Control
                                 onChange={{(e) => SetForm( (f) =>{{return {{...f, ['{Name.FirstCharToLower()}']: e.target.value}}; }}) }}
                                 value={{formState.{Name.FirstCharToLower()}}}
                                 type=""{"Password".PlaceIf(IsPassword, "string")}"" name=""{Name}"" placeholder =""{Name}"" />
                               </Col>
                             </Form.Group>
                                      ";

                case "Int32":
                    return @$"
                              <Form.Group as={{Row}} className=""mb-3"" controlId=""formHorizontalEmail"" >
                               <Form.Label column sm={{2}}>
                                {Name}
                               </Form.Label>
                               <Col sm={{10}}>
                                 <Form.Control
                                 onChange={{(e) => SetForm( (f) =>{{return {{...f, ['{Name.FirstCharToLower()}']: e.target.value}}; }}) }}
                                 value={{formState.{Name.FirstCharToLower()}}}
                                 type=""{"Password".PlaceIf(IsPassword, "number")}"" name=""{Name}"" placeholder =""{Name}"" />
                               </Col>
                             </Form.Group>
                                      ";


                case "Guid":
                    return @$"<input  type='hidden' name=""{Name}""  value={{formState.{Name.FirstCharToLower()}}} onChange={{(e) => SetForm( (f) =>{{return {{...f, ['{Name.FirstCharToLower()}']: e.target.value}}; }}) }}  /> ";
                default:
                    break;
            }

            return "";
        }

        public formModuleField AsPassword()
        {
            IsPassword = true;
            return this;
        }
    }

}
