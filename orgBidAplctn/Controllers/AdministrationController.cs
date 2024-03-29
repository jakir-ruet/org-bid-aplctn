using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using orgBidAplctn.Filters;
using orgBidAplctn.Models;
using orgBidAplctn.Models.Data;
using orgBidAplctn.Models.DataViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace orgBidAplctn.Controllers
{
    [TypeFilter(typeof(ViewBagActionFilter))]
    public class AdministrationController : Controller
    {
        private AppDbContext dbContext = null;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AdministrationController(AppDbContext appDbContext, IWebHostEnvironment env)
        {
            this.dbContext = appDbContext;
            _hostingEnvironment = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CompanyDet()
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            var UserId = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId")?.Value);
            int UsrAccType = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "accessType")?.Value);
            if (UsrAccType != 3)
            {
                return Json(new { success = false, message = "You are not authorize to see or edit value ..." });
            }
            CommCompInfo compDet = await dbContext.CommCompInfos.AsNoTracking().FirstOrDefaultAsync(c => c.CompId == Int32.Parse(CompanyId));
            return View(compDet);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCompDet(CommCompInfo model)
        {
            var UserId = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId")?.Value);
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            int UsrAccType = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "accessType")?.Value);
            int UserCanMod = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userCanMod")?.Value);
            if (UsrAccType != 3)
            {
                return Json(new { success = false, message = "You are not authorize to see or edit value ..." });
            }
            if (UserCanMod == 0)
            {
                return Json(new { success = false, message = "You are not authorize to change value ..." });
            }
            ModelState.Remove("model.CompStrtNo");
            ModelState.Remove("model.DefCurr");
            if (ModelState.IsValid)
            {
                using (var context = new AppDbContext())
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            CommCompInfo compDet = await dbContext.CommCompInfos.FirstOrDefaultAsync(s => s.CompId == Int32.Parse(CompanyId));
                            compDet.CompName = model.CompName.Trim();
                            compDet.Address = model.Address != null ? model.Address.Trim() : "";
                            compDet.CompArea = model.CompArea != null ? model.CompArea.Trim() : "";
                            compDet.CompDist = model.CompDist != null ? model.CompDist.Trim() : "";
                            compDet.CompCountry = model.CompCountry != null ? model.CompCountry.Trim() : "";
                            compDet.ContPersn = model.ContPersn != null ? model.ContPersn.Trim() : "";
                            compDet.ContEmail = model.ContEmail != null ? model.ContEmail.Trim() : "";
                            compDet.ContOthNo = model.ContOthNo != null ? model.ContOthNo.Trim() : "";
                            dbContext.Update(compDet);
                            await dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return Json(new { success = true, message = "Company updated successfully." });
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
            }
        }

        public IActionResult UserList()
        {
            return View();
        }

        public async Task<IActionResult> GetUserListAjax()
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            List<CommLoginInfo> userLists = await (from userList in dbContext.CommLoginInfos
                                              where userList.CompId == Int32.Parse(CompanyId)
                                              select new CommLoginInfo
                                              {
                                                  UserId = userList.UserId,
                                                  UserNm = userList.UserNm,
                                                  FastName = userList.FastName,
                                                  LastName = userList.LastName,
                                                  LoginTp = userList.LoginTp,
                                                  CanMod = userList.CanMod,
                                                  CanDel = userList.CanDel,
                                                  IsActive = userList.IsActive
                                              }).AsNoTracking().ToListAsync();
            // Calculate the total result count
            int totalCount = userLists.Count;

            // Create an anonymous object to include additional data
            var response = new
            {
                recordsTotal = totalCount,
                recordsFiltered = totalCount,
                data = userLists
            };

            return Json(response);
        }

        public async Task<IActionResult> GetUserDetAjax(string userId)
        {
            // Get category for the specified id
            List<CommLoginInfo> partyList = await (from userDet in dbContext.CommLoginInfos
                                             where userDet.UserId == Int32.Parse(userId)
                                             select new CommLoginInfo
                                             {
                                                 UserId = userDet.UserId,
                                                 UserNm = userDet.UserNm,
                                                 FastName = userDet.FastName,
                                                 LastName = userDet.LastName,
                                                 LoginTp = userDet.LoginTp,
                                                 CanMod = userDet.CanMod,
                                                 CanDel = userDet.CanDel,
                                                 IsActive = userDet.IsActive
                                             }).AsNoTracking().ToListAsync();
            return Json(partyList);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrEditUser(string id, CommLoginInfo model)
        {
            int UserCanMod = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userCanMod")?.Value);
            if (!string.IsNullOrEmpty(id))
            {
                if (UserCanMod == 0)
                {
                    return Json(new { success = false, message = "You are not authorize to change value ..." });
                }
            }
            // Check for Duplicate User Name
            var duplicateName = await dbContext.CommLoginInfos.AnyAsync(p => p.UserNm == model.UserNm && (model.UserId != 0 ? p.UserId != model.UserId : true));
            if (duplicateName)
            {
                return Json(new { success = false, message = "User Name Already Exist in Database ..." });
            }
            string CurrPasswords = "";
            ModelState.Remove("model.UserPass");
            if (ModelState.IsValid)
            {
                using (var context = new AppDbContext())
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            bool isNew = false;
                            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
                            string CompanyStartId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyStartId")?.Value.ToUpper();
                            if (string.IsNullOrEmpty(id) || id == "<< AUTO >>")
                            {
                                // Add new category
                                isNew = true;
                                CurrPasswords = MD5Hash(model.UserPass.ToString());
                            }
                            CommLoginInfo userDet = isNew ? new CommLoginInfo
                            {
                                UserPass = CurrPasswords,
                                CompId = Int32.Parse(CompanyId)
                            } : await dbContext.CommLoginInfos.FirstOrDefaultAsync(s => s.UserId == Int32.Parse(id));
                            userDet.UserNm = model.UserNm.Trim();
                            userDet.FastName = model.FastName.Trim();
                            userDet.LastName = model.LastName != null ? model.LastName.Trim() : "";
                            userDet.LoginTp = model.LoginTp;
                            userDet.IsActive = model.IsActive;
                            userDet.CanMod = model.CanMod;
                            userDet.CanDel = model.CanDel;
                            if (isNew)
                            {
                                await dbContext.AddAsync(userDet);
                            } else {
                                dbContext.Update(userDet);
                            }
                            await dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return Json(new { success = true, message = "User added successfully." });
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
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswd(string id)
        {
            int UserCanMod = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userCanMod")?.Value);
            if (!string.IsNullOrEmpty(id))
            {
                if (UserCanMod == 0)
                {
                    return Json(new { success = false, message = "You are not authorize to change value ..." });
                }
            }
            else
            {
                return Json(new { success = false, message = "User not found !!!" });
            }
            string NewPasswords = MD5Hash("123456");
            // Retrieve the user based on the user ID
            CommLoginInfo loginDet = await dbContext.CommLoginInfos.FirstOrDefaultAsync(s => s.UserId == Int32.Parse(id));

            if (loginDet == null)
            {
                // Handle the case when the category is not found
                return Json(new { success = false, message = "User not found !!!" });
            }

            using (var context = new AppDbContext())
            {
                using (var transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        loginDet.UserPass = NewPasswords;
                        dbContext.Update(loginDet);
                        await dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return Json(new { success = true, message = "Password updated successfully." });
                    } catch (DbUpdateException ex1) {
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
                    } catch (Exception ex) {
                        await transaction.RollbackAsync();
                        ViewBag.ErrorMessage = "Exception: " + ex.Message;
                        // throw ex;
                        return Json(new { success = false, message = "Exception: " + ex.Message });
                    }
                }
            }
        }

        public async Task<IActionResult> UploadExcelFile(UploadViewModel model)
        {
            ViewBag.UserFullName = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userFullName")?.Value;
            int UsrAccType = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "accessType")?.Value);
            if (UsrAccType != 3)
            {
                return Json(new { success = false, message = "You are not authorize to see or edit value ..." });
            }
            if (model.ExcelFile != null && model.ExcelFile.Length > 0)
            {
                // Read the Excel file and extract the data
                using (var stream = model.ExcelFile.OpenReadStream())
                {
                    var dataTable = new DataTable();
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.Columns])
                        {
                            dataTable.Columns.Add(firstRowCell.Text.Trim());
                        }

                        for (var row = 2; row <= worksheet.Dimension.Rows; row++)
                        {
                            var newRow = dataTable.NewRow();
                            for (var col = 1; col <= worksheet.Dimension.Columns; col++)
                            {
                                newRow[col - 1] = worksheet.Cells[row, col].Value?.ToString()?.Trim();
                            }
                            dataTable.Rows.Add(newRow);
                        }
                    }

                    // Save the data to the database
                    foreach (DataRow row in dataTable.Rows)
                    {
                        var countryCode = row["CountryCode"].ToString();
                        var countryName = row["CountryName"].ToString();

                        var country = new CommCountryInfo
                        {
                            CntryId = countryCode,
                            CntryName = countryName
                        };

                        // Save the country to the database
                        await dbContext.CommCountryInfos.AddAsync(country);
                        await dbContext.SaveChangesAsync();
                    }
                }

                return RedirectToAction("Index");
            }

            return View(model);
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

        private void LoadViewBags()
        {
            ViewBag.UserFullName = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userFullName")?.Value;
            ViewBag.UserProfileImg = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "profilePic")?.Value;
        }
    }
}
