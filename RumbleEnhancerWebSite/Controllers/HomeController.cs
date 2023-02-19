using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using RumbleEnhancerWebSite.Helpers;
using RumbleEnhancerWebSite.Models;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Security.Claims;

namespace RumbleEnhancerWebSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProfileData _data;

        public HomeController(ILogger<HomeController> logger, IProfileData data)
        {
            _logger = logger;
            DBManager.DBSetup();
            _data = data;
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
        public async Task<IActionResult> LogIn(ProfileModel profile)
        {
            try
            {
                //Check if Login is valid
                Profile p = await _data.GetProfileFromEmail(profile.ProfileEmail);
                bool passwordCorrect = BCrypt.Net.BCrypt.Verify(profile.ProfilePassword, p.ProfilePassword);
                if (!passwordCorrect)
                {
                    ModelState.AddModelError("", "Incorrect Login!");
                    return View();
                }

                //If verfied profile
                if (p.ProfileVerified == 1)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, p.ProfileName),
                        new Claim(ClaimTypes.Email, p.ProfileEmail),
                        new Claim(ClaimTypes.NameIdentifier, p.ProfileId.ToString())
                    };
                    var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);
                    return RedirectToAction("Index", "Dashboard", profile);
                }
                else
                {
                    return View("VerifyProfile", profile);
                }

            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Incorrect Login!");
                Console.WriteLine(e.Message);
            };

            return View(); ;
        }

        /*[HttpPost]
        public async Task<IActionResult> LogIn(ProfileModel profile)
        {
            ModelState.Remove("ProfileName");
            if (ModelState.IsValid)
            {
                var loggedInProfile = DBManager.LogIn(profile);

                //If incorrect login
                if (loggedInProfile.ProfileEmail == null)
                {
                    ModelState.AddModelError("", "Incorrect Login!");
                }
                else
                {
                    //If verfied profile
                    if (loggedInProfile.VerificationStatus.Equals(true))
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, profile.ProfileName),
                            new Claim(ClaimTypes.Email, profile.ProfileEmail),
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
        }*/

        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Index");
        }

        /*[HttpPost]
        public IActionResult SignUp(ProfileModel profile)
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
        }*/

        [HttpPost]
       public async Task<IActionResult> SignUp(ProfileModel profile)
        {
            if (ModelState.IsValid)
            {
                Profile p = new Profile()
                {
                    ProfileId = Guid.NewGuid(),
                    ProfileName = profile.ProfileName,
                    ProfilePassword = BCrypt.Net.BCrypt.HashPassword(profile.ProfilePassword),
                    ProfileEmail = profile.ProfileEmail,
                    ProfileVerified = 0,
                };

                try
                {
                    await _data.InsertProfile(p);
                    //Send VerificationString
                    WebHelper.SendEmail(p.ProfileEmail, p.ProfileId.ToString());
                    return RedirectToAction("Login");
                }
                catch(Exception ex)
                {
                    ModelState.AddModelError("", "Username or email already exist!");
                    return View();
                }
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyProfile(ProfileModel profile)
        {
            //Check the link
            string[] videoData = WebHelper.GetVideoData(profile.RumbleURL);
            if (!(videoData[0] == profile.ProfileName && videoData[1].Contains(profile.VerificationString)))
            {
                ModelState.AddModelError("", "Link is invalid!");
                return View(profile);
            }

            //Verify in db
            try
            {
                await _data.SetVerifiedProfile(profile.VerificationString);
                TempData["Message"] = "Successfully verified!";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Verification failed!");
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