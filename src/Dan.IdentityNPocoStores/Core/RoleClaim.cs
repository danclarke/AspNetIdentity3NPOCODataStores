using System.Security.Claims;
using NPoco;

namespace Dan.IdentityNPocoStores.Core
{
    /// <summary>
    /// Represents a claim a role has
    /// </summary>
    [TableName("AspNetRoleClaims")]
    [PrimaryKey("Id")]
    public class RoleClaim
    {
        /// <summary>
        /// Unique ID / primary key for claim
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// Claim type
        /// </summary>
        public virtual string ClaimType { get; set; }

        /// <summary>
        /// Claim value
        /// </summary>
        public virtual string ClaimValue { get; set; }

        /// <summary>
        /// Role ID of role that owns this claim
        /// </summary>
        public virtual string RoleId { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public RoleClaim() { }

        /// <summary>
        /// Create a new RoleClaim that matches the specified claim and for the specified role
        /// </summary>
        /// <param name="claim">Claim</param>
        /// <param name="roleId">ID of owning role</param>
        public RoleClaim(Claim claim, string roleId)
        {
            RoleId = roleId;
            ClaimType = claim.Type;
            ClaimValue = claim.Value;
        }

        /// <summary>
        /// Get a System.Security claim that represents this claim
        /// </summary>
        /// <returns>Claim</returns>
        public virtual Claim ToClaim()
        {
            return new Claim(ClaimType, ClaimValue);
        }
    }
}
