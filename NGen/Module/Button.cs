using NGen;
using System.Linq.Expressions;

namespace NSharp
{
    public class Button
    {
        internal string _Header
        {
            get { return header.HasValue() ? header : _Name; }
            set { header = value; }
        }

        private string header;

        public Button Header(string header)
        {
            _Header = header;
            return this;
        }

        private string _Name { get; set; }
        private string _displayName { get; set; }

        protected string _Send { get; set; }

        private string _Go { get; set; }

        private bool _Save = false;

        private string _csharp;

        public Button CSharp(string codes)
        {
            _csharp = codes;
            return this;
        }
        public Button Lable(string lable)
        {
            _displayName = lable;
            return this;
        }


        public Button(string name)
        {
            _Name = name;
        }

        public virtual Button Go<TPage>() where TPage : NPag, new()
        {
            _Go = Page.GetRoute(typeof(TPage));

            if (_Go.None())
                _Go = typeof(TPage).Name;
            _Go = _Go.EnsureStartWith('/');

            return this;
        }

        public string ControllerAction(System.Type moduleType, System.Type pageType, string viewModelName, System.Type entityType)
        {
            if (_Save)
            {
                return @$"
        [HttpPost]
        [Route(""[action]"")]
        public async Task<IActionResult> {ControllerActionName(moduleType, pageType)}({viewModelName} data)
        {{

            if (data.Id.HasValue())
            {{
                var item = await Database.Of<{entityType.FullName}>().FirstOrDefaultAsync(c => c.Id == data.Id);
                if (item == null) return NotFound();
                item = item?.UpdateFrom(data);
                await Database.UpdateAsync(item);
                return Ok();
            }}

            await Database.InsertAsync((new {entityType.FullName}()).MapFrom(data));
            return Ok();
        }}
            ";
            }


            if (_csharp.HasValue())
            {
                return @$"
        [HttpPost]
        [Route(""[action]"")]
        public async Task<IActionResult> {ControllerActionName(moduleType, pageType)}({viewModelName} data)
        {{
            {_csharp}
        }}
            ";
            }

            return string.Empty;
        }

        public string ControllerActionName(System.Type moduleType, System.Type pageType)
        {
            return $"{pageType.Name}{moduleType.Name}{_Name}";
        }

        public string ReactBody(System.Type moduleType, System.Type pageType)
        {
            return "let navigate = useNavigate();\n".OnlyWhen(_Go.HasValue())
            /////////////////////
            + $@"
    const {ControllerActionName(moduleType, pageType)} = (event)=>{{
        //var form = new FormData(event.target.form);
        //var data = {{}};
        //for (const pair of form.entries()) {{data[pair[0]] = pair[1];}}

         NPostData('/{pageType.Name}/{ControllerActionName(moduleType, pageType)}' , formState)
        .then(function (response) {{

            {$"navigate('{_Go}');".OnlyWhen(_Go.HasValue())}
            
         }})
    }}".OnlyWhen(_Save)

            /////////////////////
            + $@"
    const {ControllerActionName(moduleType, pageType)} = (event)=>{{

         NPostData('/{pageType.Name}/{ControllerActionName(moduleType, pageType)}' , formState)
        .then(function (response) {{

            {$"navigate('{_Go}');".OnlyWhen(_Go.HasValue())}
            
         }})
    }}".OnlyWhen(_csharp.HasValue());

  
        }

        public string React(System.Type moduleType, System.Type pageType)
        {
            var navigateUrl = _Go;

            if (_Send.HasValue())
                navigateUrl = navigateUrl.TrimEnd('/') + '?' + _Send + '=' + $"${{k.{_Send.FirstCharToLower()}}}";

            return $"<Button {$"onClick={{(e)=> {ControllerActionName(moduleType, pageType)}(e)}}".OnlyWhen(_Save || _csharp.HasValue())} {$"onClick={{()=>navigate(`{navigateUrl}`)}}".OnlyWhen(_Go.HasValue() && (!_Save && !_csharp.HasValue()))} variant=\"primary\">{_displayName.IfEmpty(_Name)}</Button>\n";
        }

        public string ReactImport()
        {
            /////// button
            return $"import Button from 'react-bootstrap/Button';\n"
                ///////navigation
                + $"{"import { useNavigate } from \"react-router-dom\";\n".OnlyWhen(_Go.HasValue())}"
                + $"{"import { NPostData } from '../../../Tools/Extentions';\n".OnlyWhen(_Save || _csharp.HasValue())}";
        }

        public Button Save()
        {
            _Save = true;
            return this;
        }


    }

    public class Button<T> : Button where T : class
    {
        public Button(string name) : base(name)
        {

        }

        public Button<T> Send<U>(Expression<Func<T, U>> item)
        {
            this._Send = item.MemberName();
            return this;
        }

        public override Button<T> Go<TPage>()
        {
            base.Go<TPage>();
            return this;
        }

    }

}
