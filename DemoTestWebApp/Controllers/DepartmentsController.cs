using DemoTestWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DemoTestWebApp.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; // Required for file handling

        public DepartmentsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var departments = _context.Departments.Include(d => d.SubDepartments).ToList();
            return View(departments);
        }

        // GET: Department/Create
        public IActionResult Create()
        {
            // Example: Get list of parent departments from database
            var parentDepartments = _context.Departments
                                              .Where(d => d.ParentDepartmentId == null) // Assuming top-level departments
                                              .Select(d => new SelectListItem
                                              {
                                                  Value = d.Id.ToString(),
                                                  Text = d.Name
                                              })
                                              .ToList();

            // Add an empty option if needed
            parentDepartments.Insert(0, new SelectListItem
            {
                Value = null,
                Text = "Select Parent Department"
            });

            ViewBag.ParentDepartmentId = new SelectList(parentDepartments, "Value", "Text");

            return View();

        }

        // POST: Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department, IFormFile logoFile)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload
                if (logoFile != null && logoFile.Length > 0)
                {
                    // Save the uploaded file to wwwroot/Images folder (or any other location)
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + logoFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        logoFile.CopyTo(fileStream);
                    }

                    // Save the file path to the department object
                    department.Logo = "/Images/" + uniqueFileName; // Store relative path to the image
                }

                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }


        public IActionResult Edit(int id)
        {
            var department = _context.Departments.Find(id);
            if (department == null)
            {
                return NotFound();
            }

            // Prepare ViewBag or ViewData for dropdown if needed
            PrepareDropdownData(id); // Example method to prepare dropdown data

            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department department,IFormFile LogoUpload)
        {
            if (id != department.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", LogoUpload.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await LogoUpload.CopyToAsync(stream);
                    }

                    department.Logo = $"/images/{LogoUpload.FileName}";

                    // Update the department in the database
                    _context.Update(department);
                    _context.SaveChanges();

                    return RedirectToAction("Index"); // Redirect to department list or details page
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists((int)department.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If ModelState is not valid, return to the same view with validation errors
            PrepareDropdownData(id); // Re-populate dropdown data if needed
            return View(department);
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(d => d.Id == id);
        }


        private void PrepareDropdownData(int id)
         {
            // Example: Get list of parent departments from database
            ViewBag.ParentDepartments = _context.Departments
                                 .Where(d => d.Id != id) // Exclude current department if editing
                                 .Select(d => new SelectListItem
                                 {
                                     Value = d.Id.ToString(),
                                     Text = d.Name
                                 })
                                 .ToList();
        }


        // GET: Departments/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = _context.Departments
                .FirstOrDefault(m => m.Id == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var department = _context.Departments.Find(id);
            if (department == null)
            {
                return NotFound();
            }

            _context.Departments.Remove(department);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }




    }
}
