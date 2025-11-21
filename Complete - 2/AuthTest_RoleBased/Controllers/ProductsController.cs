using AuthTest_RoleBased.Data;
using AuthTest_RoleBased.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthTest_RoleBased.Controllers
{
    [Authorize(Roles = "ADMIN,MANAGER")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(string userText, string sortOrder, int page = 1)
        {
            ViewBag.sWord = userText;
            ViewBag.currentSort = sortOrder;

            ViewBag.NameSortParam = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.PriceSortParam = sortOrder == "price_asc" ? "price_desc" : "price_asc";

            IQueryable<Product> products = _context.Products;

            if (!string.IsNullOrEmpty(userText))
            {
                string lower = userText.ToLower();
                products = products.Where(p => p.Name.ToLower().Contains(lower) || p.Unit.ToLower().Contains(lower));
            }

            switch (sortOrder)
            {
                case "name_asc":
                    products = products.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.ProductId); // ✅ Default: serial by ProductId
                    break;
            }

            int pageSize = 10;
            var paginatedList = await PaginatedList<Product>.CreateAsync(products, page, pageSize);

            ViewBag.cp = paginatedList.PageIndex;
            ViewBag.tp = paginatedList.TotalPages;
            ViewBag.count = await products.CountAsync();
            ViewBag.pSize = pageSize;

            return View(paginatedList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    product.Photo = "/images/products/" + uniqueFileName;
                }

                
                _context.Add(product);
                await _context.SaveChangesAsync();

               
                var createdProduct = await _context.Products
                                                    .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);

              
                if (createdProduct != null)
                {
                    await LogProductAction(createdProduct.ProductId, "Created", $"New product created: {createdProduct.Name}");
                }

                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }



        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var oldProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        product.Photo = "/images/products/" + uniqueFileName;
                    }
                    else
                    {
                        
                        var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);
                        if (existingProduct != null)
                        {
                            product.Photo = existingProduct.Photo;
                        }
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();

                   
                    await LogProductAction(product.ProductId, "Updated", "Product details updated.", oldProduct, product);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
              
                await LogProductAction(product.ProductId, "Deleted", $"Product deleted: {product.Name}");

                
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }




        private async Task LogProductAction(int productId, string action, string details, Product? oldProduct = null, Product? newProduct = null)
        {
            var userId = _userManager.GetUserId(User); // Get the logged-in user

            var log = new ProductLog
            {
                ProductId = productId,
                ProductName = newProduct?.Name ?? oldProduct?.Name ?? "[Deleted]",
                Action = action,
                Details = details,
                UserId = userId,
                ActionDate = DateTime.Now
            };

            
            if (action == "Updated" && oldProduct != null && newProduct != null)
            {
                log.Details = $"Old Name: {oldProduct.Name}, New Name: {newProduct.Name} | Old Price: {oldProduct.Price}, New Price: {newProduct.Price} | Old Quantity: {oldProduct.Quantity}, New Quantity: {newProduct.Quantity}";
            }

            _context.ProductLogs.Add(log);
            await _context.SaveChangesAsync();
        }






        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ViewLogs()
        {
            var logs = await _context.ProductLogs
                .Include(log => log.User) // still works since User FK is intact
                .OrderByDescending(log => log.ActionDate)
                .ToListAsync();

            return View(logs);
        }




        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
