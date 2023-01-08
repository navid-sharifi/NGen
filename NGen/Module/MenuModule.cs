using NGen;

namespace NSharp
{
    public abstract class MenuModule : BaseModule
    {

        List<(System.Type type, string displayName)> Pages = new List<(System.Type, string displayName)>();
        internal string Css = string.Empty;

        public void SetCss(string displayName)
        {
            Css = displayName;
        }

        public void Item<T>(string displayName = "") where T : NPag
        {
            if (displayName.None())
                displayName = typeof(T).Name;

            Pages.Add((typeof(T), displayName));
        }

        internal async Task Dispose()
        {
            var html = "<div><ul>\n";
            foreach (var item in Pages)
            {
                await React.AddMenuRouteToAppJs(item.type.Name, Page.GetRoute(item.type), this.GetType().Name + "Module");
                html += $"\t<li onClick={{() => navigate('{Page.GetRoute(item.type).IfEmpty('/' + item.type.Name).EnsureStartWith('/')}')}}>{item.displayName}</li>\n";
            }

            html += "</ul></div>";

            await React.ImportModule(this.GetType().Name + "Module");


            var react = (await TemplatesDirectory.ReadFileAsync("ReactPage.js"))
                .AsReactFile()
                 .ImportCss("./" + this.GetType().Name + "Module.scss")
                .Name(this.GetType().Name + "Module")
                .Body(html).Import("{ useNavigate }", "react-router-dom")
                .AddToMethodBody("let navigate = useNavigate();")
                .PageClass(this.GetType().Name)
                .ToString();

            await ReactPagesBaseDirectory.SubDirectory("Modules").EnsureExsit().WriteFileAsync(this.GetType().Name + "Module.scss", '.' + this.GetType().Name + '{' + '\n' + await TemplatesDirectory.ReadFileAsync("MenuModuleDefaultCss.scss") + '}');

            await ReactPagesBaseDirectory.SubDirectory("Modules").EnsureExsit().WriteFileAsync(this.GetType().Name + "Module.js", react);
        }
    }
}
