using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSharp
{
    public class React
    {
        public static async Task ClearAppJsImport()
        {
            var newAppJs = (await NCommon.ReactAppJs.ReadFileAsync()).RemoveBetween("/////");
            await NCommon.ReactAppJs.WriteFileAsync(newAppJs);
        }

        public static async Task ClearAppJsRoutes()
        {
            var newAppJs = (await NCommon.ReactAppJs.ReadFileAsync()).RemoveBetweens("<BrowserRouter>", "</BrowserRouter>").InsertAfter("<BrowserRouter>" , "<Routes></Routes>");
            await NCommon.ReactAppJs.WriteFileAsync(newAppJs);
        }

        public static void ClearModulesFolder()
        {
             NCommon.ReactPagesBaseDirectory.SubDirectory("Modules").EnsureExsit().CleanDirectory();
        }


        public static void ClearPagesDirectory()
        {
            NCommon.ReactPagesBaseDirectory.EnsureExsit().CleanDirectory();
        }

        public static void AddPageImportToAppJs(string objectName)
        {
            var ddd = NCommon.ReactAppJs.ReadFile().InsertAfter("/////", $"\n import {objectName}Page from './Pages/{objectName}/{objectName}Page'; \n");
            NCommon.ReactAppJs.WriteFile(ddd);
        }


        public static async Task ImportModule(string moduleName)
        {
            var ddd = (await NCommon.ReactAppJs.ReadFileAsync()).InsertAfter("/////", $"\n import {moduleName} from './Pages/Modules/{moduleName}'; \n");
            await NCommon.ReactAppJs.WriteFileAsync(ddd);
        }

        public static void AddRouteToAppJs(string objectName, string path = "")
        {
            if (string.IsNullOrEmpty(path))
                path = objectName;

            var ddd =  NCommon.ReactAppJs.ReadFile().InsertBefor("</Routes></BrowserRouter>", $"\n\t\t\t\t <Route path=\"/{path}\" element={{<div class='col'><div class='content'> <{objectName}Page /></div></div>}}/> \n \t\t\t");
            NCommon.ReactAppJs.WriteFile(ddd);
        }

        public static async Task AddMenuRouteToAppJs(string objectName, string path , string module)
        {
            if (string.IsNullOrEmpty(path))
                path = objectName;

            var react = await NCommon.ReactAppJs.ReadFileAsync();

            var route = $"\n\t\t\t\t <Routes><Route path=\"/{path}\" element={{<div class='col-md-3 w-240'><{module} /></div>}}/></Routes> \n \t\t\t";

            if (react.Contains(route))
                return;

            var ddd = react.InsertAfter("<BrowserRouter>", route);
            await NCommon.ReactAppJs.WriteFileAsync(ddd);
        }
    }

    public class ReactFile
    {
        public string Lines { get; set; }
        private string pageName = string.Empty;
        private string body = string.Empty;
        private string @Class = string.Empty;

        public ReactFile PageClass (string @class)
        {
            @Class = @class;
            return this;
        }

        public ReactFile(string lines)
        {
            Lines = lines;
        }
        public ReactFile AddModuleToBody(string moduleName)
        {
            body += $"<{moduleName}/> ";
            return this;
        }

        
        public ReactFile Body(string html)
        {
            body += html;
            return this;
        }

        public ReactFile Name(string objectName)
        {
            pageName = objectName;
            return this;
        }
        public ReactFile ImportModule(string moduleName)
        {
            Lines = $"import {moduleName} from './Modules/{moduleName}' \n" + Lines;
            return this;
        }

        public ReactFile Import(string Name, string Path)
        {
            Lines = $"import {Name} from '{Path}'; \n" + Lines;
            return this;
        }
        public ReactFile AddToMethodBody(string html)
        {
            Lines =  Lines.InsertBefor("return" , html + '\n');
            return this;
        }
        
        public ReactFile ImportCss(string path)
        {
            Lines = $"import '{path}'; \n" + Lines;
            return this;
        }

        public string ToString() => Lines.Replace("[#PageName#]", $"{pageName}").Replace("[#BODY#]", $"<div {("class=" + "'" + @Class + "'").OnlyWhen(@Class.HasValue())}>" + body + "</div>");
    }

}
