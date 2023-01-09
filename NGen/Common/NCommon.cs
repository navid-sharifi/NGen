using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NGen;

namespace NSharp
{
    public abstract class NCommon
    {
        public static NPath WebSiteBasePath
        {
            get => NPath.GetCurrentDirectory().UpDirectory().SubDirectory("Website");
        }
      
        private static NPath BaseDirectory
        {
            get => NPath.GetBaseDirectory();
        }

        protected static NPath TemplatesDirectory
        {
            get => NPath.GetBaseDirectory().SubDirectory("Templete");
        }
        protected static NPath DomainDirectory
        {
            get => NPath.GetCurrentDirectory().UpDirectory().SubDirectory("Domain");
        }
        
        protected static NPath EntitiesDirectory
        {
            get => NPath.GetCurrentDirectory().UpDirectory().SubDirectory("Domain").SubDirectory("Entities");
        }
        
        protected static NPath ReactBaseDirectory
        {
            get => NPath.GetCurrentDirectory().UpDirectory().SubDirectory("UI");
        }

        protected static NPath FlutterPagesDirectory
        {
            get => NPath.GetCurrentDirectory().UpDirectory()
                .SubDirectory("flutter")
                .SubDirectory("mobile")
                .SubDirectory("lib");
        }


        public static NPath ReactPagesBaseDirectory { 
            get=> NPath.GetCurrentDirectory().UpDirectory().SubDirectory("UI").SubDirectory("src").SubDirectory("Pages");
        } 
        
        public static NPath ReactAppJs
        {
            get => NPath.GetCurrentDirectory().UpDirectory().SubDirectory("UI").SubDirectory("src").SubDirectory("App.js");
        }


        public virtual Task Dispose(string ObjectName)
        {
            return Task.CompletedTask;
        }
    }
}
