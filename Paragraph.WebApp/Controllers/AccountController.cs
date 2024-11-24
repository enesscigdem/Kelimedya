using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Paragraph.WebApp.Models;

namespace Paragraph.WebApp.Controllers;

public class AccountController : Controller
{
    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }
}