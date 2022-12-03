using NSharp;

namespace NGen.NewModuleStructure
{
    public abstract class AModule
    {

        #region Backend
        private protected List<(string Name, string Action)> Actions = new List<(string Name, string Action)>();
        private protected List<(string Type, string Name)> Props = new List<(string Type, string Name)>();
        #endregion

        #region React
        private protected List<(string Name, string Import)> ReactImport = new List<(string Name, string Import)>();
        private protected List<(string Name, string Method)> ReactBodys = new List<(string Name, string Method)>();
        private protected string ReactHTML = string.Empty;
        private protected List<(string Name, string Method)> ReactBeforMethod = new List<(string Name, string Method)>();
        private protected List<(string Name, string html)> ReactButtonHtmls = new List<(string Name, string html)>();

        #endregion

        public abstract string GetActions(Type pageType, Type moduleType);
        public abstract string GetViewModel(Type pageType, Type moduleType);
        public abstract string GetReactImports(Type pageType, Type moduleType);

        public virtual string GetReactBody(Type pageType, Type moduleType)
        {
            return @$"{ReactBodys.GroupBy(c => c.Method).Select(c => $"\t{{/* {c.Select(c => c.Name).Join(',')} */}}\n\t{c.Key}").Join('\n')}";
        }

        public abstract string GetReactHTML(Type pageType, Type moduleType);
        public abstract string GetReactBeforMethod(Type pageType, Type moduleType);
        public abstract string GetReactPage(Type pageType, Type moduleType);
        public virtual string GetReactModuleName(Type pageType, Type moduleType) => moduleType.Name + "Module";

    }

}
