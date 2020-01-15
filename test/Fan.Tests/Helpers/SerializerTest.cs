using Fan.Helpers;
using Fan.Membership;
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
            var list = new List<User> {
                new User { DisplayName = "User1", UserName = "user1" },
                new User { DisplayName = "User2", UserName = "user2" },
            };

            // Act: serialize it to bytes and back
            var bytes = await Serializer.ObjectToBytesAsync(list);
            var list2 = await Serializer.BytesToObjectAsync<List<User>>(bytes);

            // Assert
            Assert.Equal(list.Count, list2.Count);
        }
    }
}
