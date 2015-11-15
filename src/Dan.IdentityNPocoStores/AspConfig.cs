using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dan.IdentityNPocoStores.Core;
using Microsoft.AspNet.Identity;

namespace Dan.IdentityNPocoStores
{
    /// <summary>
    /// Configuration helper for ASP.NET 5
    /// </summary>
    public static class AspConfig
    {
        internal static string _connectionString, _providerName;

        /// <summary>
        /// Get the connection string set by the user during config
        /// </summary>
        /// <returns>Connection string</returns>
        internal static string GetConnectionString()
        {
            return _connectionString;
        }

        /// <summary>
        /// Get the provider name set by the user during config
        /// </summary>
        /// <returns>Provider name</returns>
        internal static string GetProviderName()
        {
            return _providerName;
        }

        /// <summary>
        /// Add NPOCO stores to ASP.NET Identity framework
        /// </summary>
        /// <typeparam name="TUser">Type of user stored in DB</typeparam>
        /// <typeparam name="TRole">Type of role stored in DB</typeparam>
        /// <param name="builder">IdentityBuilder to configure ASP.NET Identity</param>
        /// <param name="connectionString">Connection string to use to connect to DB</param>
        /// <param name="providerName">Name of DB provider eg. System.Data.SqlClient</param>
        /// <returns>IdentityBuilder for chaining</returns>
        public static IdentityBuilder AddNPocoStores<TUser, TRole>(this IdentityBuilder builder, string connectionString, string providerName)
            where TUser : IdentityUser
            where TRole : IdentityRole
        {
            _connectionString = connectionString;
            _providerName = providerName;

            return builder
                .AddUserStore<UserStore<TUser, TRole, UserClaim, UserLogin, UserRole>>()
                .AddRoleStore<RoleStore<TRole>>();
        }

        /// <summary>
        /// Add NPOCO stores to ASP.NET Identity framework
        /// </summary>
        /// <typeparam name="TUser">Type of user stored in DB</typeparam>
        /// <typeparam name="TRole">Type of role stored in DB</typeparam>
        /// <typeparam name="TUserClaim">Type of claim stored in DB</typeparam>
        /// <typeparam name="TUserLogin">Type of user login stored in DB</typeparam>
        /// <typeparam name="TUserRole">Type of user role stored in DB</typeparam>
        /// <param name="builder">IdentityBuilder to configure ASP.NET Identity</param>
        /// <param name="connectionString">Connection string to use to connect to DB</param>
        /// <param name="providerName">Name of DB provider eg. System.Data.SqlClient</param>
        /// <returns>IdentityBuilder for chaining</returns>
        public static IdentityBuilder AddNPocoStores<TUser, TRole, TUserClaim, TUserLogin, TUserRole>(
            this IdentityBuilder builder, string connectionString, string providerName)
            where TUser : IdentityUser
            where TRole : IdentityRole
            where TUserClaim : UserClaim
            where TUserLogin : UserLogin
            where TUserRole : UserRole
        {
            _connectionString = connectionString;
            _providerName = providerName;

            return builder
                .AddUserStore<UserStore<TUser, TRole, TUserClaim, TUserLogin, TUserRole>>()
                .AddRoleStore<RoleStore<TRole>>();
        }
    }
}
