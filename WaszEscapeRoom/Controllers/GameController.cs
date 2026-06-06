using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WaszEscapeRoom.Controllers;

public class GameController : Controller
{
    private int GetUserId()
    {
        var username = HttpContext.Session.GetString("Username");
        return Database.GetUserId(username);
    }

    private void SaveLevelTime(int level)
    {
        var startTime = HttpContext.Session.GetString($"Level{level}Start");
        if (long.TryParse(startTime, out var startTicks))
        {
            var elapsed = (int)(DateTime.UtcNow.Ticks - startTicks) / TimeSpan.TicksPerSecond;
            var userId = GetUserId();
            if (userId > 0)
            {
                Database.LogLevelCompletion(userId, level, (int)elapsed);
            }
        }
    }

    private void StartLevelTimer(int level)
    {
        HttpContext.Session.SetString($"Level{level}Start", DateTime.UtcNow.Ticks.ToString());
    }

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
        var level = Database.GetCurrentLevel(username);
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
                return RedirectToAction("Level5");
            case 6:
                return RedirectToAction("Level5End");
            default:
                return RedirectToAction("Level5End");
        }
    }
    public IActionResult Intro()
    {
        return View();
    }
    public IActionResult Level1()
    {
        StartLevelTimer(1);
        return View();
    }

    [HttpGet]
    public IActionResult Level2()
    {
        SaveLevelTime(1);
        StartLevelTimer(2);
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
        SaveLevelTime(2);
        return View();
    }

    [HttpGet]
    public IActionResult Level3()
    {
        StartLevelTimer(3);
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
        if (code == "admin1")
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
        SaveLevelTime(3);
        return View();
    }

    [HttpGet]
    public IActionResult Level4()
    {
        StartLevelTimer(4);
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
        SaveLevelTime(4);
        return View();
    }

    [HttpGet]
    public IActionResult Level5()
    {
        StartLevelTimer(5);
        return View();
    }

    [HttpPost]
    public IActionResult Level5Complete(string code)
    {
        if (code == "REACTOR_DOWN")
        {
            var username = HttpContext.Session.GetString("Username");
            if (!string.IsNullOrWhiteSpace(username))
            {
                Database.SetCurrentLevel(username, 6);
            }
            return RedirectToAction("Level5End");
        }
        return RedirectToAction("Level5");
    }

    public IActionResult Leaderboard(int level = 1)
    {
        if (level < 1 || level > 5)
        {
            level = 1;
        }
        var leaderboardData = Database.GetLeaderboardForLevel(level);
        ViewBag.CurrentLevel = level;
        return View(leaderboardData);
    }

    public IActionResult Level5End()
    {
        SaveLevelTime(5);
        return View();
    }
    public IActionResult ResetLevels()
    {
        Database.deleteUserProgress(HttpContext.Session.GetString("Username")??throw new InvalidOperationException("User not logged in"));
        return RedirectToAction("Index");
    }
}