using System;
using System.Collections.Generic;
using NPoco;

namespace Dan.IdentityNPocoStores.Core
{
    /// <summary>
    /// Represents a User entity
    /// </summary>
    [TableName("AspNetUsers")]
    [PrimaryKey("Id", AutoIncrement = false)]
    public class IdentityUser
    { 
        /// <summary>
        /// A random value that should change whenever a user is persisted to the store
        /// </summary>
        public virtual string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

        public virtual string Id { get; set; }

        public virtual string UserName { get; set; }

        public virtual string NormalizedUserName { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        public virtual string Email { get; set; }

        public virtual string NormalizedEmail { get; set; }

        /// <summary>
        /// True if the email is confirmed, default is false
        /// </summary>
        public virtual bool EmailConfirmed { get; set; }

        /// <summary>
        /// The salted/hashed form of the user password
        /// </summary>
        public virtual string PasswordHash { get; set; }

        /// <summary>
        /// A random value that should change whenever a users credentials change (password changed, login removed)
        /// </summary>
        public virtual string SecurityStamp { get; set; }

        /// <summary>
        /// Phone number for the user
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// True if the phone number is confirmed, default is false
        /// </summary>
        public virtual bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// Is two factor enabled for the user
        /// </summary>
        public virtual bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// DateTime in UTC when lockout ends, any time in the past is considered not locked out.
        /// </summary>
        public virtual DateTimeOffset? LockoutEnd { get; set; }

        /// <summary>
        /// Is lockout enabled for this user
        /// </summary>
        public virtual bool LockoutEnabled { get; set; }

        /// <summary>
        /// Used to record failures for the purposes of lockout
        /// </summary>
        public virtual int AccessFailedCount { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public IdentityUser()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userName">Initial username</param>
        public IdentityUser(string userName)
          : this()
        {
            UserName = userName;
        }

        /// <summary>
        /// Returns a friendly name
        /// </summary>
        /// <returns/>
        public override string ToString()
        {
            return this.UserName;
        }
    }
}
