using Microsoft.AspNetCore.Mvc;

namespace WaszEscapeRoom.Controllers;

public class GameController:Controller
{
    public IActionResult Index()
    {
        return View();
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