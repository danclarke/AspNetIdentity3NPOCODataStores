using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Dan.IdentityNPocoStores.Core;
using Microsoft.AspNet.Identity;
using Microsoft.Framework.Configuration;
using NPoco;

namespace Dan.IdentityNPocoStores
{
    /// <summary>
    /// ASP.NET RoleStore using NPOCO
    /// </summary>
    /// <typeparam name="TRole">Type of role stored in the DB</typeparam>
    public class RoleStore<TRole> : IRoleClaimStore<TRole>
        where TRole : IdentityRole
    {
        private readonly string _connectionString;
        private readonly string _providerName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Configuration containing DB connection string</param>
        public RoleStore(IConfigurationRoot config)
        {
            _connectionString = config["Data:DefaultConnection:ConnectionString"];
            _providerName = "System.Data.SqlClient";
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString">Connection string for DB</param>
        /// <param name="providerName">Underlying provider to use for DB communication</param>
        public RoleStore(string connectionString, string providerName)
        {
            _connectionString = connectionString;
            _providerName = providerName;
        }

        public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));

            using (var db = GetDatabase())
                await db.InsertAsync(role);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));

            using (var db = GetDatabase())
                await db.UpdateAsync(role);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));

            using (var db = GetDatabase())
                await db.DeleteAsync(role);

            return IdentityResult.Success;
        }

        public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.Id);
        }

        public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.Name);
        }

        public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentOutOfRangeException(nameof(roleName));

            role.Name = roleName;

            return Task.FromResult(0);
        }

        public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.NormalizedName);
        }

        public Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));
            if (string.IsNullOrWhiteSpace(normalizedName))
                throw new ArgumentOutOfRangeException(nameof(normalizedName));

            role.NormalizedName = normalizedName;

            return Task.FromResult(0);
        }

        public Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentOutOfRangeException(nameof(roleId));

            using (var db = GetDatabase())
            {
                var result = db.FirstOrDefault<TRole>("WHERE Id = @0", roleId);
                return Task.FromResult(result);
            }
        }

        public Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(normalizedRoleName))
                throw new ArgumentOutOfRangeException(nameof(normalizedRoleName));

            using (var db = GetDatabase())
            {
                var result = db.FirstOrDefault<TRole>("WHERE NormalizedName = @0", normalizedRoleName);
                return Task.FromResult(result);
            }
        }

        public async Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));

            IList<RoleClaim> claims;

            using (var db = GetDatabase())
                claims = await db.FetchAsync<RoleClaim>("WHERE RoleId = @0", role.Id);

            cancellationToken.ThrowIfCancellationRequested();

            return claims.Select(c => c.ToClaim()).ToArray();
        }

        public async Task AddClaimAsync(TRole role, Claim claim,
            CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            var dbClaim = new RoleClaim(claim, role.Id);

            using (var db = GetDatabase())
                await db.InsertAsync(dbClaim);
        }

        public Task RemoveClaimAsync(TRole role, Claim claim,
            CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
                throw new ArgumentNullException(nameof(role));
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            using (var db = GetDatabase())
            {
                db.DeleteWhere<RoleClaim>("RoleId = @0 AND ClaimType = @1 AND ClaimValue = @2",
                    role.Id, claim.Type, claim.Value);
            }

            return Task.FromResult(0);
        }

        #region Utility

        /// <summary>
        /// Get the database to use for data operations
        /// </summary>
        /// <returns>Database to use</returns>
        protected virtual IDatabase GetDatabase()
        {
            return new Database(_connectionString, _providerName);
        }

        #endregion

        #region Disposal

        ~RoleStore()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Unused
        }

        #endregion
    }
}
