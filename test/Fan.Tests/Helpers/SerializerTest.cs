using Fan.Helpers;
using Fan.Models;
using System.Collections.Generic;
using Xunit;

namespace Fan.Tests.Helpers
{
    /// <summary>
    /// Test for <see cref="Serializer"/> class.
    /// </summary>
    public class SerializerTest
    {
        [Fact]
        public async void Serializer_Can_Serialize_Object_To_ByteArray_And_Back()
        {
            // Arrange: a list of two blog post
            var list = new List<BlogPost> {
                new BlogPost { Title = "Post 1", UserName = "Ray" },
                new BlogPost { Title = "Post 2", UserName = "Ray" },
            };

            // Act: serialize it to bytes and back
            var bytes = await Serializer.ObjectToBytesAsync(list);
            var list2 = await Serializer.BytesToObjectAsync<List<BlogPost>>(bytes);

            // Assert
            Assert.Equal(list.Count, list2.Count);
        }
    }
}
