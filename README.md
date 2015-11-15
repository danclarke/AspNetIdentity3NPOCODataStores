#ASP.NET Identity 3 - NPOCO Based Stores
The default data providers for ASP.NET Identity 3 use EntityFramework to perform the heavy lifting - this is slow, puts a hard EF requirement on your solution and is generally sucky. This solution provides data providers that use NPOCO to perform the data interaction instead of EF, giving a substantial performance increase as well as memory footprint reduction.

By default my solution uses the standard ASP.NET Identity database schema. If you are already using EF stores you can transition to this NPOCO solution very easily by just replacing the EF project with the NPOCO project and updating a few config options.

## Performance

It's no secret that EF is slow as hell, but just how slow is it with ASP.NET Identity 3? Let's find out...

## The Test
### Insert Test
- Insert 1 Role
- Insert 1000 Users
- Assign each user to the role

### Delete Test
- Remove the user from the role
- Delete the users
- Delete the role

Test code is available in the two web projects in the `TestController`.

Ran on:
- Intel i7 6700K @ 4.6Ghz
- 16GB DDR4 3200 RAM
- 512GB Samsung 951 PCI-E SSD (AHCI)
- SQL Server 2014 Web Edition

## The Results
### Entity Framework
- Insert time: 31,692ms
- Delete time: 24,416ms

### NPOCO
- Insert time: 10,520ms (**301%** faster)
- Delete time: 2,149ms (**1,136%** faster)

## Current Limitations

- You have to create the database schema yourself

## Installation

- Create a new ASP.NET 5 solution and link Dan.IdentityNPocoStores into your web project in your preferred manner.
- If you don't already have a database with Identity set up, run the 'AspNetIdentity.sql' (Dan.IdentityNPocoStores.Test/Resources) script on your DB.
- In Startup.cs remove all references to the EF Identity implementation
- Replace the `.AddEntityFrameworkStores<ApplicationDbContext>()` line with `.AddNPocoStores<IdentityUser, IdentityRole>("connectionString", "providerName")` where 'IdentityUser' and 'IdentityRole' are your User & Role classes respectively
- Ensure your custom User & Role classes inherit from IdentityUser and IdentityRole in the NPOCO project

Look at the two web projects in the code to see a working implementation.