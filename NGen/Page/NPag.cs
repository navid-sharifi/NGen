using NGen;
namespace NSharp
{

    public abstract class NPag : NCommon
    {
        private string _name { get; set; }

        private string _SecurityAttr = String.Empty;

        internal string Class { get; set; }
        internal string MenuModule { get; set; }

        public void CheckLogin(string roles = "")
        {
            this._SecurityAttr = $"[Gate({$"roles:\"{roles}\"".OnlyWhen(roles.HasValue())})]";
        }

        public NPag CssClass(string @class)
        {
            this.Class = @class;
            return this;
        }


        public NPag AddMenuModule(string menuModule)
        {
            this.MenuModule = menuModule;
            return this;
        }


        private IList<PageModule> _modules = new List<PageModule>();

        public NPag Add<T>() where T : BaseModule
        {
            _modules.Add(new PageModule
            {
                ReactBuid = ((T)Activator.CreateInstance(typeof(T))).ReactBuid(moduleType: typeof(T), pageType: this.GetType()).Result,
                ReactFileName = typeof(T).Name + "Module",
                ControllertActions = ((T)Activator.CreateInstance(typeof(T))).ControllerActions(moduleType: typeof(T), pageType: this.GetType()).Result,
                ViewModels = ((T)Activator.CreateInstance(typeof(T))).ViewModels(moduleType: typeof(T), pageType: this.GetType()).Result,

            });
            var ddd = ((T)Activator.CreateInstance(typeof(T))).Flutter(moduleType: typeof(T), pageType: this.GetType()).Result;

            return this;
        }
        public NPag Name(string name)
        {
            this._name = name;
            Console.WriteLine(name);
            return this;
        }

        public override async Task Dispose(string ObjectName)
        {
            var template = await TemplatesDirectory.ReadFileAsync("Controller.cs");
            template = template.InsertBefor("public class", _SecurityAttr + "\n\t");
            var controller = template.AsController();



            var pageTemplate = await TemplatesDirectory.ReadFileAsync("ReactPage.js");
            var reactPageFile = pageTemplate.AsReactFile().Name(ObjectName + "Page").PageClass(Class);

            var workingDirextory = ReactPagesBaseDirectory.SubDirectory(ObjectName).EnsureExsit().CleanDirectory();

            workingDirextory.SubDirectory("Modules").EnsureExsit().CleanDirectory();

            foreach (var item in _modules)
            {
                await workingDirextory.WriteFileAsync(item.ReactFileName + ".js", item.ReactBuid);
                reactPageFile.ImportModule(item.ReactFileName).AddModuleToBody(item.ReactFileName);
                controller.AddAction(item.ControllertActions);
                controller.AddViewModle(item.ViewModels);
            }


            //////page
            await ReactPagesBaseDirectory.SubDirectory(ObjectName).WriteFileAsync($"{ObjectName}Page.js", reactPageFile.ToString());
            React.AddPageImportToAppJs(ObjectName);
            React.AddRouteToAppJs(ObjectName, Page.GetRoute(this.GetType()));

            var pagesDirectory = WebSiteBasePath.SubDirectory("Controllers").SubDirectory("Pages");
            controller.Name(ObjectName);
            await pagesDirectory.WriteFileAsync($"{ObjectName}Controller.cs", controller.ToString());

        }

    }

    public class Controller
    {
        public static void CleanPagesDirectory()
        {
            NCommon.WebSiteBasePath.SubDirectory("Controllers").SubDirectory("Pages").EnsureExsit().CleanDirectory();
        }

        public string _Lines { get; set; }
        private string _Name = string.Empty;
        private string _Actions = string.Empty;

        public Controller(string lines)
        {
            this._Lines = lines;
        }

        internal Controller Name(string name)
        {
            this._Name = name;
            return this;
        }

        public string ToString()
        {
            return _Lines.Replace("[#PAGENAME#]", _Name).Replace("[#BODY#]", _Actions);
        }

        public Controller AddAction(string actions)
        {
            _Actions += actions + '\n';
            return this;
        }

        internal Controller AddViewModle(string viewModels)
        {
            this._Lines = _Lines.InsertBeforLast("}", viewModels + '\n');
            return this;
        }
    }


    internal class PageModule
    {
        public string ReactBuid { get; set; }
        public string ReactFileName { get; set; }
        public string ControllertActions { get; set; }
        public string ViewModels { get; set; }
    }

}