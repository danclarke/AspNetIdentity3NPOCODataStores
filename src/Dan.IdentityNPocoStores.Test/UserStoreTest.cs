using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Dan.IdentityNPocoStores.Core;
using FluentAssertions;
using Xunit;
using Microsoft.AspNet.Identity;
using System.Security.Claims;

namespace Dan.IdentityNPocoStores.Test
{
    [CollectionDefinition("IdentityStores")]
    public sealed class IdentityStoresClass : ICollectionFixture<UserData> { }

    [Collection("IdentityStores")]
    public sealed class UserStoreTest
    {
        private readonly UserData _userData;

        public UserStoreTest(UserData userData)
        {
            _userData = userData;
        }

        [Fact]
        public async Task GetUserIdAsync_GetsId()
        {
            const string id = "TestId";
            var user = new IdentityUser("Test") { Id = id };
            var store = _userData.GetUserStore();

            var fetchId = await store.GetUserIdAsync(user, CancellationToken.None);

            fetchId.Should().Be(id);
        }

        [Fact]
        public async Task GetUserNameAsync_GetsUserName()
        {
            const string name = "Test Name";
            var user = new IdentityUser(name);
            var store = _userData.GetUserStore();

            var fetchName = await store.GetUserNameAsync(user, CancellationToken.None);

            fetchName.Should().Be(name);
        }

        [Fact]
        public async Task SetUserNameAsync_SetsUserName()
        {
            const string name = "Test Name";
            var user = new IdentityUser("Not correct");
            var store = _userData.GetUserStore();

            await store.SetUserNameAsync(user, name, CancellationToken.None);

            user.UserName.Should().Be(name);
        }

        [Fact]
        public async Task SetNormalizedUserNameAsync_SetsNormalizedUserName()
        {
            const string name = "Test Name";
            var user = new IdentityUser("Not correct") { NormalizedUserName = "Not correct" };
            var store = _userData.GetUserStore();

            await store.SetNormalizedUserNameAsync(user, name, CancellationToken.None);

            user.NormalizedUserName.Should().Be(name);
        }

        [Fact]
        public async Task CreateAsync_Creates()
        {
            var user = new IdentityUser("Create User")
            {
                NormalizedUserName = "Create User",
                Email = "createtest@test.com",
                NormalizedEmail = "createtest@test.com",
                PasswordHash = UserData.TestUserPassword
            };

            var store = _userData.GetUserStore();

            await store.CreateAsync(user, CancellationToken.None);

            // Verify user was saved to DB
            var savedUser = _userData.GetUser(user.Id);

            savedUser.NormalizedUserName.Should().Be("Create User");
            savedUser.Email.Should().Be("createtest@test.com");
            savedUser.PasswordHash.Should().Be(UserData.TestUserPassword);
        }

        [Fact]
        public async Task UpdateAsync_Updates()
        {
            var store = _userData.GetUserStore();
            var user = await store.FindByNameAsync(UserData.TestUserName, CancellationToken.None);

            user.Should().NotBeNull("User should be present");
            user.EmailConfirmed.Should().Be(false);

            user.EmailConfirmed = true;
            await store.UpdateAsync(user, CancellationToken.None);

            // Verify locally
            user.EmailConfirmed.Should().Be(true);

            // Verify in DB
            var savedUser = _userData.GetUser(user.Id);
            savedUser.EmailConfirmed.Should().Be(true);

            // Restore for later use
            user.EmailConfirmed = false;
            await store.UpdateAsync(user, CancellationToken.None);
        }

        [Fact]
        public async Task DeleteAsync_Deletes()
        {
            var store = _userData.GetUserStore();
            var user = new IdentityUser("Delete User")
            {
                NormalizedUserName = "Delete User",
                Email = "deletetest@test.com",
                NormalizedEmail = UserData.TestUserEmail,
                PasswordHash = UserData.TestUserPassword
            };

            // Create then delete, create is tested elsewhere so should be ok here
            await store.CreateAsync(user, CancellationToken.None);
            await store.DeleteAsync(user, CancellationToken.None);

            // Verify
            var deletedUser = _userData.GetUser(user.Id);

            deletedUser.Should().BeNull();
        }

