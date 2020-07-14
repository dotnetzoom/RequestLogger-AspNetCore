using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RequestLogger.Models;
using RequestLogger.AppCode;

namespace RequestLogger.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
        }

        [LogRequest]
        public IActionResult Index(MyViewModel myViewModel)
        {
            return View();
        }

        [LogRequest]
        public IActionResult Privacy()
        {
            throw new Exception("test");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
