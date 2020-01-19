using Fan.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Tests.Caching
{
    /// <summary>
    /// Tests for <see cref="CacheExtensions"/> class.
    /// </summary>
    public class CacheExtensionsTest
    {
        private readonly IDistributedCache cache;

        /// <summary>
        /// Sets up the distributed cache.
        /// </summary>
        public CacheExtensionsTest()
        {
            var serviceProvider = new ServiceCollection().AddMemoryCache().BuildServiceProvider();
            var memCacheOptions = serviceProvider.GetService<IOptions<MemoryDistributedCacheOptions>>();
            cache = new MemoryDistributedCache(memCacheOptions);
        }

        /// <summary>
        /// This test proves GetAsync is able to cache the object the first time you need it
        /// and subsequently it returns the object from cache without calling the service.
        /// </summary>
        [Fact]
        public async void GetAsync_caches_object_upon_initial_call()
        {
            // Given SettingService and CoreSettings
            var settingService = new Mock<ISettingService>();
            settingService.Setup(t => t.GetSettingsAsync<CoreSettings>()).Returns(Task.FromResult(new CoreSettings()));

            // When I ask cache for it for the first time, it calls the service, gets the object 
            // and caches it
            var res1 = await cache.GetAsync("cache-key", new TimeSpan(0, 10, 0), async () =>
            {
                return await settingService.Object.GetSettingsAsync<CoreSettings>();
            });

            // When I aske cache for a second time, it returns the object from cache directly 
            // and won't call the service
            var res2 = await cache.GetAsync("cache-key", new TimeSpan(0, 10, 0), async () =>
            {
                return await settingService.Object.GetSettingsAsync<CoreSettings>();
            });

            // Then the service method has only been called exactly once
            // And the objects returned each time matches their values
            settingService.Verify(service => service.GetSettingsAsync<CoreSettings>(), Times.Exactly(1));
            Assert.Equal(res1.Title, res2.Title);
        }

        /// <summary>
        /// Unalbe to cache derived class (is-a), its property TotalStrings is not serialized.
        /// </summary>
        [Fact]
        public async void GetAsync_is_not_able_serialize_prop_on_derived_list_type()
        {
            await cache.GetAsync("strlist-cache-key", new TimeSpan(0, 10, 0), async () =>
            {
                var list = new StrList
                {
                    "test"
                };
                list.TotalStrings = 1;
                return await Task.FromResult(list);
            }, includeTypeName: true);

            var result = await cache.GetAsync("strlist-cache-key", new TimeSpan(0, 10, 0), async () =>
            {
                return await Task.FromResult(new StrList());
            }, includeTypeName: true);

            Assert.Single(result);
            Assert.NotEqual(1, result.TotalStrings);
            Assert.Equal(0, result.TotalStrings);
        }

        /// <summary>
        /// Able to cache containing class (has-a), its property TotalStrings is serialized correctly.
        /// </summary>
        [Fact]
        public async void GetAsync_is_able_to_serialize_prop_on_type_that_contains_list()
        {
            await cache.GetAsync("strlist2-cache-key", new TimeSpan(0, 10, 0), async () =>
            {
                var list = new StrList2();
                list.Strings.Add("test");
                list.TotalStrings = 1;
                return await Task.FromResult(list);
            });

            var result = await cache.GetAsync("strlist2-cache-key", new TimeSpan(0, 10, 0), async () =>
            {
                return await Task.FromResult(new StrList2());
            });

            Assert.Single(result.Strings);
            Assert.Equal(1, result.TotalStrings);
        }

        /// <summary>
        /// When includeTypeName is set to true, the cache is able to serialize and deserialize
        /// derived types.
        /// </summary>
        [Fact]
        public async void GetAsync_is_able_to_serialize_derived_types()
        {
            // Given a company with 1 employee cached
            await cache.GetAsync("company-key", new TimeSpan(0, 1, 0), async () =>
            {
                return await Task.FromResult(new Company
                {
                    FullName = "Some Company",
                    Employees = new List<Person>
                    {
                        new Engineer
                        {
                            Name = "Ray Fan",
                            Stars = 5
                        }
                    }
                });
            }, includeTypeName: true);

            // When the company is accessed from cache again
            var result = await cache.GetAsync("company-key", new TimeSpan(0, 1, 0), async () =>
            {
                return await Task.FromResult(new Company()); // won't be returned since a cached ver is available
            }, includeTypeName: true);

            // Then the derived type is returned
            // Note if includeTypeName is not set to true, the type here will be "Person"
            Assert.Equal("Engineer", result.Employees[0].GetType().Name);
        }
    }

    class Person
    {
        public string Name { get; set; }
    }

    class Engineer : Person
    {
        public int Stars { get; set; }
    }

    class Company
    {
        public string FullName { get; set; }
        public IList<Person> Employees { get; set; }
    }

    /// <summary>
    /// Derived class (is-a) not able to cache the property TotalStrings.
    /// </summary>
    class StrList : List<string>
    {
        public StrList()
        {
        }
        public int TotalStrings { get; set; }
    }

    /// <summary>
    /// Containing class (has-a) is able to cache the property TotalStrings.
    /// </summary>
    class StrList2
    {
        public StrList2()
        {
            Strings = new List<string>();
        }
        public List<string> Strings { get; set; }
        public int TotalStrings { get; set; }
    }
}