        [Fact]
        public async Task FindById_Finds()
        {
            var store = _userData.GetUserStore();
            var user = await store.FindByIdAsync(_userData.TestUserId, CancellationToken.None);

            user.Should().NotBeNull();
            user.Id.Should().Be(_userData.TestUserId);
            user.UserName.Should().Be(UserData.TestUserName);
            user.Email.Should().Be(UserData.TestUserEmail);
            user.PasswordHash.Should().Be(UserData.TestUserPassword);
        }

        [Fact]
        public async Task FindByName_Finds()
        {
            var store = _userData.GetUserStore();
            var user = await store.FindByNameAsync(UserData.TestUserName, CancellationToken.None);

            user.Should().NotBeNull("User should be present");
            user.UserName.Should().Be(UserData.TestUserName);
            user.Email.Should().Be(UserData.TestUserEmail);
            user.PasswordHash.Should().Be(UserData.TestUserPassword);
        }

        [Fact]
        public async Task AddLoginAsync_AddsLogin()
        {
            var store = _userData.GetUserStore();
            var user = await store.FindByIdAsync(_userData.TestUserId, CancellationToken.None);
            var loginInfo = new UserLoginInfo("AddLoginProvider", "ProviderKey", "DisplayName");

            await store.AddLoginAsync(user, loginInfo, CancellationToken.None);

            var addedInfo = _userData.GetUserLogin("AddLoginProvider", "ProviderKey", user.Id);

            addedInfo.Should().NotBeNull();
            addedInfo.LoginProvider.Should().Be("AddLoginProvider");
            addedInfo.ProviderKey.Should().Be("ProviderKey");
            addedInfo.ProviderDisplayName.Should().Be("DisplayName");
        }

        [Fact]
        public async Task RemoveLoginAsync_RemovesLogin()
        {
            var store = _userData.GetUserStore();
            var user = await store.FindByIdAsync(_userData.TestUserId, CancellationToken.None);
            var loginInfo = new UserLoginInfo("RemoveLoginProvider", "ProviderKey", "DisplayName");

            await store.AddLoginAsync(user, loginInfo, CancellationToken.None);
            await store.RemoveLoginAsync(user, loginInfo.LoginProvider, loginInfo.ProviderKey, CancellationToken.None);

            var addedInfo = _userData.GetUserLogin("RemoveLoginProvider", "ProviderKey", user.Id);

            addedInfo.Should().BeNull();
        }

        [Fact]
        public async Task GetLoginsAsync_GetsLogins()
        {
            var store = _userData.GetUserStore();
            var user = await store.FindByIdAsync(_userData.TestUserId, CancellationToken.None);
            var loginInfo = new UserLoginInfo("GetLoginProvider", "ProviderKey", "DisplayName");
            var loginInfo2 = new UserLoginInfo("GetLoginProvider", "ProviderKey2", "DisplayName2");

            // Add the test logins
            await store.AddLoginAsync(user, loginInfo, CancellationToken.None);
            await store.AddLoginAsync(user, loginInfo2, CancellationToken.None);

            // Fetch the logins
            var logins = await store.GetLoginsAsync(user, CancellationToken.None);

            logins.Should().NotBeNullOrEmpty();
            logins.Where(l => l.LoginProvider == "GetLoginProvider").Should().HaveCount(2);
            logins.FirstOrDefault(l => l.LoginProvider == "GetLoginProvider" && l.ProviderKey == "ProviderKey" && l.ProviderDisplayName == "DisplayName").Should().NotBeNull();
            logins.FirstOrDefault(l => l.LoginProvider == "GetLoginProvider" && l.ProviderKey == "ProviderKey2" && l.ProviderDisplayName == "DisplayName2").Should().NotBeNull();
        }

        [Fact]
        public async Task FindByLoginAsync_FindsUserByLogin()
        {
            var store = _userData.GetUserStore();
            var user = await store.FindByIdAsync(_userData.TestUserId, CancellationToken.None);
            var loginInfo = new UserLoginInfo("FindByLoginProvider", "ProviderKey", "DisplayName");
            var loginInfo2 = new UserLoginInfo("FindByLoginProvider", "ProviderKey2", "DisplayName2");

            // Add the test logins
            await store.AddLoginAsync(user, loginInfo, CancellationToken.None);
            await store.AddLoginAsync(user, loginInfo2, CancellationToken.None);

            // Fetch the user
            var foundUser = await store.FindByLoginAsync("FindByLoginProvider", "ProviderKey", CancellationToken.None);

            foundUser.Should().NotBeNull();
            foundUser.Id.Should().Be(user.Id);
        }

