using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P225Allup.DAL;
using P225Allup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using P225Allup.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace P225Allup.Areas.Manage.Controllers
{
    [Area("manage")]
    [Authorize(Roles ="SuperAdmin,Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(bool? status)
        {
            IQueryable<Product> query = _context.Products;

            if (status != null)
            {
                query = query.Where(b => b.IsDeleted == status);
            }

            ViewBag.Status = status;

            return View(await query.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted && !c.IsMain).ToListAsync();
            ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted && !c.IsMain).ToListAsync();
            ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();

            //if (!ModelState.IsValid) return View();

            if(!await _context.Categories.AnyAsync(c=>!c.IsDeleted && !c.IsMain && c.Id == product.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Please Select A Correct Category");
                return View();
            }

            if (!await _context.Brands.AnyAsync(c => !c.IsDeleted &&  c.Id == product.BrandId))
            {
                ModelState.AddModelError("BrandId", "Please Select A Correct Brand");
                return View();
            }

            if (product.MainFile != null)
            {
                if (product.MainFile.ChechFileContentType("image/jpeg"))
                {
                    ModelState.AddModelError("MainFile", "Please Select Correct Image Type. Must Be .jpg or .jpeg");
                    return View();
                }

                if (product.MainFile.CheckFileSize(50))
                {
                    ModelState.AddModelError("MainFile", "Please Select Correct Image Size. Must Be Max 50kb");
                    return View();
                }

                product.MainImage = await product.MainFile.CreateFileAsync(_env, "assets", "images", "product");
            }
            else
            {
                ModelState.AddModelError("MainFile", "Please Select Main Image");
                return View();
            }

            if (product.SecondFile != null)
            {
                if (product.SecondFile.ChechFileContentType("image/jpeg"))
                {
                    ModelState.AddModelError("SecondFile", "Please Select Correct Image Type. Must Be .jpg or .jpeg");
                    return View();
                }

                if (product.SecondFile.CheckFileSize(50))
                {
                    ModelState.AddModelError("SecondFile", "Please Select Correct Image Size. Must Be Max 50kb");
                    return View();
                }

                product.SecondImage = await product.SecondFile.CreateFileAsync(_env, "assets", "images", "product");
            }
            else
            {
                ModelState.AddModelError("SecondFile", "Please Select Second Image");
                return View();
            }

            if (product.Files != null && product.Files.Count > 0)
            {
                List<ProductImage> productImages = new List<ProductImage>();

                foreach (IFormFile file in product.Files)
                {
                    if (file.ChechFileContentType("image/jpeg"))
                    {
                        ModelState.AddModelError("Files", "Please Select Correct Image Type. Must Be .jpg or .jpeg");
                        return View();
                    }

                    if (file.CheckFileSize(50))
                    {
                        ModelState.AddModelError("Files", "Please Select Correct Image Size. Must Be Max 50kb");
                        return View();
                    }

                    ProductImage productImage = new ProductImage
                    {
                        Image = await file.CreateFileAsync(_env, "assets", "images", "product-quick")
                    };

                    productImages.Add(productImage);
                }

                product.ProductImages = productImages;
            }
            else
            {
                ModelState.AddModelError("Files", "Please Select Files");
                return View();
            }
            product.Title = product.Title.Trim();

            string seria = (_context.Brands.FirstOrDefault(b => b.Id == product.BrandId).Name.Substring(0, 2) + product.Title.Substring(0, 2)).ToLower();
            int code = _context.Products.OrderByDescending(p => p.Id).FirstOrDefault(p => !p.IsDeleted && p.Seria == seria) != null ? _context.Products.OrderByDescending(p => p.Id).FirstOrDefault(p => !p.IsDeleted && p.Seria == seria).Code += 1 : 1;

            product.Seria = seria;
            product.Code = code;
            product.CreatedAt = DateTime.UtcNow.AddHours(4);

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
