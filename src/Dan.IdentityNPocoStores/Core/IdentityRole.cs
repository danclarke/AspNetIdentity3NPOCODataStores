using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NPoco;

namespace Dan.IdentityNPocoStores.Core
{
    /// <summary>
    /// Represents a Role entity
    /// </summary>
    [TableName("AspNetRoles")]
    [PrimaryKey("Id", AutoIncrement = false)]
    public class IdentityRole
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public IdentityRole()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="roleName">Initial name for role</param>
        public IdentityRole(string roleName) : this()
        {
            Name = roleName;
        }

        /// <summary>
        /// Role id
        /// </summary>
        public virtual string Id { get; set; }

        /// <summary>
        /// Role name
        /// </summary>
        public virtual string Name { get; set; }
        public virtual string NormalizedName { get; set; }

        /// <summary>
        /// A random value that should change whenever a role is persisted to the store
        /// </summary>
        public virtual string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Returns a friendly name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