        [Fact]
        public async Task AddToRoleAsync_AddsToRole()
        {
            var store = _userData.GetUserStore();
            var user = await store.FindByIdAsync(_userData.TestUserId, CancellationToken.None);

            await store.AddToRoleAsync(user, UserData.TestRoleName, CancellationToken.None);

            var userRole = _userData.GetUserRole(_userData.TestUserId, _userData.TestRoleId);

            userRole.Should().NotBeNull();

            // Cleanup
            await store.RemoveFromRoleAsync(user, UserData.TestRoleName, CancellationToken.None);
        }

        [Fact]
        public async Task RemoveFromRoleAsync_RemovesFromRole()
        {
            var store = _userData.GetUserStore();
            var user = await store.FindByIdAsync(_userData.TestUserId, CancellationToken.None);

            await store.AddToRoleAsync(user, UserData.TestRoleName, CancellationToken.None);

            var userRole = _userData.GetUserRole(_userData.TestUserId, _userData.TestRoleId);

            userRole.Should().NotBeNull();

            await store.RemoveFromRoleAsync(user, UserData.TestRoleName, CancellationToken.None);

            userRole = _userData.GetUserRole(_userData.TestUserId, _userData.TestRoleId);

            userRole.Should().BeNull();
        }

        [Fact]
        public async Task GetRolesAsync_GetsRoles()
        {
            var store = _userData.GetUserStore();
            var user = await store.FindByIdAsync(_userData.TestUserId, CancellationToken.None);

            await store.AddToRoleAsync(user, UserData.TestRoleName, CancellationToken.None);

            var assignedRoles = await store.GetRolesAsync(user, CancellationToken.None);

            assignedRoles.Should().NotBeNullOrEmpty();
            assignedRoles.Should().HaveCount(1);
            assignedRoles.First().Should().Be(UserData.TestRoleName);

            // Cleanup
            await store.RemoveFromRoleAsync(user, UserData.TestRoleName, CancellationToken.None);
        }

        [Fact]
        public async Task IsInRoleAsync_ChecksRoles()
        {
            var store = _userData.GetUserStore();
            var user = await store.FindByIdAsync(_userData.TestUserId, CancellationToken.None);

            await store.AddToRoleAsync(user, UserData.TestRoleName, CancellationToken.None);

            var shouldBeInTestRole = await store.IsInRoleAsync(user, UserData.TestRoleName, CancellationToken.None);
            var shouldNotBeInDummyRole = await store.IsInRoleAsync(user, "DummyRole", CancellationToken.None);

            shouldBeInTestRole.Should().Be(true);
            shouldNotBeInDummyRole.Should().Be(false);

            // Cleanup
            await store.RemoveFromRoleAsync(user, UserData.TestRoleName, CancellationToken.None);
        }

        [Fact]
        public async Task GetUsersInRoleAsync_GetsUsers()
        {
            var store = _userData.GetUserStore();
            var user = await store.FindByIdAsync(_userData.TestUserId, CancellationToken.None);

            await store.AddToRoleAsync(user, UserData.TestRoleName, CancellationToken.None);

            var users = await store.GetUsersInRoleAsync(UserData.TestRoleName, CancellationToken.None);

            users.Should().NotBeNullOrEmpty();
            users.Should().HaveCount(1);
            users.First().Id.Should().Be(_userData.TestUserId);

            // Cleanup
            await store.RemoveFromRoleAsync(user, UserData.TestRoleName, CancellationToken.None);
        }

        [Fact]
        public async Task GetClaimsAsync_GetsClaimsForUser()
        {
            var store = _userData.GetUserStore();
            var user = await store.FindByIdAsync(_userData.TestUserId, CancellationToken.None);

            var claims = await store.GetClaimsAsync(user, CancellationToken.None);

            claims.Should().NotBeNullOrEmpty();
            claims.Where(c => c.Type == UserData.TestClaimType && c.Value == UserData.TestClaimValue).Should().HaveCount(1);
        }

        [Fact]
        public async Task GetUsersForClaimAsync_GetsUsers()
        {
            var store = _userData.GetUserStore();
            var claim = new Claim(UserData.TestClaimType, UserData.TestClaimValue);

            var users = await store.GetUsersForClaimAsync(claim, CancellationToken.None);

            users.Should().NotBeNullOrEmpty();
            users.Should().HaveCount(1);
            users.First().Id.Should().Be(_userData.TestUserId);
        }

