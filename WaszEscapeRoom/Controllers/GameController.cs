using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WaszEscapeRoom.Controllers;

public class GameController:Controller
{
    public IActionResult Index()
    {
        if(string.IsNullOrWhiteSpace(HttpContext.Session.GetString("username")))
        {
            return RedirectToAction("Login","Home");
        }
        
    }
    public IActionResult Intro()
    {
        return View();
    }
    public IActionResult Intro2()
    {
        return View();
    }
}