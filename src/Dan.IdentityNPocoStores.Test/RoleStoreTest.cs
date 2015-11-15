using Dan.IdentityNPocoStores.Core;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Dan.IdentityNPocoStores.Test
{
    [Collection("IdentityStores")]
    public sealed class RoleStoreTest
    {
        private readonly UserData _userData;

        public RoleStoreTest(UserData userData)
        {
            _userData = userData;
        }

        [Fact]
        public async Task CreateAsync_CreatesRole()
        {
            var store = _userData.GetRoleStore();
            var role = new IdentityRole("DummyRole")
            {
                NormalizedName = "DUMMYROLE",
            };

            await store.CreateAsync(role, CancellationToken.None);

            var savedRole = _userData.GetRole(role.Id);

            savedRole.Should().NotBeNull();
            savedRole.Name.Should().Be("DummyRole");
            savedRole.NormalizedName.Should().Be("DUMMYROLE");
            savedRole.Id.Should().Be(role.Id);
        }

        [Fact]
        public async Task UpdateAsync_Updates()
        {
            var store = _userData.GetRoleStore();
            var role = new IdentityRole("EditRole")
            {
                NormalizedName = "EDITROLE"
            };

            await store.CreateAsync(role, CancellationToken.None);

            role.Name = "EditRole2";
            role.NormalizedName = "EDITROLE2";

            await store.UpdateAsync(role, CancellationToken.None);

            var savedRole = _userData.GetRole(role.Id);

            savedRole.Should().NotBeNull();
            savedRole.Name.Should().Be("EditRole2");
            savedRole.NormalizedName.Should().Be("EDITROLE2");
        }

        [Fact]
        public async Task UpdateAsync_ChecksConcurrency()
        {
            var store = _userData.GetRoleStore();
            var role = await store.FindByNameAsync(UserData.TestRoleName, CancellationToken.None);

            // Change stamp
            _userData.ChangeConcurrencyStampForRole(role.Id);

            // Try to update user
            await Assert.ThrowsAsync<IdentityConcurrencyException>(async () => await store.UpdateAsync(role, CancellationToken.None));
        }

        [Fact]
        public async Task DeleteAsync_Deletes()
        {
            var store = _userData.GetRoleStore();
            var role = new IdentityRole("DeleteRole");

            await store.CreateAsync(role, CancellationToken.None);
            await store.DeleteAsync(role, CancellationToken.None);

            var savedRole = _userData.GetRole(role.Id);

            savedRole.Should().BeNull();
        }

        [Fact]
        public async Task GetRoleIdAsync_GetsId()
        {
            var store = _userData.GetRoleStore();
            var role = new IdentityRole("Test");

            var id = await store.GetRoleIdAsync(role, CancellationToken.None);

            id.Should().Be(role.Id);
        }

        [Fact]
        public async Task GetRoleNameAsync_GetsRoleName()
        {
            var store = _userData.GetRoleStore();
            var role = new IdentityRole("Test");

            var name = await store.GetRoleNameAsync(role, CancellationToken.None);

            name.Should().Be("Test");
        }

        [Fact]
        public async Task SetRoleNameAsync_SetsRoleName()
        {
            var store = _userData.GetRoleStore();
            var role = new IdentityRole("Wrong");

            await store.SetRoleNameAsync(role, "Correct", CancellationToken.None);

            role.Name.Should().Be("Correct");
        }

        [Fact]
        public async Task GetNormalizedRoleNameAsync_GetsNormalizedName()
        {
            var store = _userData.GetRoleStore();
            var role = new IdentityRole() { NormalizedName = "NormName" };

            var name = await store.GetNormalizedRoleNameAsync(role, CancellationToken.None);

            name.Should().Be("NormName");
        }

        [Fact]
        public async Task SetNormalizedRoleNameAsync_SetsNormalizedRoleName()
        {
            var store = _userData.GetRoleStore();
            var role = new IdentityRole() { NormalizedName = "Wrong" };

            await store.SetNormalizedRoleNameAsync(role, "Correct", CancellationToken.None);

            role.NormalizedName.Should().Be("Correct");
        }

        [Fact]
        public async Task FindByIdAsync_FindsRole()
        {
            var store = _userData.GetRoleStore();

            var role = await store.FindByIdAsync(_userData.TestRoleId, CancellationToken.None);

            role.Should().NotBeNull();
            role.Id.Should().Be(_userData.TestRoleId);
            role.Name.Should().Be(UserData.TestRoleName);
        }

        [Fact]
        public async Task FindByNameAsync_FindsRole()
        {
            var store = _userData.GetRoleStore();

            var role = await store.FindByNameAsync(UserData.TestRoleName, CancellationToken.None);

            role.Should().NotBeNull();
            role.Name.Should().Be(UserData.TestRoleName);
            role.Id.Should().Be(_userData.TestRoleId);
        }

        [Fact]
        public async Task GetClaimsAsync_GetsClaims()
        {
            var store = _userData.GetRoleStore();
            var role = new IdentityRole("ClaimsTest");
            var claim1 = new Claim("GetClaims", "Claim1");
            var claim2 = new Claim("GetClaims", "Claim2");

            await store.CreateAsync(role, CancellationToken.None);
            await store.AddClaimAsync(role, claim1, CancellationToken.None);
            await store.AddClaimAsync(role, claim2, CancellationToken.None);

            var claims = await store.GetClaimsAsync(role, CancellationToken.None);

            claims.Should().NotBeNullOrEmpty();
            claims.Should().HaveCount(2);
            claims.FirstOrDefault(c => c.Type == "GetClaims" && c.Value == "Claim1").Should().NotBeNull();
            claims.FirstOrDefault(c => c.Type == "GetClaims" && c.Value == "Claim2").Should().NotBeNull();
        }

        [Fact]
        public async Task AddClaimAsync_AddsClaim()
        {
            var store = _userData.GetRoleStore();
            var role = new IdentityRole("AddClaimTest");
            var claim = new Claim("AddClaim", "Claim");

            await store.CreateAsync(role, CancellationToken.None);
            await store.AddClaimAsync(role, claim, CancellationToken.None);

            var claims = await store.GetClaimsAsync(role, CancellationToken.None);

            claims.Should().NotBeNullOrEmpty();
            claims.Should().HaveCount(1);
            claims.First().Type.Should().Be("AddClaim");
            claims.First().Value.Should().Be("Claim");
        }

        [Fact]
        public async Task RemoveClaimAsync_RemovesClaim()
        {
            var store = _userData.GetRoleStore();
            var role = new IdentityRole("RemoveClaimTest");
            var claim = new Claim("RemoveClaim", "Claim");

            await store.CreateAsync(role, CancellationToken.None);
            await store.AddClaimAsync(role, claim, CancellationToken.None);
            await store.RemoveClaimAsync(role, claim, CancellationToken.None);

            var claims = await store.GetClaimsAsync(role, CancellationToken.None);

            claims.Should().BeEmpty();
        }
    }
}