        [Fact]
        public async Task RemoveClaimsAsync_RemovesClaim()
        {
            var store = _userData.GetUserStore();
            var claim = new Claim("DummyClaim", "DummyValue");
            var user = await store.FindByIdAsync(_userData.TestUserId, CancellationToken.None);

            await store.AddClaimsAsync(user, new[] { claim }, CancellationToken.None);
            await store.RemoveClaimsAsync(user, new[] { claim }, CancellationToken.None);

            var claims = await store.GetClaimsAsync(user, CancellationToken.None);

            var dummyClaims = claims.Where(c => c.Type == "DummyClaim");
            dummyClaims.Should().BeEmpty();
        }

        [Fact]
        public async Task ReplaceClaimAsync_ReplacesClaim()
        {
            var store = _userData.GetUserStore();
            var claim1 = new Claim("DummyClaim", "DummyValue");
            var claim2 = new Claim("DummyClaim2", "DummyValue2");
            var user = await store.FindByIdAsync(_userData.TestUserId, CancellationToken.None);

            await store.AddClaimsAsync(user, new[] { claim1 }, CancellationToken.None);
            await store.ReplaceClaimAsync(user, claim1, claim2, CancellationToken.None);

            var claims = await store.GetClaimsAsync(user, CancellationToken.None);

            claims.FirstOrDefault(c => c.Type == "DummyClaim" && c.Value == "DummyValue").Should().BeNull();
            claims.FirstOrDefault(c => c.Type == "DummyClaim2" && c.Value == "DummyValue2").Should().NotBeNull();

            // Cleanup
            await store.RemoveClaimsAsync(user, claims.Where(c => c.Type == "DummyClaim"), CancellationToken.None);
        }

        [Fact]
        public async Task AddClaimsAsync_AddsClaim()
        {
            var store = _userData.GetUserStore();
            var claim = new Claim("DummyClaim", "DummyValue");
            var user = await store.FindByIdAsync(_userData.TestUserId, CancellationToken.None);

            await store.AddClaimsAsync(user, new[] { claim }, CancellationToken.None);

            var claims = await store.GetClaimsAsync(user, CancellationToken.None);

            claims.Should().NotBeNullOrEmpty();
            var addedClaims = claims.Where(c => c.Type == "DummyClaim" && c.Value == "DummyValue");
            addedClaims.Should().HaveCount(1);

            // Cleanup
            await store.RemoveClaimsAsync(user, addedClaims, CancellationToken.None);
        }

        [Fact]
        public async Task SetPasswordHashAsync_SetsPassword()
        {
            var store = _userData.GetUserStore();
            var user = new IdentityUser() { PasswordHash = "Wrong" };

            await store.SetPasswordHashAsync(user, "Correct", CancellationToken.None);

            user.PasswordHash.Should().Be("Correct");
        }

        [Fact]
        public async Task GetPasswordHashAsync_GetsPasswordHash()
        {
            var store = _userData.GetUserStore();
            var user = new IdentityUser() { PasswordHash = "PassHash" };

            var hash = await store.GetPasswordHashAsync(user, CancellationToken.None);

            hash.Should().Be("PassHash");
        }

        [Fact]
        public async Task HasPasswordAsync_ChecksHash()
        {
            var store = _userData.GetUserStore();
            var hasHashUser = new IdentityUser() { PasswordHash = "PassHash" };
            var noHashUser = new IdentityUser();

            var hasHash = await store.HasPasswordAsync(hasHashUser, CancellationToken.None);
            var noHash = await store.HasPasswordAsync(noHashUser, CancellationToken.None);

            hasHash.Should().BeTrue();
            noHash.Should().BeFalse();
        }

        [Fact]
        public async Task SetSecurityStampAsync_SetsSecurityStamp()
        {
            var store = _userData.GetUserStore();
            var user = new IdentityUser();

            await store.SetSecurityStampAsync(user, "SecStamp", CancellationToken.None);

            user.SecurityStamp.Should().Be("SecStamp");
        }

