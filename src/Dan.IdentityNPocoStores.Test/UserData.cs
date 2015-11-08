using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dan.IdentityNPocoStores.Core;
using NPoco;
using Dan.IdentityNPocoStores.Test.Util;

namespace Dan.IdentityNPocoStores.Test
{
    /// <summary>
    /// Setup the DB for testing the storage classes
    /// </summary>
    public class UserData : IDisposable
    {
        private const string ConnectionString = "Server=(local);Initial Catalog=IdentityNPocoStoresUserStoreTest;Integrated Security=SSPI;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string ProviderName = "System.Data.SqlClient";

        public const string TestRoleName = "Test Role";
        public const string TestUserName = "Test User";
        public const string TestUserEmail = "test@test.com";
        public const string TestUserPassword = "Test123*";

        public const string TestClaimType = "Test ClaimType";
        public const string TestClaimValue = "Test ClaimValue";

        private string _testUserId;
        public string TestUserId => _testUserId;

        private string _testRoleId;
        public string TestRoleId => _testRoleId;

        public UserData()
        {
            // Set up DB
            CreateDb();
            CreateDummyUsers();
        }

        /// <summary>
        /// Get a UserStore for testing
        /// </summary>
        /// <returns>UserStore with access to DB</returns>
        public UserStore<IdentityUser, IdentityRole, UserClaim, UserLogin, UserRole> GetUserStore()
        {
            return new UserStore<IdentityUser, IdentityRole, UserClaim, UserLogin, UserRole>(ConnectionString, ProviderName);
        }

        /// <summary>
        /// Get a RoleStore for testing
        /// </summary>
        /// <returns>RoleStore with access to DB</returns>
        public RoleStore<IdentityRole> GetRoleStore()
        {
            return new RoleStore<IdentityRole>(ConnectionString, ProviderName);
        }

        /// <summary>
        /// Get a specific user directly from the DB using ADO.NET rather than NPOCO. Ideal for checking data matches between store/NPOCO and the actual DB
        /// </summary>
        /// <param name="id">ID of user</param>
        /// <returns>User or NULL</returns>
        public IdentityUser GetUser(string id)
        {
            IdentityUser user = null;

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT * FROM AspNetUsers WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int col = 0;
                        user = new IdentityUser();

                        // Read in user data
                        user.Id = reader.GetString(col++);
                        user.AccessFailedCount = reader.GetInt32(col++);
                        user.ConcurrencyStamp = reader.GetStringSafe(col++);
                        user.Email = reader.GetStringSafe(col++);
                        user.EmailConfirmed = reader.GetBoolean(col++);
                        user.LockoutEnabled = reader.GetBoolean(col++);
                        user.LockoutEnd = reader.GetDateTimeOffsetSafe(col++);
                        user.NormalizedEmail = reader.GetStringSafe(col++);
                        user.NormalizedUserName = reader.GetStringSafe(col++);
                        user.PasswordHash = reader.GetStringSafe(col++);
                        user.PhoneNumber = reader.GetStringSafe(col++);
                        user.PhoneNumberConfirmed = reader.GetBoolean(col++);
                        user.SecurityStamp = reader.GetStringSafe(col++);
                        user.TwoFactorEnabled = reader.GetBoolean(col++);
                        user.UserName = reader.GetStringSafe(col++);
                    }
                }

