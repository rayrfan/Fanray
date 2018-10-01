using Fan.Helpers;
using Fan.Membership;
using System.Collections.Generic;
using Xunit;
namespace Fan.UnitTests.Helpers
{
    /// <summary>
    /// Test for <see cref="RegexUtilities"/> class.
    /// </summary>
    public class RegexUtilitiesTest
    {
        [Fact]
        public void RegexUtilities_Valid_Email_Address()
        {
            // list valid email addres
            string[] emailAddresses = { "david.jones@proseware.com", "d.j@server1.proseware.com",
                                  "jones@ms1.proseware.com",
                                  "j@proseware.com9", "js#internal@proseware.com",
                                  "j_9@[129.126.118.1]",
                                  "js@proseware.com9", "j.s@server1.proseware.com",
                                   "\"j\\\"s\\\"\"@proseware.com", "js@contoso.中国" };
            // Act: valid email
            var valid = true;
            foreach (var item in emailAddresses)
            {
                var result = RegexUtilities.IsValidEmail(item);
                
                if(!result)
                //if item is Invalid email address
                    valid = result;
            }
            // Assert
            Assert.True(valid);
            
        }

        [Fact]
        public void RegexUtilities_InValid_Email_Address()
        {
            // list invalid email addres
            string[] emailAddresses = {"j.@server1.proseware.com","j..s@proseware.com","js*@proseware.com","js@proseware..com","username"};

            // Act: valid email
            var valid = true;
            foreach (var item in emailAddresses)
            {
                var result = RegexUtilities.IsValidEmail(item);
                
                if(!result)
                //if item is Invalid email address
                    valid = result;
            }
            // Assert
            Assert.True(!valid);

           
        }
    }
}
