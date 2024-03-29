using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using orgBidAplctn.Filters;
using orgBidAplctn.Models;
using orgBidAplctn.Models.Data;
using orgBidAplctn.Models.DataViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace orgBidAplctn.Controllers
{
    [TypeFilter(typeof(ViewBagActionFilter))]
    public class DashboardController : Controller
    {
        private AppDbContext dbContext = null;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public DashboardController(AppDbContext appDbContext, IWebHostEnvironment env)
        {
            this.dbContext = appDbContext;
            _hostingEnvironment = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult PageNotFound()
        {
            return View();
        }

        public IActionResult BiddingX()
        {
            return View();
        }

        public async Task<IActionResult> Profile()
        {
            var UserId = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId")?.Value);
            CommLoginInfo userProfile = await dbContext.CommLoginInfos.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == UserId);
            return View(userProfile);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserBasicDet(CommLoginInfo model)
        {
            var UserId = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId")?.Value);
            ModelState.Remove("model.UserNm");
            ModelState.Remove("model.UserPass");
            if (ModelState.IsValid)
            {
                using (var context = new AppDbContext())
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            CommLoginInfo loginDet = await dbContext.CommLoginInfos.FirstOrDefaultAsync(s => s.UserId == UserId);
                            loginDet.FastName = model.FastName.Trim();
                            loginDet.LastName = model.LastName.Trim();
                            loginDet.BrthDt = model.BrthDt;
                            loginDet.Gender = model.Gender;
                            loginDet.UserAdd = model.UserAdd != null ? model.UserAdd.Trim() : "";
                            loginDet.UserCity = model.UserCity != null ? model.UserCity.Trim() : "";
                            loginDet.UserCntry = model.UserCntry != null ? model.UserCntry.Trim() : "";
                            dbContext.Update(loginDet);
                            await dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return Json(new { success = true, message = "User updated successfully." });
                        }
                        catch (DbUpdateException ex1)
                        {
                            await transaction.RollbackAsync();
                            var Message = ex1.Message;
                            if (ex1.InnerException != null)
                            {
                                Message += " : " + ex1.InnerException.Message;
                            }
                            Debug.WriteLine("Exception: " + Message);
                            ViewBag.ErrorMessage = "Exception: " + Message;
                            // throw ex1;
                            return Json(new { success = false, message = "Exception: " + Message });
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            ViewBag.ErrorMessage = "Exception: " + ex.Message;
                            // throw ex;
                            return Json(new { success = false, message = "Exception: " + ex.Message });
                        }
                    }
                }
            }
            else
            {
                var errors = ModelState.Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                var errorMessage = string.Join(", ", errors);
                return Json(new { success = false, message = errorMessage });
                // return Json(new { success = false, message = "Invalid Party Data." }); UpdateUserOtherDet
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserOtherDet(CommLoginInfo model)
        {
            var UserId = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId")?.Value);
            ModelState.Remove("model.UserNm");
            ModelState.Remove("model.UserPass");
            if (ModelState.IsValid)
            {
                using (var context = new AppDbContext())
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            CommLoginInfo loginDet = await dbContext.CommLoginInfos.FirstOrDefaultAsync(s => s.UserId == UserId);
                            loginDet.EmailAdd = model.EmailAdd.Trim();
                            loginDet.ContNo = model.ContNo.Trim();
                            dbContext.Update(loginDet);
                            await dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return Json(new { success = true, message = "User updated successfully." });
                        }
                        catch (DbUpdateException ex1)
                        {
                            await transaction.RollbackAsync();
                            var Message = ex1.Message;
                            if (ex1.InnerException != null)
                            {
                                Message += " : " + ex1.InnerException.Message;
                            }
                            Debug.WriteLine("Exception: " + Message);
                            ViewBag.ErrorMessage = "Exception: " + Message;
                            // throw ex1;
                            return Json(new { success = false, message = "Exception: " + Message });
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            ViewBag.ErrorMessage = "Exception: " + ex.Message;
                            // throw ex;
                            return Json(new { success = false, message = "Exception: " + ex.Message });
                        }
                    }
                }
            }
            else
            {
                var errors = ModelState.Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                var errorMessage = string.Join(", ", errors);
                return Json(new { success = false, message = errorMessage });
                // return Json(new { success = false, message = "Invalid Party Data." }); 
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePasswordDet(string userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                return Json(new { success = false, message = "You are not authorize to change value ..." });
            }
            var UserId = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId")?.Value);
            string CurrPasswords = MD5Hash(currentPassword.ToString());
            string NewPasswords = MD5Hash(newPassword.ToString());
            var query = await (from u in dbContext.CommLoginInfos
                         where u.UserId == UserId && u.UserPass == CurrPasswords && u.IsActive == 1
                         select new { UserId = u.UserId }).AsNoTracking().FirstOrDefaultAsync();
            if (query != null)
            {
                using (var context = new AppDbContext())
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            CommLoginInfo loginDet = await dbContext.CommLoginInfos.FirstOrDefaultAsync(s => s.UserId == UserId);
                            loginDet.UserPass = NewPasswords;
                            dbContext.Update(loginDet);
                            await dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return Json(new { success = true, message = "Password updated successfully." });
                        }
                        catch (DbUpdateException ex1)
                        {
                            await transaction.RollbackAsync();
                            var Message = ex1.Message;
                            if (ex1.InnerException != null)
                            {
                                Message += " : " + ex1.InnerException.Message;
                            }
                            Debug.WriteLine("Exception: " + Message);
                            ViewBag.ErrorMessage = "Exception: " + Message;
                            // throw ex1;
                            return Json(new { success = false, message = "Exception: " + Message });
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            ViewBag.ErrorMessage = "Exception: " + ex.Message;
                            // throw ex;
                            return Json(new { success = false, message = "Exception: " + ex.Message });
                        }
                    }
                }
            } else
            {
                return Json(new { success = false, message = "Invalid User or Password ..." });
            }
        }

        [HttpPost]
        public IActionResult UploadProfileImage(IFormFile image)
        {
            var UserId = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId")?.Value);
            // Save the image on the server
            if (image != null && image.Length > 0)
            {
                string path = Path.Combine(_hostingEnvironment.WebRootPath, "images/profile");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string randomFileName = "";
                CommLoginInfo loginDet = dbContext.CommLoginInfos.FirstOrDefault(s => s.UserId == UserId);
                if (string.IsNullOrEmpty(loginDet.ProfileImg))
                {
                    randomFileName = Path.GetRandomFileName();
                    randomFileName = new string(randomFileName.Where(c => Char.IsLetterOrDigit(c)).ToArray());
                    randomFileName = randomFileName + ".jpg";
                } else
                {
                    randomFileName = loginDet.ProfileImg;
                }
                var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images/profile/" + randomFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }
                using (var context = new AppDbContext())
                {
                    using (var transaction = dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            loginDet.ProfileImg = randomFileName;
                            dbContext.Update(loginDet);
                            dbContext.SaveChanges();
                            transaction.Commit();

                            // Updating Image Source to Cookie ...
                            var claimsIdentity = (ClaimsIdentity)User.Identity;
                            var claim = claimsIdentity.FindFirst("profilePic");
                            if (claim != null)
                            {
                                claimsIdentity.RemoveClaim(claim);
                            }
                            claimsIdentity.AddClaim(new Claim("profilePic", "/images/profile/" + randomFileName));
                            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));


                            return Json(new { success = true, imageUrl = "/images/profile/" + randomFileName });
                        }
                        catch (DbUpdateException ex1)
                        {
                            transaction.Rollback();
                            var Message = ex1.Message;
                            if (ex1.InnerException != null)
                            {
                                Message += " : " + ex1.InnerException.Message;
                            }
                            Debug.WriteLine("Exception: " + Message);
                            ViewBag.ErrorMessage = "Exception: " + Message;
                            // throw ex1;
                            return Json(new { success = false, message = "Exception: " + Message });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            ViewBag.ErrorMessage = "Exception: " + ex.Message;
                            // throw ex;
                            return Json(new { success = false, message = "Exception: " + ex.Message });
                        }
                    }
                }
                // return Ok();
            }
            // return BadRequest();
            return Json(new { success = false, message = "No File Found To Process ..." });
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

        [HttpGet]
        public async Task<IActionResult> GetBidEvents(DateTime startDate, DateTime endDate)
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            endDate = endDate.Date.AddDays(1).AddSeconds(-1); // Set the time component to 23:59:59
            List<CommBidMstr> bidEventList;
            // Get bidEventList based on the date range
            bidEventList = await (from bidList in dbContext.CommBidMstrs.Include(o => o.Prod)
                        where bidList.CompId == Int32.Parse(CompanyId)
                        where bidList.BidDate >= startDate && bidList.BidDate <= endDate
                        orderby bidList.BidDate
                        select bidList).AsNoTracking().ToListAsync();

            // Convert the bidEventList to a list of ViewModalCalendarEvent
            List<ViewModalCalendarEvent> events = bidEventList.Select(game => new ViewModalCalendarEvent
            {
                EventId = game.BidId,
                Title = game.Prod.ProdName, // Replace with a suitable title property from CommGameMstr
                Start = game.BidStrtTm,
                End = game.BidEndTm,
                Status = game.BidStat
            }).ToList();

            return Json(events);
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData(DateTime startDate, DateTime endDate)
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            var draftBidCount = await dbContext.CommBidMstrs.CountAsync(bid => bid.BidStat == 0 && bid.CompId == Int32.Parse(CompanyId));
            var activeBidCount = await dbContext.CommBidMstrs.CountAsync(bid => bid.BidStat == 1 && bid.CompId == Int32.Parse(CompanyId));
            var completedBidCount = await dbContext.CommBidMstrs.CountAsync(bid => bid.BidStat == 2 && bid.CompId == Int32.Parse(CompanyId));

            ViewDashboardEvent events = new ViewDashboardEvent
            {
                DraftBid = draftBidCount,
                ActiveBid = activeBidCount,
                TotalBid = completedBidCount,
            };

            return Json(events);
        }

        // Action method to return category for a specific id
        [HttpGet]
        public async Task<IActionResult> GetBidEventsDetAjax(string bidId)
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            // Get category for the specified id
            List<CommBidMstr> bidList = await (from bidDet in dbContext.CommBidMstrs
                                          where bidDet.BidId == Int32.Parse(bidId) && bidDet.CompId == Int32.Parse(CompanyId)
                                           select new CommBidMstr
                                           {
                                               BidId = bidDet.BidId,
                                               Prod = new CommProdInfo { ProdName = bidDet.Prod.ProdName },
                                               Warehouse = new CommWarehsInfo { WareName = bidDet.Warehouse.WareName },
                                               BidDate = bidDet.BidDate,
                                               BidStrtTm = bidDet.BidStrtTm,
                                               BidEndTm = bidDet.BidEndTm,
                                               BidQnty = bidDet.BidQnty,
                                               BidRate = bidDet.BidRate,
                                               AllocRate = bidDet.AllocRate,
                                               NoOfPartyEng = dbContext.CommBidClntBidders
                                                              .Where(bInfo => bInfo.BidId == bidDet.BidId)
                                                              .Any() ? dbContext.CommBidClntBidders
                                                              .Count(bInfo => bInfo.BidId == bidDet.BidId) : 0,
                                               BidNote = bidDet.BidNote != null ? bidDet.BidNote : "",
                                               BidStat = bidDet.BidStat
                                           }).AsNoTracking().ToListAsync();
            return Json(bidList);
        }

        private void LoadViewBags()
        {
            ViewBag.UserFullName = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userFullName")?.Value;
            ViewBag.UserProfileImg = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "profilePic")?.Value;
        }
    }
}
