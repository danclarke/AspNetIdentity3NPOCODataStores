using Microsoft.AspNet.Identity;
using NPoco;

namespace Dan.IdentityNPocoStores.Core
{
    /// <summary>
    /// Entity that represents a user login
    /// </summary>
    [TableName("AspNetUserLogins")]
    [PrimaryKey("LoginProvider,ProviderKey", AutoIncrement = false)]
    public class UserLogin
    {
        /// <summary>
        /// The login provider for the login (i.e. facebook, google)
        /// </summary>
        public virtual string LoginProvider { get; set; }

        /// <summary>
        /// Key representing the login for the provider
        /// </summary>
        public virtual string ProviderKey { get; set; }

        /// <summary>
        /// Display name for the login
        /// </summary>
        public virtual string ProviderDisplayName { get; set; }

        /// <summary>
        /// User Id for the user who owns this login
        /// </summary>
        public virtual string UserId { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public UserLogin() {}

        /// <summary>
        /// Create a new UserLogin for the specified info and user
        /// </summary>
        /// <param name="loginInfo">Information about the login</param>
        /// <param name="userId">ID of the user the login is for</param>
        public UserLogin(UserLoginInfo loginInfo, string userId)
        {
            LoginProvider = loginInfo.LoginProvider;
            ProviderDisplayName = loginInfo.ProviderDisplayName;
            ProviderKey = loginInfo.ProviderKey;
            UserId = userId;
        }

        /// <summary>
        /// Get a Identity UserLoginInfo that represents the data in this entity
        /// </summary>
        /// <returns>Identity UserLoginInfo</returns>
        public virtual UserLoginInfo ToLoginInfo()
        {
            return new UserLoginInfo(LoginProvider, ProviderKey, ProviderDisplayName);
        }
    }

}
