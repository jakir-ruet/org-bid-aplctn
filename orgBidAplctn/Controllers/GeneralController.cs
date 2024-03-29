using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using orgBidAplctn.Filters;
using orgBidAplctn.Models;
using orgBidAplctn.Models.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace orgBidAplctn.Controllers
{
    [TypeFilter(typeof(ViewBagActionFilter))]
    public class GeneralController : Controller
    {
        private AppDbContext dbContext = null;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public GeneralController(AppDbContext appDbContext, IWebHostEnvironment env)
        {
            this.dbContext = appDbContext;
            _hostingEnvironment = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region === Party Category ===
        public IActionResult PartyCat()
        {
            return View();
        }

        // Action method to return Party Category List based on date range
        [HttpGet]
        public async Task<IActionResult> GetPartyCatAjax()
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            // Get invoices based on the date range
            List<CommPartyCat> CatLists = await (from catList in dbContext.CommPartyCats
                                           where catList.CompId == Int32.Parse(CompanyId)
                                           select catList).AsNoTracking().ToListAsync();

            // Calculate the total result count
            int totalCount = CatLists.Count;

            // Create an anonymous object to include additional data
            var response = new
            {
                recordsTotal = totalCount,
                recordsFiltered = totalCount,
                data = CatLists
            };

            return Json(response);
        }

        // Action method to return category for a specific id
        [HttpGet]
        public async Task<IActionResult> GetPartyCatDetAjax(string categoryId)
        {
            // Get category for the specified id
            List<CommPartyCat> CatList = await (from catDet in dbContext.CommPartyCats
                                          where catDet.CatId == categoryId
                                          select catDet).AsNoTracking().ToListAsync();
            return Json(CatList);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrEditPartyCategory(string id, CommPartyCat model)
        {
            int UserCanMod = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userCanMod")?.Value);
            if (!string.IsNullOrEmpty(id))
            {
                if (UserCanMod == 0)
                {
                    return Json(new { success = false, message = "You are not authorize to change value ..." });
                }
            }
            // Check for Duplicate Category Name
            var duplicateName = await dbContext.CommPartyCats.AnyAsync(p => p.CatName == model.CatName && (model.CatId == null || p.CatId != model.CatId));
            if (duplicateName)
            {
                return Json(new { success = false, message = "Party Category Name Already Exist in Database ..." });
            }
            ModelState.Remove("model.CatId");
            if (ModelState.IsValid)
            {
                using (var context = new AppDbContext())
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            bool isNew = false;
                            string NewVoucherId = null;
                            // Check if the model contains a category ID to determine if it's an add or edit operation
                            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
                            string CompanyStartId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyStartId")?.Value.ToUpper();
                            if (string.IsNullOrEmpty(id) || id == "<< AUTO >>")
                            {
                                // Add new category
                                isNew = true;
                                var GetRecordQuery = await dbContext.GetVoucherIdPKs.FromSqlRaw("SELECT TOP 1 CASE WHEN EXISTS(SELECT TOP 1 CAT_ID FROM COMM_PARTY_CAT WHERE CAT_ID LIKE '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()),2) + RIGHT('00'+CAST(MONTH(GETDATE()) AS VARCHAR(2)),2) + '-%' ORDER BY CAT_ID DESC) THEN (SELECT TOP 1 '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()), 2) + RIGHT('00' + CAST(MONTH(GETDATE()) AS VARCHAR(2)), 2) + '-' + RIGHT('0000' + CAST(RIGHT(LTRIM(RTRIM(CAT_ID)), 4) + 1 AS VARCHAR(4)), 4) FROM COMM_PARTY_CAT WHERE CAT_ID LIKE '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()), 2) + RIGHT('00' + CAST(MONTH(GETDATE()) AS VARCHAR(2)), 2) + '-%' ORDER BY CAT_ID DESC) ELSE '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()), 2) + RIGHT('00' + CAST(MONTH(GETDATE()) AS VARCHAR(2)), 2) + '-' + '0001' END AS VoucherId FROM COMM_PARTY_CAT").AsNoTracking().FirstOrDefaultAsync();
                                if (GetRecordQuery == null)
                                {
                                    NewVoucherId = CompanyStartId + "-" + DateTime.Now.ToString("yyMM") + "-0001";
                                } else {
                                    NewVoucherId = GetRecordQuery.VoucherId;
                                }
                            }
                            CommPartyCat prodCat = isNew ? new CommPartyCat
                            {
                                CatId = NewVoucherId,
                                CompId = Int32.Parse(CompanyId)
                            } : await dbContext.CommPartyCats.FirstOrDefaultAsync(s => s.CatId == id);
                            prodCat.CatName = model.CatName;
                            prodCat.IsDiscontinue = model.IsDiscontinue;
                            if (isNew)
                            {
                                prodCat.AddBy = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId")?.Value);
                                await dbContext.AddAsync(prodCat);
                            }
                            else
                            {
                                dbContext.Update(prodCat);
                            }
                            await dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return Json(new { success = true, message = "Category added successfully." });
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
                return Json(new { success = false, message = "Invalid Category Data." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeletePartyCategory(string id)
        {
            int UserCanDel = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userCanDel")?.Value);
            if (!string.IsNullOrEmpty(id))
            {
                if (UserCanDel == 0)
                {
                    return Json(new { success = false, message = "You are not authorize to delete value ..." });
                }
            }
            else
            {
                return Json(new { success = false, message = "Party category not found !!!" });
            }

            // Check for Category ever used in Bid Information
            var catExistsInTbl = await dbContext.CommPartyInfos
                                            .AnyAsync(c => c.CatId == id);

            if (catExistsInTbl)
            {
                return Json(new { success = false, message = "There are Party information exist against this Category ! You can not delete it ..." });
            }

            // Retrieve the category based on the category ID
            var category = await dbContext.CommPartyCats.FindAsync(id);

            if (category == null)
            {
                // Handle the case when the category is not found
                return Json(new { success = false, message = "Party category not found !!!" });
            }

            // Remove the category from the context
            dbContext.CommPartyCats.Remove(category);

            try
            {
                // Save the changes to the database
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the deletion process
                return Json(new { success = false, message = "Exception: " + ex.Message });
            }

            // Return a success response
            return Json(new { success = true, message = "Party category deleted successfully !!!" });
        }
        #endregion

        #region === Product Category ===
        public IActionResult ProdCat()
        {
            return View();
        }

        // Action method to return Feedback List based on date range
        [HttpGet]
        public async Task<IActionResult> GetProdCatAjax()
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            // Get invoices based on the date range
            List<CommProdCat> catLists = await (from catList in dbContext.CommProdCats
                                          where catList.CompId == Int32.Parse(CompanyId)
                                          select catList).AsNoTracking().ToListAsync();

            // Calculate the total result count
            int totalCount = catLists.Count;

            // Create an anonymous object to include additional data
            var response = new
            {
                recordsTotal = totalCount,
                recordsFiltered = totalCount,
                data = catLists
            };

            return Json(response);
            // return Json(filteredInvoices);
        }

        // Action method to return category for a specific id
        [HttpGet]
        public async Task<IActionResult> GetProdCatDetAjax(string categoryId)
        {
            // Get category for the specified id
            List<CommProdCat> CatList = await (from catDet in dbContext.CommProdCats
                                         where catDet.CatId == categoryId
                                         select catDet).AsNoTracking().ToListAsync();
            return Json(CatList);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrEditProdCategory(string id, CommProdCat model)
        {
            int UserCanMod = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userCanMod")?.Value);
            if (!string.IsNullOrEmpty(id))
            {
                if (UserCanMod == 0)
                {
                    return Json(new { success = false, message = "You are not authorize to change value ..." });
                }
            }
            // Check for Duplicate Category Name
            var duplicateName = await dbContext.CommProdCats.AnyAsync(p => p.CatName == model.CatName && (model.CatId == null || p.CatId != model.CatId));
            if (duplicateName)
            {
                return Json(new { success = false, message = "Product Category Name Already Exist in Database ..." });
            }
            ModelState.Remove("model.CatId");
            if (ModelState.IsValid)
            {
                using (var context = new AppDbContext())
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            bool isNew = false;
                            string NewVoucherId = null;
                            // Check if the model contains a category ID to determine if it's an add or edit operation
                            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
                            string CompanyStartId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyStartId")?.Value.ToUpper();
                            if (string.IsNullOrEmpty(id) || id == "<< AUTO >>")
                            {
                                // Add new category
                                isNew = true;
                                var GetRecordQuery = await dbContext.GetVoucherIdPKs.FromSqlRaw("SELECT TOP 1 CASE WHEN EXISTS(SELECT TOP 1 CAT_ID FROM COMM_PROD_CAT WHERE CAT_ID LIKE '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()),2) + RIGHT('00'+CAST(MONTH(GETDATE()) AS VARCHAR(2)),2) + '-%' ORDER BY CAT_ID DESC) THEN (SELECT TOP 1 '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()), 2) + RIGHT('00' + CAST(MONTH(GETDATE()) AS VARCHAR(2)), 2) + '-' + RIGHT('0000' + CAST(RIGHT(LTRIM(RTRIM(CAT_ID)), 4) + 1 AS VARCHAR(4)), 4) FROM COMM_PROD_CAT WHERE CAT_ID LIKE '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()), 2) + RIGHT('00' + CAST(MONTH(GETDATE()) AS VARCHAR(2)), 2) + '-%' ORDER BY CAT_ID DESC) ELSE '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()), 2) + RIGHT('00' + CAST(MONTH(GETDATE()) AS VARCHAR(2)), 2) + '-' + '0001' END AS VoucherId FROM COMM_PROD_CAT").AsNoTracking().FirstOrDefaultAsync();
                                if (GetRecordQuery == null)
                                {
                                    NewVoucherId = CompanyStartId + "-" + DateTime.Now.ToString("yyMM") + "-0001";
                                } else {
                                    NewVoucherId = GetRecordQuery.VoucherId;
                                }
                            }
                            CommProdCat prodCat = isNew ? new CommProdCat
                            {
                                CatId = NewVoucherId,
                                CompId = Int32.Parse(CompanyId)
                            } : await dbContext.CommProdCats.FirstOrDefaultAsync(s => s.CatId == id);
                            prodCat.CatName = model.CatName;
                            prodCat.BaseUnit = model.BaseUnit;
                            prodCat.IsInvItem = model.IsInvItem;
                            prodCat.IsDiscontinue = model.IsDiscontinue;
                            if (isNew)
                            {
                                prodCat.AddBy = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId")?.Value);
                                await dbContext.AddAsync(prodCat);
                            }
                            else
                            {
                                dbContext.Update(prodCat);
                            }
                            await dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return Json(new { success = true, message = "Category added successfully." });
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
                return Json(new { success = false, message = "Invalid Category Data." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProdCategory(string id)
        {
            int UserCanDel = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userCanDel")?.Value);
            if (!string.IsNullOrEmpty(id))
            {
                if (UserCanDel == 0)
                {
                    return Json(new { success = false, message = "You are not authorize to delete value ..." });
                }
            } else {
                return Json(new { success = false, message = "Product category not found !!!" });
            }

            // Check for Category ever used in Bid Information
            var catExistsInTbl = await dbContext.CommProdInfos
                                            .AnyAsync(c => c.CatId == id);

            if (catExistsInTbl)
            {
                return Json(new { success = false, message = "There are Product information exist against this Category ! You can not delete it ..." });
            }

            // Retrieve the category based on the category ID
            var category = await dbContext.CommProdCats.FindAsync(id);

            if (category == null)
            {
                // Handle the case when the category is not found
                return Json(new { success = false, message = "Product category not found !!!" });
            }
            // var prodCatExist = await dbContext.CommPartyInfos.FirstOrDefaultAsync
            bool prodCatExist = await dbContext.CommProdInfos.AnyAsync(prod => prod.CatId == id);
            if (prodCatExist == true)
            {
                // Handle the case when the category is not found
                return Json(new { success = false, message = "Product against category found !!! You need to delete the product first before deleting category..." });
            }
            // Remove the category from the context
            dbContext.CommProdCats.Remove(category);

            try
            {
                // Save the changes to the database
                await dbContext.SaveChangesAsync();
            } catch (Exception ex) {
                // Handle any exceptions that occurred during the deletion process
                return Json(new { success = false, message = "Exception: " + ex.Message });
            }

            // Return a success response
            return Json(new { success = true, message = "Product category deleted successfully !!!" });
        }
        #endregion

        #region === Warehouse Details ===
        public IActionResult WareDet()
        {
            return View();
        }

        // Action method to return List based on date range
        [HttpGet]
        public async Task<IActionResult> GetWareListAjax()
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            // Get invoices based on the date range
            List<CommWarehsInfo> wareLists = await (from wareList in dbContext.CommWarehsInfos
                                          where wareList.CompId == Int32.Parse(CompanyId)
                                          select wareList).AsNoTracking().ToListAsync();

            // Calculate the total result count
            int totalCount = wareLists.Count;

            // Create an anonymous object to include additional data
            var response = new
            {
                recordsTotal = totalCount,
                recordsFiltered = totalCount,
                data = wareLists
            };

            return Json(response);
            // return Json(filteredInvoices);
        }

        // Action method to return category for a specific id
        [HttpGet]
        public async Task<IActionResult> GetWareDetAjax(string warehouseId)
        {
            // Get category for the specified id
            List<CommWarehsInfo> wareList = await (from wareDet in dbContext.CommWarehsInfos
                                            where wareDet.WareId == warehouseId
                                             select wareDet).AsNoTracking().ToListAsync();
            return Json(wareList);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrEditWarehouse(string id, CommWarehsInfo model)
        {
            int UserCanMod = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userCanMod")?.Value);
            if (!string.IsNullOrEmpty(id))
            {
                if (UserCanMod == 0)
                {
                    return Json(new { success = false, message = "You are not authorize to change value ..." });
                }
            }
            // Check for Duplicate Warehouse Name
            var duplicateName = await dbContext.CommWarehsInfos.AnyAsync(p => p.WareName == model.WareName && (model.WareId == null || p.WareId != model.WareId));
            if (duplicateName)
            {
                return Json(new { success = false, message = "Party Name Already Exist in Database ..." });
            }
            ModelState.Remove("model.WareId");
            if (ModelState.IsValid)
            {
                using (var context = new AppDbContext())
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            bool isNew = false;
                            string NewVoucherId = null;
                            // Check if the model contains a ID to determine if it's an add or edit operation
                            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
                            string CompanyStartId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyStartId")?.Value.ToUpper();
                            if (string.IsNullOrEmpty(id) || id == "<< AUTO >>")
                            {
                                // Add new data
                                isNew = true;
                                var GetRecordQuery = await dbContext.GetVoucherIdPKs.FromSqlRaw("SELECT TOP 1 CASE WHEN EXISTS(SELECT TOP 1 WARE_ID FROM COMM_WAREHS_INFO WHERE WARE_ID LIKE '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()),2) + RIGHT('00'+CAST(MONTH(GETDATE()) AS VARCHAR(2)),2) + '-%' ORDER BY WARE_ID DESC) THEN (SELECT TOP 1 '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()), 2) + RIGHT('00' + CAST(MONTH(GETDATE()) AS VARCHAR(2)), 2) + '-' + RIGHT('0000' + CAST(RIGHT(LTRIM(RTRIM(WARE_ID)), 4) + 1 AS VARCHAR(4)), 4) FROM COMM_WAREHS_INFO WHERE WARE_ID LIKE '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()), 2) + RIGHT('00' + CAST(MONTH(GETDATE()) AS VARCHAR(2)), 2) + '-%' ORDER BY WARE_ID DESC) ELSE '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()), 2) + RIGHT('00' + CAST(MONTH(GETDATE()) AS VARCHAR(2)), 2) + '-' + '0001' END AS VoucherId FROM COMM_WAREHS_INFO").AsNoTracking().FirstOrDefaultAsync();
                                if (GetRecordQuery == null)
                                {
                                    NewVoucherId = CompanyStartId + "-" + DateTime.Now.ToString("yyMM") + "-0001";
                                } else { 
                                    NewVoucherId = GetRecordQuery.VoucherId;
                                }
                            }
                            CommWarehsInfo warehsInfo = isNew ? new CommWarehsInfo
                            {
                                WareId = NewVoucherId,
                                CompId = Int32.Parse(CompanyId)
                            } : await dbContext.CommWarehsInfos.FirstOrDefaultAsync(s => s.WareId == id);
                            warehsInfo.WareName = model.WareName;
                            warehsInfo.WareAdd = model.WareAdd;
                            warehsInfo.IsDiscontinue = model.IsDiscontinue;
                            if (isNew)
                            {
                                warehsInfo.AddBy = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId")?.Value);
                                await dbContext.AddAsync(warehsInfo);
                            }
                            else
                            {
                                dbContext.Update(warehsInfo);
                            }
                            await dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return Json(new { success = true, message = "Warehouse added successfully." });
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
                return Json(new { success = false, message = "Invalid Warehouse Data." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWarehouse(string id)
        {
            int UserCanDel = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userCanDel")?.Value);
            if (!string.IsNullOrEmpty(id))
            {
                if (UserCanDel == 0)
                {
                    return Json(new { success = false, message = "You are not authorize to delete value ..." });
                }
            } else {
                return Json(new { success = false, message = "Warehouse not found !!!" });
            }

            // Check for Warehouse ever used in Bid Information
            var wareExistsInTbl = await dbContext.CommBidMstrs
                                            .AnyAsync(c => c.WareId == id);

            if (wareExistsInTbl)
            {
                return Json(new { success = false, message = "There are Bid information exist against this Warehouse ! You can not delete it ..." });
            }

            // Retrieve the category based on the category ID
            var category = await dbContext.CommWarehsInfos.FindAsync(id);

            if (category == null)
            {
                // Handle the case when the category is not found
                return Json(new { success = false, message = "Warehouse not found !!!" });
            }

            // Remove the category from the context
            dbContext.CommWarehsInfos.Remove(category);

            try
            {
                // Save the changes to the database
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the deletion process
                return Json(new { success = false, message = "Exception: " + ex.Message });
            }

            // Return a success response
            return Json(new { success = true, message = "Warehouse deleted successfully !!!" });
        }
        #endregion

        #region === Product Details ===
        public IActionResult ProdList()
        {
            return View();
        }

        // Action method to return List based on date range
        [HttpGet]
        public async Task<IActionResult> GetProdListAjax()
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            // Get invoices based on the date range
            //List<CommProdInfo> prodLists = (from prodList in dbContext.CommProdInfos.Include(o => o.Cat)
            //                                  where prodList.CompId == Int32.Parse(CompanyId)
            //                                  select prodList).AsNoTracking().ToList();

            List<CommProdInfo> prodLists = await (from prodList in dbContext.CommProdInfos.Include(o => o.Cat)
                                            where prodList.CompId == Int32.Parse(CompanyId)
                                            select new CommProdInfo
                                            {
                                                ProdId = prodList.ProdId,
                                                ProdName = prodList.ProdName,
                                                Cat = new CommProdCat { CatName = prodList.Cat.CatName },
                                                ProdCode = prodList.ProdCode,
                                                Wght = prodList.Wght,
                                                IsDiscontinue = prodList.IsDiscontinue
                                            }).AsNoTracking().ToListAsync();
            // Calculate the total result count
            int totalCount = prodLists.Count;

            // Create an anonymous object to include additional data
            var response = new
            {
                recordsTotal = totalCount,
                recordsFiltered = totalCount,
                data = prodLists
            };

            return Json(response);
        }

        // Action method to return details for a specific id
        [HttpGet]
        public async Task<IActionResult> GetProdDetAjax(string prodId)
        {
            // Get category for the specified id
            List<CommProdInfo> prodList = await (from prodDet in dbContext.CommProdInfos
                                           where prodDet.ProdId == prodId
                                           select prodDet).AsNoTracking().ToListAsync();
            return Json(prodList);
        }

        // Action method to return category for select combo
        [HttpGet]
        public async Task<IActionResult> SelProdCatAjax()
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            // Get category for the specified id
            List<CommProdCat> catList = await (from catDet in dbContext.CommProdCats
                                         where catDet.CompId == Int32.Parse(CompanyId) && catDet.IsDiscontinue == 0
                                         orderby catDet.CatName
                                         select new CommProdCat
                                         {
                                             CatId = catDet.CatId,
                                             CatName = catDet.CatName,
                                         }).AsNoTracking().ToListAsync();
            return Json(catList);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrEditProdDet(string id, CommProdInfo model)
        {
            int UserCanMod = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userCanMod")?.Value);
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            if (!string.IsNullOrEmpty(id))
            {
                if (UserCanMod == 0)
                {
                    return Json(new { success = false, message = "You are not authorize to change value ..." });
                }
            }
            // Check for Duplicate Product Name
            var duplicateName = await dbContext.CommProdInfos.AnyAsync(p => p.ProdName == model.ProdName && (model.ProdId == null || p.ProdId != model.ProdId));
            if (duplicateName)
            {
                return Json(new { success = false, message = "Product Name Already Exist in Database ..." });
            }
            // Check for Duplicate Product Code
            var duplicateCode = await dbContext.CommProdInfos.AnyAsync(p => p.ProdCode == model.ProdCode && (model.ProdId == null || p.ProdId != model.ProdId));
            if (duplicateCode)
            {
                return Json(new { success = false, message = "Product Code Already Exist in Database ..." });
            }
            ModelState.Remove("model.ProdId");
            if (ModelState.IsValid)
            {
                using (var context = new AppDbContext())
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            bool isNew = false;
                            string NewVoucherId = null;
                            string CompanyStartId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyStartId")?.Value.ToUpper();
                            if (string.IsNullOrEmpty(id) || id == "<< AUTO >>")
                            {
                                // Add new category
                                isNew = true;
                                var GetRecordQuery = await dbContext.GetVoucherIdPKs.FromSqlRaw("SELECT TOP 1 CASE WHEN EXISTS(SELECT TOP 1 PROD_ID FROM COMM_PROD_INFO WHERE PROD_ID LIKE '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()),2) + RIGHT('00'+CAST(MONTH(GETDATE()) AS VARCHAR(2)),2) + '-%' ORDER BY PROD_ID DESC) THEN (SELECT TOP 1 '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()), 2) + RIGHT('00' + CAST(MONTH(GETDATE()) AS VARCHAR(2)), 2) + '-' + RIGHT('0000' + CAST(RIGHT(LTRIM(RTRIM(PROD_ID)), 4) + 1 AS VARCHAR(4)), 4) FROM COMM_PROD_INFO WHERE PROD_ID LIKE '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()), 2) + RIGHT('00' + CAST(MONTH(GETDATE()) AS VARCHAR(2)), 2) + '-%' ORDER BY PROD_ID DESC) ELSE '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()), 2) + RIGHT('00' + CAST(MONTH(GETDATE()) AS VARCHAR(2)), 2) + '-' + '0001' END AS VoucherId FROM COMM_PROD_INFO").AsNoTracking().FirstOrDefaultAsync();
                                if (GetRecordQuery == null)
                                {
                                    NewVoucherId = CompanyStartId + "-" + DateTime.Now.ToString("yyMM") + "-0001";
                                }
                                else
                                {
                                    NewVoucherId = GetRecordQuery.VoucherId;
                                }
                            }
                            CommProdInfo prodDet = isNew ? new CommProdInfo
                            {
                                ProdId = NewVoucherId,
                                CompId = Int32.Parse(CompanyId)
                            } : await dbContext.CommProdInfos.FirstOrDefaultAsync(s => s.ProdId == id);
                            prodDet.ProdName = model.ProdName.Trim();
                            prodDet.ProdCode = model.ProdCode != null ? model.ProdCode.Trim() : "";
                            prodDet.BarcdId = model.BarcdId != null ? model.BarcdId.Trim() : "";
                            prodDet.CatId = model.CatId.Trim();
                            prodDet.Wght = model.Wght != null ? model.Wght.Trim() : "";
                            prodDet.ProdSpec = model.ProdSpec != null ? model.ProdSpec.Trim() : ""; 
                            prodDet.LeadTm = model.LeadTm;
                            prodDet.MinQnty = 0;
                            prodDet.MaxQnty = 0;
                            prodDet.IsDiscontinue = model.IsDiscontinue;
                            if (isNew)
                            {
                                prodDet.AddBy = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId")?.Value);
                                await dbContext.AddAsync(prodDet);
                            }
                            else
                            {
                                dbContext.Update(prodDet);
                            }
                            await dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return Json(new { success = true, message = "Product added successfully." });
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
                return Json(new { success = false, message = "Invalid Product Data." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            int UserCanDel = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userCanDel")?.Value);
            if (!string.IsNullOrEmpty(id))
            {
                if (UserCanDel == 0)
                {
                    return Json(new { success = false, message = "You are not authorize to delete value ..." });
                }
            } else {
                return Json(new { success = false, message = "Product not found !!!" });
            }

            // Check for Party ever used in Bid Information
            var prodExistsInTbl = await dbContext.CommBidMstrs
                                            .AnyAsync(c => c.ProdId == id);

            if (prodExistsInTbl)
            {
                return Json(new { success = false, message = "There are Bid information exist against this Product ! You can not delete it ..." });
            }

            // Retrieve the category based on the category ID
            var category = await dbContext.CommProdInfos.FindAsync(id);

            if (category == null)
            {
                // Handle the case when the category is not found
                return Json(new { success = false, message = "Product not found !!!" });
            }
            
            // Remove the category from the context
            dbContext.CommProdInfos.Remove(category);

            try
            {
                // Save the changes to the database
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the deletion process
                return Json(new { success = false, message = "Exception: " + ex.Message });
            }

            // Return a success response
            return Json(new { success = true, message = "Product deleted successfully !!!" });
        }

        #endregion

        #region === Party Details ===
        public IActionResult PartyList()
        {
            return View();
        }

        // Action method to return List based on date range
        [HttpGet]
        public async Task<IActionResult> GetPartyListAjax()
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            // Get invoices based on the date range
            //List<CommProdInfo> prodLists = (from prodList in dbContext.CommProdInfos.Include(o => o.Cat)
            //                                  where prodList.CompId == Int32.Parse(CompanyId)
            //                                  select prodList).AsNoTracking().ToList();

            List<CommPartyInfo> partyLists = await (from partyList in dbContext.CommPartyInfos.Include(o => o.Cat)
                                            where partyList.CompId == Int32.Parse(CompanyId)
                                            select new CommPartyInfo
                                            {
                                                PartyId = partyList.PartyId,
                                                PartyName = partyList.PartyName,
                                                Cat = new CommPartyCat { CatName = partyList.Cat.CatName },
                                                PartyAdd = partyList.PartyAdd,
                                                DistNm = partyList.DistNm,
                                                CntryNm = partyList.CntryNm,
                                                SmsContNo = partyList.SmsContNo,
                                                IsDiscontinue = partyList.IsDiscontinue
                                            }).AsNoTracking().ToListAsync();
            // Calculate the total result count
            int totalCount = partyLists.Count;

            // Create an anonymous object to include additional data
            var response = new
            {
                recordsTotal = totalCount,
                recordsFiltered = totalCount,
                data = partyLists
            };

            return Json(response);
        }

        // Action method to return details for a specific id
        [HttpGet]
        public async Task<IActionResult> GetPartyDetAjax(string partyId)
        {
            // Get category for the specified id
            List<CommPartyInfo> partyList = await (from partyDet in dbContext.CommPartyInfos
                                            where partyDet.PartyId == partyId
                                             select partyDet).AsNoTracking().ToListAsync();
            return Json(partyList);
        }

        // Action method to return category for select combo
        [HttpGet]
        public async Task<IActionResult> SelPartyCatAjax()
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            // Get category for the specified id
            List<CommPartyCat> catList = await (from catDet in dbContext.CommPartyCats
                                          where catDet.CompId == Int32.Parse(CompanyId) && catDet.IsDiscontinue == 0
                                         orderby catDet.CatName
                                         select new CommPartyCat
                                         {
                                             CatId = catDet.CatId,
                                             CatName = catDet.CatName,
                                         }).AsNoTracking().ToListAsync();
            return Json(catList);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrEditPartyDet(string id, CommPartyInfo model)
        {
            int UserCanMod = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userCanMod")?.Value);
            if (!string.IsNullOrEmpty(id))
            {
                if (UserCanMod == 0)
                {
                    return Json(new { success = false, message = "You are not authorize to change value ..." });
                }
            }
            // Check for Duplicate Party Name
            var duplicateName = await dbContext.CommPartyInfos.AnyAsync(p => p.PartyName == model.PartyName && (model.PartyId == null || p.PartyId != model.PartyId));
            if (duplicateName)
            {
                return Json(new { success = false, message = "Party Name Already Exist in Database ...." });
            }
            // Check for Duplicate Phone No
            var duplicateSMS = await dbContext.CommPartyInfos.AnyAsync(p => p.SmsContNo == model.SmsContNo && (model.PartyId == null || p.PartyId != model.PartyId));
            if (duplicateSMS)
            {
                return Json(new { success = false, message = "Mobile No Already Exist in Database ...." });
            }
            ModelState.Remove("model.PartyId");
            ModelState.Remove("model.CustType");
            if (ModelState.IsValid)
            {
                using (var context = new AppDbContext())
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            bool isNew = false;
                            string NewVoucherId = null;
                            // Check if the model contains a category ID to determine if it's an add or edit operation
                            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
                            string CompanyStartId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyStartId")?.Value.ToUpper();
                            if (string.IsNullOrEmpty(id) || id == "<< AUTO >>")
                            {
                                // Add new category
                                isNew = true;
                                var GetRecordQuery = await dbContext.GetVoucherIdPKs.FromSqlRaw("SELECT TOP 1 CASE WHEN EXISTS(SELECT TOP 1 PARTY_ID FROM COMM_PARTY_INFO WHERE PARTY_ID LIKE '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()),2) + RIGHT('00'+CAST(MONTH(GETDATE()) AS VARCHAR(2)),2) + '-%' ORDER BY PARTY_ID DESC) THEN (SELECT TOP 1 '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()), 2) + RIGHT('00' + CAST(MONTH(GETDATE()) AS VARCHAR(2)), 2) + '-' + RIGHT('0000' + CAST(RIGHT(LTRIM(RTRIM(PARTY_ID)), 4) + 1 AS VARCHAR(4)), 4) FROM COMM_PARTY_INFO WHERE PARTY_ID LIKE '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()), 2) + RIGHT('00' + CAST(MONTH(GETDATE()) AS VARCHAR(2)), 2) + '-%' ORDER BY PARTY_ID DESC) ELSE '" + CompanyStartId + "-' + RIGHT(YEAR(GETDATE()), 2) + RIGHT('00' + CAST(MONTH(GETDATE()) AS VARCHAR(2)), 2) + '-' + '0001' END AS VoucherId FROM COMM_PARTY_INFO").AsNoTracking().FirstOrDefaultAsync();
                                if (GetRecordQuery == null)
                                {
                                    NewVoucherId = CompanyStartId + "-" + DateTime.Now.ToString("yyMM") + "-0001";
                                }
                                else
                                {
                                    NewVoucherId = GetRecordQuery.VoucherId;
                                }
                            }
                            CommPartyInfo partyDet = isNew ? new CommPartyInfo
                            {
                                PartyId = NewVoucherId,
                                CompId = Int32.Parse(CompanyId)
                            } : await dbContext.CommPartyInfos.FirstOrDefaultAsync(s => s.PartyId == id);
                            partyDet.PartyName = model.PartyName.Trim();
                            partyDet.PartyCode = "";
                            partyDet.PartyAdd = model.PartyAdd != null ? model.PartyAdd.Trim() : "";
                            partyDet.DistNm = model.DistNm != null ? model.DistNm.Trim() : "";
                            partyDet.CntryNm = model.CntryNm != null ? model.CntryNm.Trim() : "";
                            partyDet.CatId = model.CatId.Trim();
                            partyDet.CustType = "C";
                            partyDet.SmsContNo = model.SmsContNo != null ? model.SmsContNo.Trim() : "";
                            partyDet.OthContNo = model.OthContNo != null ? model.OthContNo.Trim() : "";
                            partyDet.IsDiscontinue = model.IsDiscontinue;
                            if (isNew)
                            {
                                partyDet.AddBy = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId")?.Value);
                                await dbContext.AddAsync(partyDet);
                            }
                            else
                            {
                                dbContext.Update(partyDet);
                            }
                            await dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return Json(new { success = true, message = "Party added successfully." });
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
        public async Task<IActionResult> DeleteParty(string id)
        {
            int UserCanDel = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userCanDel")?.Value);
            if (!string.IsNullOrEmpty(id))
            {
                if (UserCanDel == 0)
                {
                    return Json(new { success = false, message = "You are not authorize to delete value ..." });
                }
            } else {
                return Json(new { success = false, message = "Party not found !!!" });
            }

            // Check for Party ever used in Bid Information
            var partyExistsInCommBidClnt = await dbContext.CommBidClntBidders
                                            .AnyAsync(c => c.PartyId == id);

            if (partyExistsInCommBidClnt)
            {
                return Json(new { success = false, message = "There are Bid information exist against this Party ! You can not delete it ..." });
            }

            // Retrieve the information based on the Party ID
            var category = await dbContext.CommPartyInfos.FindAsync(id);

            if (category == null)
            {
                // Handle the case when the category is not found
                return Json(new { success = false, message = "Party not found !!!" });
            }

            // Remove the category from the context
            dbContext.CommPartyInfos.Remove(category);

            try
            {
                // Save the changes to the database
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the deletion process
                return Json(new { success = false, message = "Exception: " + ex.Message });
            }

            // Return a success response
            return Json(new { success = true, message = "Party deleted successfully !!!" });
        }

        #endregion

        // Action method to return Country List
        [HttpGet]
        public async Task<IActionResult> GetCountryAjax()
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            // Get invoices based on the date range
            var countries = await dbContext.CommCountryInfos
                .OrderBy(c => c.CntryName)
                .Select(c => new { id = c.CntryName, text = c.CntryName })
                .AsNoTracking().ToListAsync();

            return Json(countries);
        }

        private void LoadViewBags()
        {
            ViewBag.UserFullName = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userFullName")?.Value;
            ViewBag.UserProfileImg = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "profilePic")?.Value;
        }
    }
}
