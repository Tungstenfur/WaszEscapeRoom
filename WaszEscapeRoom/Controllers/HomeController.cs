using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WaszEscapeRoom.Models;

namespace WaszEscapeRoom.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    [ValidateAntiForgeryToken]
    [HttpPost]
    public IActionResult Register(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError("", "Nazwa użytkownika i hasło są wymagane");
            return View();
        }
        username = username.Trim().ToLower();
        var result=Database.registerUser(username,password);
        if(result==RegisterResult.UserAlreadyExists)
        {
            ModelState.AddModelError("", "Nazwa użytkownika jest już zajęta");
            return View();
        }
        return RedirectToAction("Login");
    }
    [ValidateAntiForgeryToken]
    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Nazwa użytkownika i hasło są wymagane");
            return View();
        }

        username = username.Trim().ToLower();
        var result = Database.verifyLogin(username, password);
        if (result == LoginResult.Success)
        {
            HttpContext.Session.SetString("Username", username);
            return RedirectToAction("Index");
        }

        if (result == LoginResult.InvalidPassword)
        {
            ModelState.AddModelError("", "Nieprawidłowe hasło");
        }
        else if (result == LoginResult.UserNotFound)
        {
            ModelState.AddModelError("", "Użytkownik nie istnieje");
        }
        else
        {
            ModelState.AddModelError("", "Wystąpił błąd podczas logowania");
        }

        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Remove("Username");
        return RedirectToAction("Index");
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}