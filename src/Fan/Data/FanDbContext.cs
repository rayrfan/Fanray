using Fan.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Fan.Data
{
    /// <summary>
    /// The db context for the entire system.
    /// </summary>
    public class FanDbContext : IdentityDbContext<User, Role, int>
    {
        public FanDbContext(DbContextOptions<FanDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Creates the data model for entire system.
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <remarks>
        /// Having multiple DbContexts and still create database for user on app launch is a challenge,
        /// <see cref="https://stackoverflow.com/a/11198345/32240"/>. I get around this issue using 
        /// reflection here to load everything dynamically.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // logger, can't inject through constructor due to AddDbContextPool restriction
            var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<FanDbContext>();

            // bin dir
            var bin = new DirectoryInfo(Directory.GetCurrentDirectory());
            logger.LogInformation($"Bin: {bin}");

            // find entities and model builders from app assemblies
            var entityTypes = new List<Type>();
            var modelBuilderTypes = new List<Type>();
            foreach (var dll in bin.GetFileSystemInfos("*.dll", SearchOption.AllDirectories))
            {
                // https://stackoverflow.com/a/44139005/32240
                // https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Runtime/Loader/AssemblyLoadContext.cs
                Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll.FullName);
                logger.LogInformation($"Assembly: {assembly.FullName} loaded");

                entityTypes.AddRange(assembly.DefinedTypes.Where(t => t.BaseType == typeof(Entity) && !t.GetTypeInfo().IsAbstract));
                modelBuilderTypes.AddRange(assembly.DefinedTypes.Where(t => typeof(IEntityModelBuilder).IsAssignableFrom(t)));
            }

            // add entity types to the model
            foreach (var type in entityTypes)
            {
                modelBuilder.Entity(type);
                logger.LogInformation($"Entity: {type.Name} added to model");
            }

            // call base
            base.OnModelCreating(modelBuilder);

            // add mappings and relations
            foreach (var builderType in modelBuilderTypes)
            {
                if (builderType != null && builderType != typeof(IEntityModelBuilder))
                {
                    logger.LogInformation($"ModelBuilder {builderType.Name} added to model");
                    var builder = (IEntityModelBuilder) Activator.CreateInstance(builderType);
                    builder.CreateModel(modelBuilder);
                }
            }
        }
    }
}

