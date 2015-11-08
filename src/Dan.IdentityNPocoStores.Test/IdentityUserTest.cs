using Dan.IdentityNPocoStores.Core;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Dan.IdentityNPocoStores.Test
{
    public sealed class IdentityUserTest
    {
        [Fact]
        public void IdIsGeneratedAutomatically()
        {
            var user = new IdentityUser();
            var user2 = new IdentityUser();

            user.Id.Should().NotBeNullOrWhiteSpace();
            user2.Id.Should().NotBe(user.Id);
        }

        [Fact]
        public void UsernameConstructorPopulates()
        {
            var user = new IdentityUser("TestUser");

            user.UserName.Should().Be("TestUser");
        }

        [Fact]
        public void UsernameConstructorPopulatesId()
        {
            var user = new IdentityUser("Test");
            var user2 = new IdentityUser("Test2");

            user.Id.Should().NotBeNullOrWhiteSpace();
            user2.Id.Should().NotBe(user.Id);
        }
    }
}
