using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;

namespace Fan.Helpers
{
    /// <summary>
    /// Finds a type in assembly.
    /// </summary>
    /// <remarks>
    /// TODO needs a new strategy.
    /// </remarks>
    public class TypeFinder
    {
        /// <summary>
        /// Skip dll file name regex.
        /// </summary>
        public const string SKIP_DLL_REGEX = "^System|^Microsoft|^Newtonsoft|^AutoMapper|^Humanizer|^MediatR|^Scrutor|^Serilog|^HtmlAgilityPack|^SixLabors|^FluentValidation|^morelinq|^TimeZoneConverter|^WindowsAzure|^Moq|^xunit";

        /// <summary>
        /// Returns types that derive or implement type T from sln projects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> Find<T>()
        {
            return Find(typeof(T));
        }

        /// <summary>
        /// Returns types that derive or implement baseType from sln projects.
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static IEnumerable<Type> Find(Type baseType)
        {
            var types = new List<Type>();
            var dlls = new DirectoryInfo(AppContext.BaseDirectory).GetFileSystemInfos("*.dll", SearchOption.TopDirectoryOnly);
            foreach (var dll in dlls)
            {
                try
                {
                    Assembly assembly = null;
                    var fileName = Path.GetFileName(dll.FullName);
                    if (IsDllMatch(fileName))
                        assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll.FullName);

                    if (assembly != null)
                    {
                        if (baseType.IsInterface)
                            types.AddRange(assembly.DefinedTypes.Where(t =>
                                (baseType.IsAssignableFrom(t) || (baseType.IsGenericTypeDefinition && DoesTypeImplementGeneric(t, baseType)))
                                && !t.IsInterface));
                        else
                            types.AddRange(assembly.DefinedTypes.Where(t => t.BaseType == baseType && !t.GetTypeInfo().IsAbstract));
                    }
                }
                catch (BadImageFormatException)
                {
                    // non .net dll
                }
                catch (ReflectionTypeLoadException)
                {
                    // missing a referenced dll
                }
            }

            return types;
        }

        /// <summary>
        /// Returns true if dll file name is a match.
        /// </summary>
        /// <param name="fileName">A dll file name.</param>
        /// <returns></returns>
        public static bool IsDllMatch(string fileName) =>
            !Regex.IsMatch(fileName, SKIP_DLL_REGEX, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Returns true if the type implements the genericType.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericType">The base type that is a generic type.</param>
        /// <returns></returns>
        private static bool DoesTypeImplementGeneric(Type type, Type genericType)
        {
            try
            {
                // gets the type object that represents a generic type definition from which
                // the current generic type can be constructed.
                var genericTypeDef = genericType.GetGenericTypeDefinition();

                // finds type objects representing a list of interfaces implemented or inherited by the current type.
                var implementedInterfaces = type.FindInterfaces((objType, objCriteria) => true, null);
                foreach (var implementedInterface in implementedInterfaces)
                {
                    if (!implementedInterface.IsGenericType)
                        continue;

                    return genericTypeDef.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition());
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
