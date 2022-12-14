using AutoMapper;
using System.Linq.Expressions;

namespace NSharp
{
    public static class Extentions
    {
        public static string Join(this IEnumerable<string> @this , char seprator) => string.Join(seprator, @this);

        public static string Join(this IEnumerable<string> @this , string seprator) => string.Join(seprator, @this);

        public static bool HasValue(this Guid? @this)
        {
            return @this != null && @this != Guid.Empty;
        }
        public static bool HasValue(this Guid @this)
        {
            return  @this != Guid.Empty;
        }

        public static FlutterPage AsFlutterPage(this string @this)
        {
            return new FlutterPage(@this);
        }

        public static string IfEmpty(this string @this , string replace)
        {
            if (@this.HasValue())
                return @this;
            return replace;
        }
        
        public static string PlaceIf(this string @this, bool con, string replace)
        {
            if (con)
                return @this;
            return replace;
        }

        public static string FirstCharToLower(this string @this)
        {
            if (@this.None()) return @this;

            @this = @this.Trim();
            return @this.Substring(0, 1).ToLower() + @this.Substring(1, @this.Length -1);
        }

        public static TEntity MapFrom<TEntity,T>(this TEntity @this , T from) where TEntity : class where T : class 
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<T, TEntity>());

            var mapper = new Mapper(config);
            return mapper.Map(from, @this);

        }



        public static string EnsureStartWith(this string @this  , char target)
        {
            @this = @this.Trim();

            if (@this.StartsWith(target))
                return @this;

            return target + @this;
        }
       
        public static string EnsureNotStartWith(this string @this  , char target)
        {
            if (@this.Trim().StartsWith(target))
               return @this.Trim().Substring(1);
            return @this;   
        }

        public static bool HasValue(this string @this)
        {
           return string.IsNullOrEmpty(@this) ? false : true;
        }
        
        public static bool None(this string? @this)
        {
           return string.IsNullOrEmpty(@this) ? true : false;
        }

        public static string OnlyWhen(this string @this , bool condition)
        {
            if (condition)
                return @this;
            return "";
        }

        public static Controller AsController(this string @this)
        {
            return new Controller(@this);
        }

        public static string MemberName<T, U>(this Expression<Func<T, U>> expression)
        {
            return (expression.Body as MemberExpression).Member.Name;
        }
        
        public static string TypeName<T, U>(this Expression<Func<T, U>> expression)
        {
            var name = expression.MemberName();
            return typeof(T).GetProperties().First(c => c.Name == name).PropertyType.Name;
        }


        public static string TrimFromEnd(this string @this, char end)
        {
            var data = @this.LastIndexOf(end);
            if (data == -1)
                return @this;
            return @this.Substring(0, data);
        }

        public static bool None(this IEnumerable<object> @this)
        {
            if (@this == null || !@this.Any())
                return true;
            return false;
        }
        
        public static string RemoveBetween(this string @this , string between)
        {
            var data = @this;

            var from = data.IndexOf(between) + between.Length;
            data = data.Substring(from);
            var to = data.IndexOf(between);
            
            return @this.Remove(from, to );
        }

        public static string RemoveBetweens(this string @this , string between , string secend)
        {
            var data = @this;

            var from = data.IndexOf(between) + between.Length;
            data = data.Substring(from);
            var to = data.IndexOf(secend);
            
            return @this.Remove(from, to );
        }
        public static string InsertBefor(this string @this , string between , string lines)
        {
            var from = @this.IndexOf(between);
             return  @this.Insert(from, lines);
        }
        public static string InsertBeforLast(this string @this , string last , string lines)
        {
                        var index = @this.LastIndexOf(last);
            return @this.Insert(index, lines);  
        }


        public static string InsertAfter(this string @this , string between , string lines)
        {
            var from = @this.IndexOf(between) + between.Length;
             return  @this.Insert(from, lines);
        }

        public static ReactFile AsReactFile(this string @this)=> new ReactFile( @this);


        public static string Join(this List<string> @this , string seprator = "")
        {
            var data = "";

            if (@this.None())
                return data;

            for (int i = 0; i < @this.Count; i++)
            {
                if (i == @this.Count - 1)
                    data += @this[i];

                else
                   data += @this[i] + seprator;

            }

            return data;
        }

    }
}
