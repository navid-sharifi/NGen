using NGen.NewModuleStructure;
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
            await  React.ClearAppJsImport();
            await  React.ClearAppJsRoutes();
            React.ClearModulesFolder();

            React.ClearPagesDirectory();
            Controller.CleanPagesDirectory();

            await GetEnumerableOfType<NPag, T>();

            await GetEnumerableOfTypeTwo<MenuModule, T>();

             GetEnumerableOfTypeThree<Page, T>();


            //await GetEnumerableOfType<NModel, T>();

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
