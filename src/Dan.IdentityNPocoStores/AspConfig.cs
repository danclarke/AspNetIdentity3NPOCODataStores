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
        /// <summary>
        /// Add NPOCO stores to ASP.NET Identity framework
        /// </summary>
        /// <typeparam name="TUser">Type of user stored in DB</typeparam>
        /// <typeparam name="TRole">Type of role stored in DB</typeparam>
        /// <param name="builder">IdentityBuilder to configure ASP.NET Identity</param>
        /// <returns>IdentityBuilder for chaining</returns>
        public static IdentityBuilder AddNPocoStores<TUser, TRole>(this IdentityBuilder builder)
            where TUser : IdentityUser
            where TRole : IdentityRole
        {
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
        /// <returns>IdentityBuilder for chaining</returns>
        public static IdentityBuilder AddNPocoStores<TUser, TRole, TUserClaim, TUserLogin, TUserRole>(
            this IdentityBuilder builder)
            where TUser : IdentityUser
            where TRole : IdentityRole
            where TUserClaim : UserClaim
            where TUserLogin : UserLogin
            where TUserRole : UserRole
        {
            return builder
                .AddUserStore<UserStore<TUser, TRole, TUserClaim, TUserLogin, TUserRole>>()
                .AddRoleStore<RoleStore<TRole>>();
        }
    }
}