        [Fact]
        public async Task GetSecurityStampAsync_GetsSecurityStamp()
        {
            var store = _userData.GetUserStore();
            var user = new IdentityUser() { SecurityStamp = "SecStamp" };

            var stamp = await store.GetSecurityStampAsync(user, CancellationToken.None);

            stamp.Should().Be("SecStamp");
        }

        [Fact]
        public async Task SetEmailAsync_SetsEmail()
        {
            var store = _userData.GetUserStore();
            var user = new IdentityUser();

            await store.SetEmailAsync(user, "testemail@test.com", CancellationToken.None);

            user.Email.Should().Be("testemail@test.com");
        }

        [Fact]
        public async Task GetEmailAsync_GetsEmail()
        {
            var store = _userData.GetUserStore();
            var user = new IdentityUser() { Email = "myemail@test.com" };

            var email = await store.GetEmailAsync(user, CancellationToken.None);

            email.Should().Be("myemail@test.com");
        }

        [Fact]
        public async Task GetEmailConfirmedAsync_GetsEmailConfirmed()
        {
            var store = _userData.GetUserStore();
            var confirmedUser = new IdentityUser() { EmailConfirmed = true };
            var nonConfirmedUser = new IdentityUser() { EmailConfirmed = false };

            var confirmed = await store.GetEmailConfirmedAsync(confirmedUser, CancellationToken.None);
            var notConfirmed = await store.GetEmailConfirmedAsync(nonConfirmedUser, CancellationToken.None);

            confirmed.Should().BeTrue();
            notConfirmed.Should().BeFalse();
        }

        [Fact]
        public async Task SetEmailConfirmedAsync_SetsEmailConfirmed()
        {
            var store = _userData.GetUserStore();
            var confirmedUser = new IdentityUser();
            var nonConfirmedUser = new IdentityUser();

            await store.SetEmailConfirmedAsync(confirmedUser, true, CancellationToken.None);
            await store.SetEmailConfirmedAsync(nonConfirmedUser, false, CancellationToken.None);

            confirmedUser.EmailConfirmed.Should().BeTrue();
            nonConfirmedUser.EmailConfirmed.Should().BeFalse();
        }

        [Fact]
        public async Task FindByEmailAsync_FindsUser()
        {
            var store = _userData.GetUserStore();

            var user = await store.FindByEmailAsync(UserData.TestUserEmail, CancellationToken.None);

            user.Should().NotBeNull();
            user.Id.Should().Be(_userData.TestUserId);
        }
        
        [Fact]
        public async Task GetNormalizedEmailAsync_GetsNormalizedEmail()
        {
            var store = _userData.GetUserStore();
            var user = new IdentityUser() { NormalizedEmail = "myemail@email.com" };

            var email = await store.GetNormalizedEmailAsync(user, CancellationToken.None);

            email.Should().Be("myemail@email.com");
        }

        [Fact]
        public async Task SetNormalizedEmailAsync_SetsNormalizedEmail()
        {
            var store = _userData.GetUserStore();
            var user = new IdentityUser();

            await store.SetNormalizedEmailAsync(user, "hello@email.com", CancellationToken.None);

            user.NormalizedEmail.Should().Be("hello@email.com");
        }

        [Fact]
        public async Task GetLockoutEndDateAsync_GetsLockoutEndDate()
        {
            var end = new DateTimeOffset(DateTime.Now.AddDays(1));
            var store = _userData.GetUserStore();
            var user = new IdentityUser() { LockoutEnd = end };

            var foundEnd = await store.GetLockoutEndDateAsync(user, CancellationToken.None);

            foundEnd.Should().Be(end);
        }

        [Fact]
        public async Task SetLockoutEndDateAsync_SetsLockoutEndDate()
        {
            var end = new DateTimeOffset(DateTime.Now.AddDays(1));
            var store = _userData.GetUserStore();
            var user = new IdentityUser();

            await store.SetLockoutEndDateAsync(user, end, CancellationToken.None);

            user.LockoutEnd.Should().Be(end);
        }

        [Fact]
        public async Task IncrementAccessFailedCountAsync_IncrementsAccessFailedCount()
        {
            var store = _userData.GetUserStore();
            var user = new IdentityUser() { AccessFailedCount = 1 };

            await store.IncrementAccessFailedCountAsync(user, CancellationToken.None);

            user.AccessFailedCount.Should().Be(2);
        }

