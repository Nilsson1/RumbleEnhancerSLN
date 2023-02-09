using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using RumbleEnhancerWebSite.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace RumbleEnhancerWebSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            DBManager.DBSetup();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        public IActionResult LogIn()
        {
            return View();
        }

        public IActionResult VerifyProfile()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(Profile profile)
        {
            ModelState.Remove("ProfileName");
            if (ModelState.IsValid)
            {
                var loggedInProfile = DBManager.LogIn(profile);

                //If incorrect login
                if (loggedInProfile.ProfileEmail == null)
                {
                    ModelState.AddModelError("", "Incorrect login!");
                }
                else
                {
                    //If verfied profile
                    if (loggedInProfile.VerificationStatus.Equals(true))
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, profile.ProfileName),
                        };
                        var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
                        await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        return View("VerifyProfile", loggedInProfile);
                    }
                }
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult SignUp(Profile profile)
        {
            if (ModelState.IsValid)
            {
                var createdProfile = DBManager.CreateProfile(profile);
                if (createdProfile.ProfileName == null)
                {
                    ModelState.AddModelError("", "Username or email already exist!");
                }
                else
                {
                    //Send VerificationString
                    DBManager.SendEmail(DBManager.GetProfile(createdProfile));
                    return RedirectToAction("Login");
                }
            }
            return View();
        }

        [HttpPost]
        public IActionResult VerifyProfile(Profile profile)
        {
            if (DBManager.VerifyProfile(profile)) 
            {
                TempData["Message"] = "Successfully verified!";
                return RedirectToAction("Index", "Dashboard");
            }
            return View(profile);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}