                conn.Close();
            }

            return user;
        }

        /// <summary>
        /// Get a specific UserLogin directly fromn the DB using ADO.NET rather than NPOCO.
        /// </summary>
        /// <param name="loginProvider">Login provider</param>
        /// <param name="providerKey">Provider key</param>
        /// <param name="userId">User ID</param>
        /// <returns>UserLogin or NULL</returns>
        public UserLogin GetUserLogin(string loginProvider, string providerKey, string userId)
        {
            UserLogin userLogin = null;

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT * FROM AspNetUserLogins WHERE LoginProvider = @provider AND ProviderKey = @providerKey AND UserId = @userId", conn))
            {
                cmd.Parameters.AddWithValue("@provider", loginProvider);
                cmd.Parameters.AddWithValue("@providerKey", providerKey);
                cmd.Parameters.AddWithValue("@userId", userId);

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int col = 0;
                        userLogin = new UserLogin();

                        // Read in UserLogin data
                        userLogin.LoginProvider = reader.GetString(col++);
                        userLogin.ProviderKey = reader.GetString(col++);
                        userLogin.ProviderDisplayName = reader.GetStringSafe(col++);
                        userLogin.UserId = reader.GetStringSafe(col++);
                    }
                }

                conn.Close();
            }

            return userLogin;
        }

        /// <summary>
        /// Get a specific UserRole directly from the DB using ADO.NET rather than NPOCO. Ideal for checking data matches between store/NPOCO and the actual DB
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="roleId">ID of role</param>
        /// <returns>UserRole or NULL</returns>
        public UserRole GetUserRole(string userId, string roleId)
        {
            UserRole userRole = null;

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT * FROM AspNetUserRoles WHERE UserId = @userId AND RoleId = @roleId", conn))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@roleId", roleId);

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int col = 0;
                        userRole = new UserRole();

                        // Read in UserLogin data
                        userRole.UserId = reader.GetString(col++);
                        userRole.RoleId = reader.GetString(col++);
                    }
                }

                conn.Close();
            }

            return userRole;
        }

        /// <summary>
        /// Get a specific role directly from the DB using ADO.NET rather than NPOCO. Ideal for checking data matches between store/NPOCO and the actual DB
        /// </summary>
        /// <param name="id">ID of role</param>
        /// <returns>Role or NULL</returns>
        public IdentityRole GetRole(string id)
        {
            IdentityRole role = null;

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT * FROM AspNetRoles WHERE Id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int col = 0;
                        role = new IdentityRole();

                        // Read in role data
                        role.Id = reader.GetString(col++);
                        role.ConcurrencyStamp = reader.GetStringSafe(col++);
                        role.Name = reader.GetStringSafe(col++);
                        role.NormalizedName = reader.GetStringSafe(col++);
                    }
                }

                conn.Close();
            }

            return role;
        }

        /// <summary>
        /// Create the neccesary Identity tables in the DB
        /// </summary>
        private static void CreateDb()
        {
            var insertSql = File.ReadAllText("Resources/AspNetIdentity.sql");

            RunSqlScript(insertSql);
        }

        /// <summary>
        /// Drop everything from DB
        /// </summary>
        private static void CleanupDb()
        {
            var cleanupSql = File.ReadAllText("Resources/AspNetIdentityDrop.sql");

            RunSqlScript(cleanupSql);
        }

        /// <summary>
        /// Adds the standard dummy users to the DB, uses NPOCO to save
        /// </summary>
        private void CreateDummyUsers()
        {
            var user = new IdentityUser(TestUserName)
            {
                NormalizedUserName = TestUserName,
                Email = TestUserEmail,
                NormalizedEmail = TestUserEmail,
                PasswordHash = TestUserPassword
            };

            var user2 = new IdentityUser("Dont Use Me")
            {
                NormalizedUserName = "Dont Use Me",
                Email = "im@not.here",
                NormalizedEmail = "im@not.here",
                PasswordHash = "goaway"
            };

            var dummyLogin = new UserLogin()
            {
                ProviderKey = "Dummy",
                LoginProvider = "LoginProvider",
                ProviderDisplayName = "DummyDontUse",
                UserId = user2.Id
            };

            var role = new IdentityRole(TestRoleName)
            {
                NormalizedName = TestRoleName
            };

            var testClaim = new UserClaim
            {
                ClaimType = TestClaimType,
                ClaimValue = TestClaimValue,
                UserId = user.Id
            };

            using (var db = new Database(ConnectionString, ProviderName))
            {
                db.Insert(user);
                db.Insert(user2);
                db.Insert(dummyLogin);
                db.Insert(role);
                db.Insert(testClaim);
            }

            _testUserId = user.Id;
            _testRoleId = role.Id;
        }

        /// <summary>
        /// Run a SQL script on the DB
        /// </summary>
        /// <param name="script">Script to execute</param>
        private static void RunSqlScript(string script)
        {
            // Bit naughty
            script = script.Replace("GO", "");

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(script, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();

                conn.Close();
            }
        }

        public void Dispose()
        {
            // Clear everything from DB
            CleanupDb();
        }
    }
}
