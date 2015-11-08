using Dan.IdentityNPocoStores.Core;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Dan.IdentityNPocoStores.Test
{
    public sealed class IdentityRoleTest
    {
        [Fact]
        public void IdIsGeneratedAutomatically()
        {
            var role = new IdentityRole();
            var role2 = new IdentityRole();

            role.Id.Should().NotBeNullOrWhiteSpace();
            role2.Id.Should().NotBe(role.Id);
        }

        [Fact]
        public void NameConstructorPopulates()
        {
            var role = new IdentityRole("TestRole");

            role.Name.Should().Be("TestRole");
        }

        [Fact]
        public void NameConstructorPopulatesId()
        {
            var role = new IdentityRole("Test");
            var role2 = new IdentityRole("Test2");

            role.Id.Should().NotBeNullOrWhiteSpace();
            role2.Id.Should().NotBe(role.Id);
        }
    }
}
