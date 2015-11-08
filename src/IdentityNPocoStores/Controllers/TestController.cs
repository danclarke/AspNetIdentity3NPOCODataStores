using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Identity;
using Dan.IdentityNPocoStores.Core;
using System.Diagnostics;

namespace IdentityNPocoStores.Controllers
{
    public class TestController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TestController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var stopWatch = new Stopwatch();
            long createTime, deleteTime;

            stopWatch.Start();

            var role = new IdentityRole("TestRole");
            var users = new IdentityUser[1000];

            await _roleManager.CreateAsync(role);

            for (int i = 0; i < 1000; ++i)
            {
                var user = new IdentityUser
                {
                    UserName = "Test" + i,
                    Email = $"test{i}@test.com",
                };

                users[i] = user;
                await _userManager.CreateAsync(user, "Password123*");
                await _userManager.AddToRoleAsync(user, "TestRole");
            }

            stopWatch.Stop();
            createTime = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();
            stopWatch.Start();

            for (int i = 0; i < 1000; ++i)
            {
                var user = users[i];
                await _userManager.RemoveFromRoleAsync(user, "TestRole");
                await _userManager.DeleteAsync(user);
            }
            await _roleManager.DeleteAsync(role);

            stopWatch.Stop();
            deleteTime = stopWatch.ElapsedMilliseconds;

            var response = $"Create time: {createTime}ms\r\nDelete Time: {deleteTime}ms";

            return Content(response);
        }
    }
}
