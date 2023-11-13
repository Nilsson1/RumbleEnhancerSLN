using Azure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using RumbleEnhancerWebSite.Helpers;
using RumbleEnhancerWebSite.Models;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Security.Claims;
using System.Text;

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
            //TEST RUMBLE POST REQUEST FOR LOGIN VERIFICATION **WORKS**
            /*
            try
            {
                using (var client = new HttpClient())
                {
                    
                    //Uri url = new Uri("https://rumble.com/service.php?name=user.get_salts&included_js_libs=main,web_services,events,error,facebook_events,darkmode,ui_header,ui,event_handler,ui_overlay,md5,validate,facebook,google,apple,login_form&included_css_libs=ui_overlay,form,service.user");
                    Uri url2 = new Uri("https://rumble.com/service.php?name=user.login&included_js_libs=main,web_services,events,error,facebook_events,darkmode,ui_header,ui,event_handler,ui_overlay,md5,validate,facebook,google,apple,login_form&included_css_libs=ui_overlay,form,service.user");
                    
                    var data = new System.Net.Http.StringContent("username=USERNAME&password_hashes=PASSWORD", Encoding.UTF8, "application/x-www-form-urlencoded");

                    var result = await client.PostAsync(url2, data);
                    string resultContent = await result.Content.ReadAsStringAsync();
                    Console.WriteLine(resultContent);
                }
            }
            catch (Exception ex)
            {

            }*/

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
                    profile.VerificationString = p.ProfileId.ToString();
                    profile.ProfileName = p.ProfileName;
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

        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Index");
        }

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
        public IActionResult SendVerificationCode(ProfileModel profile)
        {
            return View("VerifyProfile", profile);
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