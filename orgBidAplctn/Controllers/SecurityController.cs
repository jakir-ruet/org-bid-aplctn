using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using orgBidAplctn.Models.Security;
using orgBidAplctn.Models;
using orgBidAplctn.Models.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace orgBidAplctn.Controllers
{
    public class SecurityController : Controller
    {
        private AppDbContext db = null;
        private readonly IWebHostEnvironment _env;

        public SecurityController(AppDbContext _db, IWebHostEnvironment env)
        {
            this.db = _db;
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult SignInApp(string returnUrl = null)
        {
            if (!User.Identity.IsAuthenticated)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }
            else
            {
                string userAccessType = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "accessType")?.Value;
                ViewBag.UserSurName = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userFullName")?.Value;
                ViewBag.ProfilePic = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "profilePic")?.Value; ;
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignInApp(SignIn obj)
        {
            if (ModelState.IsValid)
            {
                if (!User.Identity.IsAuthenticated)
                {
                    string passwords = MD5Hash(obj.Password.ToString());
                    var query = await (from u in db.CommLoginInfos
                                 join companyDet in db.CommCompInfos on u.CompId equals companyDet.CompId
                                 where u.UserNm == obj.UserName && u.UserPass == passwords && u.IsActive == 1
                                 select new { UserId = u.UserId, UserName = u.UserNm, AccessType = u.LoginTp, UserFirstName = u.FastName, UserLastName = u.LastName, UserCanMod = u.CanMod, UserCanDel = u.CanDel, ProfilePic = u.ProfileImg, CompanyId = u.CompId, CompanyName = companyDet.CompName, CompanyStartId = companyDet.CompStrtNo }).AsNoTracking().FirstOrDefaultAsync();
                    if (query != null)
                    {
                        var wwwroot = _env.WebRootPath;
                        var profilePicPath = query.ProfilePic != null ? Path.Combine(wwwroot, "images/profile", query.ProfilePic.Trim()) : null;
                        // Adding Cookie Authentication Scheme ...
                        var cookieClaims = new List<Claim>
                        {
                            new Claim("accessType", query.AccessType.ToString()),
                            new Claim("userId", query.UserId.ToString()),
                            new Claim("userName", query.UserName.Trim()),
                            new Claim("userFullName", new string(CharsToTitleCase(query.UserFirstName.Trim() + ' ' + query.UserLastName.Trim()).ToArray())),
                            new Claim("userSurName", query.UserFirstName.Trim()),
                            new Claim("userCanMod", query.UserCanMod.ToString()),
                            new Claim("userCanDel", query.UserCanDel.ToString()),
                            new Claim("companyId", query.CompanyId.ToString()),
                            new Claim("companyStartId", query.CompanyStartId.Trim()),
                            new Claim("companyName", query.CompanyName.Trim()),
                            new Claim("rememberMe", obj.RememberMe.ToString()),
                            new Claim("profilePic", System.IO.File.Exists(profilePicPath) ? "/images/profile/" + query.ProfilePic.Trim() : "")
                        };
                        var cookieIdentity = new ClaimsIdentity(cookieClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var cookiePrincipal = new ClaimsPrincipal(cookieIdentity);

                        // HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, cookiePrincipal, new AuthenticationProperties { IsPersistent = true });
                        // Set the authentication cookie with or without persistent option based on Remember Me status
                        if (obj.RememberMe)
                        {
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, cookiePrincipal, new AuthenticationProperties { IsPersistent = true });
                        } else {
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, cookiePrincipal);
                        }
                        // Redirect the user to the returnUrl if it is provided and is a local URL
                        if (!string.IsNullOrEmpty(obj.ReturnUrl) && Url.IsLocalUrl(obj.ReturnUrl))
                        {
                            return Redirect(obj.ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Dashboard");
                        }
                    }
                    else
                    {
                        await HttpContext.SignOutAsync();
                        ViewBag.ErrorMessage = "Invalid User Name Or Password...";
                        return View("SignInApp");
                        // return RedirectToAction("SignInApp", "Security");
                    }
                }
                else
                {
                    await HttpContext.SignOutAsync();
                    ViewBag.ErrorMessage = "You Need To Re-Login Before Using The Function...";
                    return RedirectToAction("SignInApp", "Security");
                }
            }
            else
            {
                await HttpContext.SignOutAsync();
                ViewBag.ErrorMessage = "Invalid Data...";
                return RedirectToAction("SignInApp", "Security");
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> SignOutApp()
        {
            HttpContext.Session.Remove("loginStatus");
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignInApp", "Security");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        IEnumerable<char> CharsToTitleCase(string s)
        {
            bool newWord = true;
            foreach (char c in s)
            {
                if (newWord) { yield return Char.ToUpper(c); newWord = false; }
                else yield return Char.ToLower(c);
                if (c == ' ') newWord = true;
            }
        }

        public static string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            // MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            MD5 md5provider = MD5.Create();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }
    }
}
