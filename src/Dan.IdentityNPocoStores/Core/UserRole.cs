using NPoco;

namespace Dan.IdentityNPocoStores.Core
{
    /// <summary>
    /// Entity that matches a user to a role
    /// </summary>
    [TableName("AspNetUserRoles")]
    [PrimaryKey("UserId,RoleId", AutoIncrement = false)]
    public class UserRole
    {
        /// <summary>
        /// Id of the user
        /// </summary>
        public virtual string UserId { get; set; }

        /// <summary>
        /// Id of the role
        /// </summary>
        public virtual string RoleId { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public UserRole() { }

        /// <summary>
        /// Create a new UserRole for the specified user and role
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="roleId">ID of the role</param>
        public UserRole(string userId, string roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }
    }
}
