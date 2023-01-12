namespace NGen
{
    public static class Extention
    {
        public static FormModuleField AsFileManager(this FormModuleField @this)
        {
            @this.ReactHtmlFunc = (Type pageType, Type moduleType, string viewModel) =>
            {
                return @$"<div class=""form-group"">
        <label>{@this._name}</label>
     
    <FileManagerModule
            onChange={{(e) => SetForm( (f) =>{{return {{...f, ['{@this._name.FirstCharToLower()}']: e.target.value}}; }}) }}
            formName={{'{@this._name.FirstCharToLower()}'}}
            setForm={{SetForm}}
            value={{form.{@this._name.FirstCharToLower()}}}
            ClassName={{""form-control""}}
            placeholder={{'{@this._name}'}}
/>
    </div>";

            };
            
            @this.ExtraReactImports.Add(("FileManager" , "import FileManagerModule from '../../IndependentModules/FileManager';"));

            return @this;

        }
    }
}