        [Fact]
        public async Task ResetAccessFailedCountAsync_ResetsAccessFailedCount()
        {
            var store = _userData.GetUserStore();
            var user = new IdentityUser() { AccessFailedCount = 1 };

            await store.ResetAccessFailedCountAsync(user, CancellationToken.None);

            user.AccessFailedCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAccessFailedCountAsync_GetsAccessFailedCount()
        {
            var store = _userData.GetUserStore();
            var user = new IdentityUser() { AccessFailedCount = 1 };

            var failedCount = await store.GetAccessFailedCountAsync(user, CancellationToken.None);

            failedCount.Should().Be(1);
        }

        [Fact]
        public async Task GetLockoutEnabledAsync_GetsLockoutEnabled()
        {
            var store = _userData.GetUserStore();
            var lockedOutUser = new IdentityUser() { LockoutEnabled = true };
            var notLockedOutUser = new IdentityUser() { LockoutEnabled = false };

            var lockedOut = await store.GetLockoutEnabledAsync(lockedOutUser, CancellationToken.None);
            var notLockedOut = await store.GetLockoutEnabledAsync(notLockedOutUser, CancellationToken.None);

            lockedOut.Should().BeTrue();
            notLockedOut.Should().BeFalse();
        }

        [Fact]
        public async Task SetLockoutEnabledAsync_SetsLockedOut()
        {
            var store = _userData.GetUserStore();
            var user = new IdentityUser() { LockoutEnabled = false };

            await store.SetLockoutEnabledAsync(user, true, CancellationToken.None);

            user.LockoutEnabled.Should().BeTrue();
        }

        [Fact]
        public async Task SetPhoneNumberAsync_SetsPhoneNumber()
        {
            var store = _userData.GetUserStore();
            var user = new IdentityUser() { PhoneNumber = "Wrong" };

            await store.SetPhoneNumberAsync(user, "Correct", CancellationToken.None);

            user.PhoneNumber.Should().Be("Correct");
        }

        [Fact]
        public async Task GetPhoneNumberAsync_GetsPhoneNumber()
        {
            var store = _userData.GetUserStore();
            var user = new IdentityUser() { PhoneNumber = "PhoneNum" };

            var num = await store.GetPhoneNumberAsync(user, CancellationToken.None);

            num.Should().Be("PhoneNum");
        }

        [Fact]
        public async Task GetPhoneNumberConfirmedAsync_GetsPhoneNumberConfirmed()
        {
            var store = _userData.GetUserStore();
            var confirmedUser = new IdentityUser() { PhoneNumberConfirmed = true };
            var notConfirmedUser = new IdentityUser() { PhoneNumberConfirmed = false };

            var confirmed = await store.GetPhoneNumberConfirmedAsync(confirmedUser, CancellationToken.None);
            var notConfirmed = await store.GetPhoneNumberConfirmedAsync(notConfirmedUser, CancellationToken.None);

            confirmed.Should().BeTrue();
            notConfirmed.Should().BeFalse();
        }

        [Fact]
        public async Task SetPhoneNumberConfirmedAsync_SetsPhoneNumberConfirmed()
        {
            var store = _userData.GetUserStore();
            var user = new IdentityUser() { PhoneNumberConfirmed = false };

            await store.SetPhoneNumberConfirmedAsync(user, true, CancellationToken.None);

            user.PhoneNumberConfirmed.Should().BeTrue();
        }

        [Fact]
        public async Task SetTwoFactorEnabledAsync_SetsTwoFactorEnabled()
        {
            var store = _userData.GetUserStore();
            var user = new IdentityUser() { TwoFactorEnabled = false };

            await store.SetTwoFactorEnabledAsync(user, true, CancellationToken.None);

            user.TwoFactorEnabled.Should().BeTrue();
        }

        [Fact]
        public async Task GetTwoFactorEnabledAsync_GetsTwoFactorEnabled()
        {
            var store = _userData.GetUserStore();
            var enabledUser = new IdentityUser() { TwoFactorEnabled = true };
            var disabledUser = new IdentityUser() { TwoFactorEnabled = false };

            var enabled = await store.GetTwoFactorEnabledAsync(enabledUser, CancellationToken.None);
            var disabled = await store.GetTwoFactorEnabledAsync(disabledUser, CancellationToken.None);

            enabled.Should().BeTrue();
            disabled.Should().BeFalse();
        }
    }
}
