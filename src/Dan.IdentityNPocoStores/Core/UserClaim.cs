using System.Security.Claims;
using NPoco;

namespace Dan.IdentityNPocoStores.Core
{
    /// <summary>
    /// EntityType that represents one specific user claim
    /// </summary>
    [TableName("AspNetUserClaims")]
    [PrimaryKey("Id")]
    public class UserClaim
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// User Id for the user who owns this claim
        /// </summary>
        public virtual string UserId { get; set; }

        /// <summary>
        /// Claim type
        /// </summary>
        public virtual string ClaimType { get; set; }

        /// <summary>
        /// Claim value
        /// </summary>
        public virtual string ClaimValue { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public UserClaim() { }

        /// <summary>
        /// Create a new UserClaim that matches the specified claim and for the specified user
        /// </summary>
        /// <param name="claim">Claim</param>
        /// <param name="userId">ID of owning user</param>
        public UserClaim(Claim claim, string userId)
        {
            UserId = userId;
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
