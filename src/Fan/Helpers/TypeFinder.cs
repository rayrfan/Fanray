using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Fan.Helpers
{
    public class TypeFinder
    {
        /// <summary>
        /// Sln project dlls.
        /// </summary>
        private FileSystemInfo[] _dllInfos;
        private readonly ILogger<TypeFinder> _logger;

        public TypeFinder(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TypeFinder>();
            
            // https://github.com/aspnet/Announcements/issues/237
            // the dir that contains dlls "...\bin\Debug\netcoreapp2.0\"
            var dllDir = new DirectoryInfo(AppContext.BaseDirectory); 
            _dllInfos = dllDir.GetFileSystemInfos("*.dll", SearchOption.AllDirectories);
        }

        /// <summary>
        /// Returns types that derive or implement type T from sln projects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<Type> Find<T>()
        {
            return Find(typeof(T));
        }

        /// <summary>
        /// Returns types that derive or implement baseType from sln projects.
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public IEnumerable<Type> Find(Type baseType)
        {
            var types = new List<Type>();
            foreach (var dll in _dllInfos)
            {
                Assembly assembly = null;
                try
                {
                    assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll.FullName);
                }
                catch (BadImageFormatException ex)
                {
                    _logger.LogCritical($"Unable to load dll {dll.FullName} - {ex.Message}");
                }

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

            return types;
        }

        /// <summary>
        /// Returns true if the type implements the genericType.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericType">The base type that is a generic type.</param>
        /// <returns></returns>
        private bool DoesTypeImplementGeneric(Type type, Type genericType)
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
