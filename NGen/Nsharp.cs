using Microsoft.EntityFrameworkCore;
using NGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NSharp
{
    public abstract class Nsharp<T> where T : class
    {
        public static async Task Run()
        {
            await React.ClearAppJsImport();
            await React.ClearAppJsRoutes();
            React.ClearModulesFolder();

            React.ClearPagesDirectory();
            Controller.CleanPagesDirectory();

            await GetEnumerableOfType<NPag, T>();

            await GetEnumerableOfTypeTwo<MenuModule, T>();

            GetEnumerableOfTypeThree<Page, T>();



            var assembliesFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);

            foreach (var file in assembliesFiles.Where(c => c.EndsWith(".dll")))
            {
                try
                {
                    var assemblies = Assembly.LoadFrom(file);
                    var types = assemblies.GetExportedTypes()
                                 .Where(c => c.IsClass && !c.IsAbstract && c.IsPublic && typeof(IIndependentModule).IsAssignableFrom(c));

                    foreach (var type in types)
                    {
                        var independentModule = (IIndependentModule)Activator.CreateInstance(type);
                        var controller = independentModule.Controller();

                        if (controller.name.HasValue())
                            NCommon.WebSiteBasePath.SubDirectory("Controllers").SubDirectory("Pages").EnsureExsit().SubDirectory("IndependentPages").EnsureExsit().WriteFile(independentModule.Controller().name, independentModule.Controller().content);



                    }
                }
                catch (Exception)
                {

                }

            }

        }

        public static async Task GetEnumerableOfType<T, TGetAssembly>() where T : NCommon
        {
            foreach (Type type in Assembly.GetAssembly(typeof(TGetAssembly))
                .GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                await ((T)Activator.CreateInstance(type)).Dispose(type.Name);
            }
        }


        public static void GetEnumerableOfTypeThree<T, TGetAssembly>() where T : Page
        {
            foreach (Type type in Assembly.GetAssembly(typeof(TGetAssembly))
                .GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                ((T)Activator.CreateInstance(type)).Buid(type);
            }
        }


        public static async Task GetEnumerableOfTypeTwo<T, TGetAssembly>() where T : MenuModule
        {
            foreach (Type type in Assembly.GetAssembly(typeof(TGetAssembly))
                .GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                await ((T)Activator.CreateInstance(type)).Dispose();
            }
        }
    }
}
