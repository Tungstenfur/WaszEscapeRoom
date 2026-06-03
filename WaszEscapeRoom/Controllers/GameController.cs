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
                return RedirectToAction("Level1");
            case 2:
                return RedirectToAction("Level2");
            case 3:
                return RedirectToAction("Level3");
            case 4:
                return RedirectToAction("Level4");
            case 5:
                return RedirectToAction("Level4End");
            default:
                return RedirectToAction("Level4End");
        }
    }
    public IActionResult Intro()
    {
        return View();
    }
    public IActionResult Level1()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Level2()
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
            return RedirectToAction("Level2");
        }
        ModelState.AddModelError("", "Niepoprawny kod");
        return View("Level1");
    }

    public IActionResult Level2End()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Level3()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Level3(int code)
    {
        if (code == 81649)
        {
            var username = HttpContext.Session.GetString("Username");
            if (!string.IsNullOrWhiteSpace(username))
            {
                Database.SetCurrentLevel(username, 3);
            }
            return RedirectToAction("Level2End");
        }
        return RedirectToAction("Level2");
    }

    [HttpPost]
    public IActionResult Level3Complete(string code)
    {
        if (code == "739215")
        {
            var username = HttpContext.Session.GetString("Username");
            if (!string.IsNullOrWhiteSpace(username))
            {
                Database.SetCurrentLevel(username, 4);
            }
            return RedirectToAction("Level3End");
        }
        return RedirectToAction("Level3");
    }

    public IActionResult Level3End()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Level4()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Level4Complete(string code)
    {
        if (code == "492706")
        {
            var username = HttpContext.Session.GetString("Username");
            if (!string.IsNullOrWhiteSpace(username))
            {
                Database.SetCurrentLevel(username, 5);
            }
            return RedirectToAction("Level4End");
        }
        return RedirectToAction("Level4");
    }

    public IActionResult Level4End()
    {
        return View();
    }
}