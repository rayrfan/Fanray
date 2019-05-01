using Fan.Helpers;
using Fan.Membership;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace Fan.Data
{
    /// <summary>
    /// The db context for the entire system.
    /// </summary>
    public class FanDbContext : IdentityDbContext<User, Role, int>
    {
        private readonly ILogger<FanDbContext> logger;

        public FanDbContext(DbContextOptions<FanDbContext> options, ILoggerFactory loggerFactory) 
            : base(options)
        {
            logger = loggerFactory.CreateLogger<FanDbContext>();
        }

        /// <summary>
        /// Creates the data model for entire system.
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <remarks>
        /// Having multiple DbContexts and still create database for user on app launch is a challenge,
        /// <see cref="https://stackoverflow.com/a/11198345/32240"/>. I get around this issue using 
        /// reflection here to load everything dynamically. This method is called once on startup.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // find entities and model builders from app assemblies
            var entityTypes = TypeFinder.Find<Entity>();
            var modelBuilderTypes = TypeFinder.Find<IEntityModelBuilder>();

            // add entity types to the model
            foreach (var type in entityTypes)
            {
                modelBuilder.Entity(type);
                logger.LogInformation($"Entity: '{type.Name}' added to model");
            }

            // call base
            base.OnModelCreating(modelBuilder);

            // add mappings and relations
            foreach (var builderType in modelBuilderTypes)
            {
                if (builderType != null && builderType != typeof(IEntityModelBuilder))
                {
                    logger.LogInformation($"ModelBuilder '{builderType.Name}' added to model");
                    var builder = (IEntityModelBuilder) Activator.CreateInstance(builderType);
                    builder.CreateModel(modelBuilder);
                }
            }
        }
    }
}
