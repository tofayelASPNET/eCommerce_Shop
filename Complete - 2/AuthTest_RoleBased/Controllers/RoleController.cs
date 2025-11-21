using AuthTest_RoleBased.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthTest_RoleBased.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class RoleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleController(RoleManager<IdentityRole> roleManager,
                              UserManager<ApplicationUser> userManager,
                              ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        // ========== Role Management Index ==========
        public IActionResult Index()
        {
            ViewBag.msg = TempData["msg"];
            ViewBag.roleList = _roleManager.Roles.ToList(); // ✅ Role list passed
            return View();
        }

        // ========== Create Role ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(string userRole)
        {
            string msg;

            if (string.IsNullOrWhiteSpace(userRole))
            {
                msg = "Please enter a valid role name!";
            }
            else if (await _roleManager.RoleExistsAsync(userRole))
            {
                msg = $"Role [{userRole}] already exists!";
            }
            else
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(userRole));
                msg = result.Succeeded ? $"Role [{userRole}] created successfully!" : "Failed to create role!";
            }

            TempData["msg"] = msg;
            return RedirectToAction(nameof(Index));
        }

        // ========== Assign Role to User ==========
        public IActionResult AssignRole()
        {
            ViewBag.users = _userManager.Users;
            ViewBag.roles = _roleManager.Roles;
            ViewBag.msg = TempData["msg"];

            ViewBag.userRoles = _userManager.Users
                .Select(u => new
                {
                    u.Email,
                    Roles = _userManager.GetRolesAsync(u).Result
                })
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userData, string roleData)
        {
            string msg;

            if (string.IsNullOrEmpty(userData) || string.IsNullOrEmpty(roleData))
            {
                msg = "Please select both a user and a role.";
            }
            else
            {
                var user = await _userManager.FindByEmailAsync(userData);
                if (user == null)
                {
                    msg = "User not found!";
                }
                else if (!await _roleManager.RoleExistsAsync(roleData))
                {
                    msg = "Role does not exist!";
                }
                else if (await _userManager.IsInRoleAsync(user, roleData))
                {
                    msg = "User already has this role.";
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, roleData);
                    msg = "Role assigned successfully!";
                }
            }

            TempData["msg"] = msg;
            return RedirectToAction(nameof(AssignRole));
        }

        // ========== Edit Role Name ==========
        public async Task<IActionResult> EditRole(string oldRoleName)
        {
            if (string.IsNullOrEmpty(oldRoleName)) return BadRequest();

            var role = await _roleManager.FindByNameAsync(oldRoleName);
            if (role == null) return NotFound();

            return View((object)role.Name);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(string oldRoleName, string newRoleName)
        {
            if (string.IsNullOrWhiteSpace(newRoleName)) return BadRequest();

            var role = await _roleManager.FindByNameAsync(oldRoleName);
            if (role == null) return NotFound();

            role.Name = newRoleName;
            role.NormalizedName = newRoleName.ToUpper();

            var result = await _roleManager.UpdateAsync(role);
            TempData["msg"] = result.Succeeded ? "Role updated successfully!" : "Failed to update role!";
            TempData["toast"] = TempData["msg"];

            return RedirectToAction(nameof(AssignRole));
        }

        // ========== Edit User's Assigned Role ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserRole(string userEmail, string oldRole, string newRole)
        {
            if (userEmail == null || oldRole == null || newRole == null) return BadRequest();

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, oldRole))
                await _userManager.RemoveFromRoleAsync(user, oldRole);

            if (!await _userManager.IsInRoleAsync(user, newRole))
                await _userManager.AddToRoleAsync(user, newRole);

            TempData["msg"] = "Role updated for user.";
            return RedirectToAction(nameof(AssignRole));
        }

        // ========== Remove User Role ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserRole(string userEmail, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return NotFound();

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            TempData["msg"] = result.Succeeded ? "Role removed from user." : "Failed to remove role.";
            return RedirectToAction(nameof(AssignRole));
        }

        public async Task<IActionResult> AllUser()
        {
            var users = _userManager.Users.ToList();
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var userRolesDict = new Dictionary<string, IList<string>>();

            foreach (var user in users)
            {
                var rolesForUser = await _userManager.GetRolesAsync(user);
                userRolesDict[user.Id] = rolesForUser;
            }

            ViewBag.Roles = roles;
            ViewBag.UserRolesDict = userRolesDict;
            ViewBag.msg = TempData["msg"];

            return View(users);
        }


        // ========== Delete Role ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName)) return BadRequest();

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                TempData["msg"] = "Role not found!";
            }
            else
            {
                var result = await _roleManager.DeleteAsync(role);
                TempData["msg"] = result.Succeeded ? "Role deleted successfully!" : "Failed to delete role!";
            }

            return RedirectToAction(nameof(Index));
        }

        // ========== List Users with Orders ==========
        [Authorize(Roles = "ADMIN,MANAGER")]
        public IActionResult UserList()
        {
            var usersWithLatestOrder = _context.Orders
                .GroupBy(o => o.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    LatestOrderId = g.Max(o => o.OrderId)
                })
                .OrderByDescending(x => x.LatestOrderId)
                .ToList();

            var userIdsOrdered = usersWithLatestOrder.Select(x => x.UserId).ToList();

            var users = _userManager.Users
                .Where(u => userIdsOrdered.Contains(u.Id))
                .ToList()
                .OrderBy(u => userIdsOrdered.IndexOf(u.Id))
                .ToList();

            return View(users);
        }

        // ========== View User Orders ==========
        [Authorize(Roles = "ADMIN,MANAGER")]
        public IActionResult UserOrders(string userId)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            ViewBag.UserName = user.UserName;
            return View(orders);
        }

        // ========== Update Order Status ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN,MANAGER")]
        public IActionResult UpdateOrderStatus(int id, string status)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == id);
            if (order != null)
            {
                order.Status = status;
                _context.SaveChanges();
                return RedirectToAction("UserOrders", new { userId = order.UserId });
            }

            return NotFound();
        }

        // ========== Delete User ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["msg"] = "Invalid user ID!";
                return RedirectToAction(nameof(AllUser));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                TempData["msg"] = result.Succeeded ? "User deleted!" : "Failed to delete user!";
            }
            else
            {
                TempData["msg"] = "User not found!";
            }

            return RedirectToAction(nameof(AllUser));
        }
    }
}
