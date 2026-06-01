using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaszEscapeRoom;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WaszEscapeRoom.Controllers;

public class GameController:Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrWhiteSpace(username))
        {
            context.Result = RedirectToAction("Login", "Home");
            return;
        }
        base.OnActionExecuting(context);
    }

    public IActionResult Index()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrWhiteSpace(username))
        {
            return RedirectToAction("Login","Home");
        }
        var level = WaszEscapeRoom.Database.GetCurrentLevel(username);
        switch (level)
        {
            case 0:
                return RedirectToAction("Intro");
            case 1:
                return RedirectToAction("Intro2");
            default:
                return View();
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

    public IActionResult Level1()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Level2(int code)
    {
        if (code == 5476)
        {
            var username = HttpContext.Session.GetString("Username");
            if (!string.IsNullOrWhiteSpace(username))
            {
                Database.SetCurrentLevel(username, 2);
            }
            return View();
        }
        ModelState.AddModelError("", "Niepoprawny kod");
        return View("Level1");
    }
}