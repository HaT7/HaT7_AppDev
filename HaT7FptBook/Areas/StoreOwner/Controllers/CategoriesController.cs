﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using HaT7FptBook.Data;
using HaT7FptBook.Models;
using HaT7FptBook.Utility;
using HaT7FptBook.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HaT7FptBook.Areas.StoreOwner.Controllers
{
    [Area(SD.Area_StoreOwner)]
    [Authorize(Roles = SD.Role_StoreOwner)]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }

        //======================== INDEX ==========================
        [HttpGet]
        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var storeId = _db.Stores.FirstOrDefault(a => a.StoreOwnerId == claims.Value);

            if (storeId == null)
            {
                ViewData["Message"] = "Error: Store Id not exist. Let's create your Store first";
                return RedirectToAction("Index", "Stores", new { area = "StoreOwner"});
            }
            
            try
            {
                var categoryList = _db.Categories.Where(a => a.StoreId == storeId.Id).ToList();
                return View(categoryList);
            }
            catch (Exception e)
            {
                Console.WriteLine("Category Error: " + e.Message);
                ViewData["Message"] = "Error: " + e.Message; 
            }

            return View(new List<Category>());
        }

        //======================== DELETE ==========================
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var category = _db.Categories.Find(id);
            _db.Categories.Remove(category);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        //======================== UPSERT ==========================
        [HttpGet]
        public IActionResult UpSert(int? id)
        {
            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var storeId = _db.Stores.FirstOrDefault(a => a.StoreOwnerId == claims.Value);

            if (id == 0 || id == null)
            {
                var categoryCreate = new Category();
                categoryCreate.StoreId = storeId.Id;
                return View(categoryCreate);
            }

            var category = _db.Categories.Find(id);
            return View(category);
        }

        [HttpPost]
        public IActionResult UpSert(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }
            if (category.Id == 0 || category.Id == null)
            {
                _db.Categories.Add(category);
            }
            else
            {
                _db.Categories.Update(category);
            }

            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}