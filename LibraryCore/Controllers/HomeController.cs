﻿using Microsoft.AspNetCore.Mvc;

namespace LibraryCore.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "About the Library System";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Details";
            return View();
        }
    }
}