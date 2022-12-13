using NSharp;
using System.Reflection;

namespace NGen;

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

        controller.Name(Page.Name);

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
        React.AddRouteToAppJs(Page.Name, NGen.Page.GetRoute(Page));

        #endregion
    }


    public static string GetRoute(System.Type type)
    {
        var attrs = type.GetCustomAttributes<NRoute>(false);
        if (attrs.None()) return type.Name;
        var attr = attrs.First();
        return attr.Text;
    }

}
