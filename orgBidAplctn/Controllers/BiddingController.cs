using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using orgBidAplctn.Filters;
using orgBidAplctn.Models;
using orgBidAplctn.Models.Data;
using orgBidAplctn.Models.DataViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace orgBidAplctn.Controllers
{
    [TypeFilter(typeof(ViewBagActionFilter))]
    public class BiddingController : Controller
    {
        private AppDbContext dbContext = null;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public BiddingController(AppDbContext appDbContext, IWebHostEnvironment env)
        {
            this.dbContext = appDbContext;
            _hostingEnvironment = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult BidMngr(string bidStat = "ALL")
        {
            ViewBag.BidStatus = bidStat;
            return View();
        }

        public IActionResult SchedulMngr()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetBidListAjax(DateTime startDate, DateTime endDate, [FromBody] DtParameters dtParameters)
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            endDate = endDate.Date.AddDays(1).AddSeconds(-1); // Set the time component to 23:59:59
            // https://stackoverflow.com/questions/64292529/datatables-server-side-processing-in-asp-net-core-3-1
            var searchBy = dtParameters.Search?.Value;

            IQueryable<CommBidMstr> qryBidList = (from bidList in dbContext.CommBidMstrs.Include(o => o.Prod)
                                                 where bidList.CompId == Int32.Parse(CompanyId)
                                                 where bidList.BidDate >= startDate && bidList.BidDate <= endDate
                                                 orderby bidList.BidId descending
                                                 select new CommBidMstr
                                                 {
                                                     BidId = bidList.BidId,
                                                     BidDate = bidList.BidDate,
                                                     BidStrtTm = bidList.BidStrtTm,
                                                     BidEndTm = bidList.BidEndTm,
                                                     Prod = new CommProdInfo { ProdName = bidList.Prod.ProdName },
                                                     BidQnty = bidList.BidQnty,
                                                     BidRate = bidList.BidRate,
                                                     AllocRate = bidList.AllocRate,
                                                     BidNote = bidList.BidNote,
                                                     BidStat = bidList.BidStat,
                                                     NoOfPartyEng = dbContext.CommBidClntBidders
                                                                .Where(bInfo => bInfo.BidId == bidList.BidId)
                                                                .Any() ? dbContext.CommBidClntBidders
                                                                            .Count(bInfo => bInfo.BidId == bidList.BidId):0
                                                 }).AsNoTracking();

            if (!string.IsNullOrEmpty(searchBy))
            {
                qryBidList = qryBidList.Where(r => r.BidId.ToString() != null && r.BidId.ToString().Contains(searchBy.ToUpper()) ||
                                            r.Prod.ProdName != null && r.Prod.ProdName.ToUpper().Contains(searchBy.ToUpper()));
            }

            // now just get the count of items (without the skip and take) - eg how many could be returned with filtering
            var filteredResultsCount = await qryBidList.CountAsync();
            var totalResultsCount = await dbContext.CommBidMstrs.CountAsync();

            // Apply sorting
            if (dtParameters.Order != null && dtParameters.Order.Length > 0)
            {
                DtOrder order = dtParameters.Order[0];
                string sortField = dtParameters.Columns[order.Column].Data;
                bool ascending = order.Dir == DtOrderDir.Asc;
                qryBidList = ApplySorting(qryBidList, sortField, ascending);
            }

            int start = dtParameters.Start >= 0 ? dtParameters.Start : 0;
            int length = dtParameters.Length > 0 ? dtParameters.Length : 10;

            return Json(new DtResult<CommBidMstr>
            {
                Draw = dtParameters.Draw,
                RecordsTotal = totalResultsCount,
                RecordsFiltered = filteredResultsCount,
                //Data = await qryBidList.ToListAsync()
                Data = await qryBidList
                    .Skip(dtParameters.Start)
                    .Take(dtParameters.Length)
                    .ToListAsync()
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetBidListCustAjax(string BidStatus, [FromBody] DtParameters dtParameters)
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            // https://stackoverflow.com/questions/64292529/datatables-server-side-processing-in-asp-net-core-3-1
            var searchBy = dtParameters.Search?.Value;

            IQueryable<CommBidMstr> qryBidList = (from bidList in dbContext.CommBidMstrs.Include(o => o.Prod)
                                                  where bidList.BidStat == Int32.Parse(BidStatus) && bidList.CompId == Int32.Parse(CompanyId)
                                                  orderby bidList.BidId descending
                                                  select new CommBidMstr
                                                  {
                                                      BidId = bidList.BidId,
                                                      BidDate = bidList.BidDate,
                                                      BidStrtTm = bidList.BidStrtTm,
                                                      BidEndTm = bidList.BidEndTm,
                                                      Prod = new CommProdInfo { ProdName = bidList.Prod.ProdName },
                                                      BidQnty = bidList.BidQnty,
                                                      BidRate = bidList.BidRate,
                                                      AllocRate = bidList.AllocRate,
                                                      BidNote = bidList.BidNote,
                                                      BidStat = bidList.BidStat,
                                                      NoOfPartyEng = dbContext.CommBidClntBidders
                                                                 .Where(bInfo => bInfo.BidId == bidList.BidId)
                                                                 .Any() ? dbContext.CommBidClntBidders
                                                                             .Count(bInfo => bInfo.BidId == bidList.BidId) : 0
                                                  }).AsNoTracking();

            if (!string.IsNullOrEmpty(searchBy))
            {
                qryBidList = qryBidList.Where(r => r.BidId.ToString() != null && r.BidId.ToString().Contains(searchBy.ToUpper()) ||
                                            r.Prod.ProdName != null && r.Prod.ProdName.ToUpper().Contains(searchBy.ToUpper()));
            }

            // now just get the count of items (without the skip and take) - eg how many could be returned with filtering
            var filteredResultsCount = await qryBidList.CountAsync();
            var totalResultsCount = await dbContext.CommBidMstrs.CountAsync();

            // Apply sorting
            if (dtParameters.Order != null && dtParameters.Order.Length > 0)
            {
                DtOrder order = dtParameters.Order[0];
                string sortField = dtParameters.Columns[order.Column].Data;
                bool ascending = order.Dir == DtOrderDir.Asc;
                qryBidList = ApplySorting(qryBidList, sortField, ascending);
            }

            int start = dtParameters.Start >= 0 ? dtParameters.Start : 0;
            int length = dtParameters.Length > 0 ? dtParameters.Length : 10;

            return Json(new DtResult<CommBidMstr>
            {
                Draw = dtParameters.Draw,
                RecordsTotal = totalResultsCount,
                RecordsFiltered = filteredResultsCount,
                //Data = await qryBidList.ToListAsync()
                Data = await qryBidList
                    .Skip(dtParameters.Start)
                    .Take(dtParameters.Length)
                    .ToListAsync()
            });
        }

        // Helper method to apply sorting based on column name and direction
        private IQueryable<CommBidMstr> ApplySorting(IQueryable<CommBidMstr> query, string sortField, bool ascending)
        {
            switch (sortField)
            {
                case "bidId":
                    return ascending ? query.OrderBy(g => g.BidId) : query.OrderByDescending(g => g.BidId);
                case "bidDate":
                    return ascending ? query.OrderBy(g => g.BidDate) : query.OrderByDescending(g => g.BidDate);
                case "bidStrtTm":
                    return ascending ? query.OrderBy(g => g.BidStrtTm) : query.OrderByDescending(g => g.BidStrtTm);
                case "prod.prodName":
                    return ascending ? query.OrderBy(g => g.Prod.ProdName) : query.OrderByDescending(g => g.Prod.ProdName);
                case "noOfPartyEng":
                    return ascending ? query.OrderBy(g => g.NoOfPartyEng) : query.OrderByDescending(g => g.NoOfPartyEng);
                case "bidQnty":
                    return ascending ? query.OrderBy(g => g.BidQnty) : query.OrderByDescending(g => g.BidQnty);
                case "bidRate":
                    return ascending ? query.OrderBy(g => g.BidRate) : query.OrderByDescending(g => g.BidRate);
                case "allocRate":
                    return ascending ? query.OrderBy(g => g.AllocRate) : query.OrderByDescending(g => g.AllocRate);
                case "bidStat":
                    return ascending ? query.OrderBy(g => g.BidStat) : query.OrderByDescending(g => g.BidStat);
                // Add other sorting options for additional columns if needed
                default:
                    return query;
            }
        }

        // Action method to return Party List
        [HttpGet]
        public async Task<IActionResult> GetPartyAjax()
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;

            // Get invoices based on the date range asynchronously
            var partyList = await dbContext.CommPartyInfos
                .Where(c => c.CompId == Int32.Parse(CompanyId) && c.IsDiscontinue == 0)
                .OrderBy(c => c.PartyName)
                .Select(c => new { id = c.PartyId, text = c.PartyName })
                .AsNoTracking().ToListAsync();

            return Json(partyList);
        }

        // Action method to return Product List
        [HttpGet]
        public async Task<IActionResult> GetProductAjax()
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            // Get invoices based on the date range
            var prodList = await dbContext.CommProdInfos
                .Where(c => c.CompId == Int32.Parse(CompanyId) && c.IsDiscontinue == 0)
                .OrderBy(c => c.ProdName)
                .Select(c => new { id = c.ProdId, text = c.ProdName })
                .AsNoTracking().ToListAsync();

            return Json(prodList);
        }

        // Action method to return Warehouse List
        [HttpGet]
        public async Task<IActionResult> GetWarehouseAjax()
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            // Get invoices based on the date range
            var wareList = await dbContext.CommWarehsInfos
                .Where(c => c.CompId == Int32.Parse(CompanyId) && c.IsDiscontinue == 0)
                .OrderBy(c => c.WareName)
                .Select(c => new { id = c.WareId, text = c.WareName })
                .AsNoTracking().ToListAsync();

            return Json(wareList);
        }

        [HttpGet]
        public async Task<IActionResult> GetSmsSett()
        {
            var smsSetList = await dbContext.CommSmsSetts
                .AsNoTracking().FirstOrDefaultAsync();

            return Json(smsSetList);
        }

        [HttpPost]
        public async Task<IActionResult> BidAddProc(string id, BidOrderDetailModel bidOrderDetailModel)
        {
            ModelState.Remove("CommBidMstrs.BidId");
            var collection = bidOrderDetailModel.CommBidClntBidders;

            // Iterate over each item in the collection
            for (int i = 0; i < collection.Count; i++)
            {
                var prefix = $"CommBidClntBidders[{i}]";
                // Remove model state entries for each property of the item
                ModelState.Remove($"{prefix}.BidId");
                ModelState.Remove($"{prefix}.SmsContNo");
                // Add more properties as needed
            }
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Model State Validation Error Found !";
                // Return the JSON error response
                return Json(new { result = false, message = "Model State Validation Error Found !" });
            }
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
                            isNew = true;
                        }
                        CommBidMstr bidMstr = isNew ? new CommBidMstr
                        {
                            BidId = GenerateCustomBidId(),
                            CompId = Int32.Parse(CompanyId)
                        } : await dbContext.CommBidMstrs.FirstOrDefaultAsync(s => s.BidId == Int32.Parse(id));
                        bidMstr.BidDate = bidOrderDetailModel.CommBidMstrs.BidDate;
                        bidMstr.BidStrtTm = bidOrderDetailModel.CommBidMstrs.BidStrtTm;
                        bidMstr.BidEndTm = bidOrderDetailModel.CommBidMstrs.BidEndTm;
                        bidMstr.ProdId = bidOrderDetailModel.CommBidMstrs.ProdId;
                        bidMstr.WareId = bidOrderDetailModel.CommBidMstrs.WareId;
                        bidMstr.BidQnty = bidOrderDetailModel.CommBidMstrs.BidQnty;
                        bidMstr.BidRate = bidOrderDetailModel.CommBidMstrs.BidRate;
                        bidMstr.BidStat = bidOrderDetailModel.CommBidMstrs.BidStat;
                        bidMstr.BidNote = bidOrderDetailModel.CommBidMstrs.BidNote;
                        if (isNew)
                        {
                            bidMstr.AddBy = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userName")?.Value;
                            await dbContext.AddAsync(bidMstr);
                        }
                        else
                        {
                            dbContext.Update(bidMstr);
                        }
                        await dbContext.SaveChangesAsync();

                        int LineNo = 1;
                        string partySMSNo = "";
                        List<CommBidClntBidder> bidClnt = new List<CommBidClntBidder>();
                        foreach (var clntBidList in bidOrderDetailModel.CommBidClntBidders)
                        {
                            if (isNew || Convert.ToDecimal(clntBidList.SysId) == 0)
                            {
                                partySMSNo = dbContext.CommPartyInfos.FirstOrDefault(p => p.PartyId == clntBidList.PartyId)?.SmsContNo;
                                bidClnt.Add(new CommBidClntBidder
                                {
                                    BidId = bidMstr.BidId,
                                    PartyId = clntBidList.PartyId.ToUpper(),
                                    SmsContNo = partySMSNo.ToUpper()
                                });
                            }
                            else
                            {
                                // Modified Product Line Item Found
                                var existingItem = await dbContext.CommBidClntBidders.FindAsync(Convert.ToDecimal(clntBidList.SysId));
                                if (existingItem != null)
                                {
                                    // Update properties of the existing item
                                    existingItem.BidId = clntBidList.BidId;
                                    existingItem.PartyId = clntBidList.PartyId.ToUpper();
                                    partySMSNo = dbContext.CommPartyInfos.FirstOrDefault(p => p.PartyId == clntBidList.PartyId)?.SmsContNo;
                                    existingItem.SmsContNo = partySMSNo.ToUpper();
                                    // Mark the entity as modified
                                    dbContext.Entry(existingItem).State = EntityState.Modified;
                                }
                            }
                            LineNo += 1;
                        }
                        await dbContext.AddRangeAsync(bidClnt);
                        await dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        if (isNew)
                        {
                            return Json(new { success = true, message = "Bid added successfully." });
                        } else { 
                            return Json(new { result = "Redirect", redirectToUrl = Url.Action("BidMngr", "Bidding") });
                        }
                    }
                    catch (DbUpdateException ex1)
                    {
                        await transaction.RollbackAsync();
                        var Message = ex1.Message;
                        if (ex1.InnerException != null)
                        {
                            Message += " : " + ex1.InnerException.Message;
                        }
                        ViewBag.ErrorMessage = "Exception: " + Message;
                        // throw ex1;
                        return Json(new { result = false, message = "Exception: " + Message });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ViewBag.ErrorMessage = "Exception: " + ex.Message;
                        // throw ex;
                        return Json(new { result = false, message = "Exception: " + ex.Message });
                    }
                }
            }
        }

        private long GenerateCustomBidId()
        {
            // Get the current year and month
            int year = DateTime.Now.Year % 100; // Get the last two digits of the year
            int month = DateTime.Now.Month;
            long customBidId;
            long targetBidId = long.Parse($"{year:D2}{month:D2}0000"); // Create the target BidId as a long

            // Query the database to find the last used serial number for the current year and month
            var lastRecord = dbContext.CommBidMstrs
                                .Where(bid => bid.BidId >= targetBidId && bid.BidId < (targetBidId + 10000)) // Assuming a maximum of 10,000 records per month
                                .OrderByDescending(bid => bid.BidId)
                                .FirstOrDefault();

            if (lastRecord != null)
            {
                // Extract the serial number part and increment it
                long serialNumber = lastRecord.BidId - targetBidId + 1;

                // Generate the new BidId as a long
                customBidId = targetBidId + serialNumber;

                // Now, 'customBidId' contains the new BidId
            }
            else
            {
                // No records found for the current year and month, start from 1
                customBidId = targetBidId + 1;
            }

            return customBidId;
        }

        //private string GenerateCustBidId()
        //{
        //    // Get the current year and month
        //    int year = DateTime.Now.Year % 100; // Get the last two digits of the year
        //    int month = DateTime.Now.Month;
        //    string customBidId;
        //    string yearMonthPrefix = $"{year:D2}{month:D2}";

        //    var lastRecord = dbContext.CommBidMstrs
        //        .Where(bid => bid.BidCustId.StartsWith(yearMonthPrefix))
        //        .OrderByDescending(bid => bid.BidCustId)
        //        .FirstOrDefault();

        //    if (lastRecord != null)
        //    {
        //        // Extract the serial number part and increment it
        //        int serialNumber = int.Parse(lastRecord.BidCustId.Substring(4));
        //        serialNumber++; // Increment the serial number

        //        // Generate the new BidCustId
        //        customBidId = $"{year:D2}{month:D2}{serialNumber:D4}";

        //        // Now, 'customBidId' contains the new BidCustId
        //    }
        //    else
        //    {
        //        // No records found for the current year and month, start from 1
        //        int serialNumber = 1;
        //        customBidId = $"{year:D2}{month:D2}{serialNumber:D4}";
        //    }

        //    return customBidId;
        //}

        [HttpPost]
        public async Task<IActionResult> BidEditProc(string id, CommBidMstr model)
        {
            ModelState.Remove("CommBidMstrs.BidId");
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Model State Validation Error Found !";
                // Return the JSON error response
                return Json(new { result = "Error", message = "Model State Validation Error Found !" });
            }
            using (var context = new AppDbContext())
            {
                using (var transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
                        string CompanyStartId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyStartId")?.Value.ToUpper();
                        CommBidMstr bidMstr = await dbContext.CommBidMstrs.FirstOrDefaultAsync(s => s.BidId == Int32.Parse(id));
                        // If Bid Activated == 1 Then Other Information Will Not Be Updated ...
                        if (bidMstr.BidStat == 0)
                        {
                            bidMstr.BidDate = model.BidDate;
                            bidMstr.BidStrtTm = model.BidStrtTm;
                            bidMstr.ProdId = model.ProdId;
                            bidMstr.WareId = model.WareId;
                            bidMstr.BidQnty = model.BidQnty;
                            bidMstr.BidRate = model.BidRate;
                        }
                        bidMstr.BidEndTm = model.BidEndTm;
                        bidMstr.BidNote = model.BidNote;
                        dbContext.Update(bidMstr);
                        await dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        await CreateNotificationAsync("Bid # " + id + " Successfully <strong>Modified</strong>", 1);
                        return Json(new { success = true, message = "Bid updated successfully." });
                    }
                    catch (DbUpdateException ex1)
                    {
                        await transaction.RollbackAsync();
                        var Message = ex1.Message;
                        if (ex1.InnerException != null)
                        {
                            Message += " : " + ex1.InnerException.Message;
                        }
                        ViewBag.ErrorMessage = "Exception: " + Message;
                        // throw ex1;
                        return Json(new { result = "Error", message = "Exception: " + Message });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ViewBag.ErrorMessage = "Exception: " + ex.Message;
                        // throw ex;
                        return Json(new { result = "Error", message = "Exception: " + ex.Message });
                    }
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> BidAddPartyProc(string id, List<CommBidClntBidder> model)
        {
            ModelState.Clear();

            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Model State Validation Error Found !";
                // Return the JSON error response
                return Json(new { success = false, message = "Model State Validation Error Found !" });
            }
            using (var context = new AppDbContext())
            {
                using (var transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
                        string CompanyStartId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyStartId")?.Value.ToUpper();
                        int LineNo = 1;
                        string partySMSNo = "";
                        List<CommBidClntBidder> bidClnt = new List<CommBidClntBidder>();
                        foreach (var clntBidList in model)
                        {
                            var partyExist = await dbContext.CommBidClntBidders.AnyAsync(p => p.PartyId == clntBidList.PartyId && p.BidId == Int32.Parse(id));
                            if (!partyExist)
                            {
                                // partySMSNo = dbContext.CommPartyInfos.FirstOrDefault(p => p.PartyId == clntBidList.PartyId)?.SmsContNo;
                                partySMSNo = await dbContext.CommPartyInfos
                                                    .Where(p => p.PartyId == clntBidList.PartyId)
                                                    .Select(p => p.SmsContNo)
                                                    .FirstOrDefaultAsync();

                                bidClnt.Add(new CommBidClntBidder
                                {
                                    BidId = Int32.Parse(id),
                                    PartyId = clntBidList.PartyId.ToUpper(),
                                    SmsContNo = partySMSNo.ToUpper()
                                });
                            }
                            LineNo += 1;
                        }
                        await dbContext.AddRangeAsync(bidClnt);
                        await dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        await CreateNotificationAsync("Bid # " + id + " <strong>New Party</strong> Added Successfully", 1);
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
        
        [HttpGet]
        public async Task<IActionResult> BidMngrEdit(string id)
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            CommBidMstr bidMstrModel;
            List<CommBidClntBidder> bidClntModel;
            using (var context = new AppDbContext())
            {
                bidMstrModel = await (from invMstr in dbContext.CommBidMstrs.Include(o => o.Prod).Include(w => w.Warehouse)
                                where invMstr.BidId == Int32.Parse(id)
                                where invMstr.CompId == Int32.Parse(CompanyId)
                                select invMstr).AsNoTracking().FirstOrDefaultAsync();
                if (bidMstrModel == null)
                {
                    return RedirectToAction("PageNotFound", "Dashboard");
                }
                bidClntModel = await (from invClnt in dbContext.CommBidClntBidders.Include(o => o.Party)
                                where invClnt.BidId == bidMstrModel.BidId
                                select invClnt).AsNoTracking().ToListAsync();
            }
            BidOrderDetailModel model = new BidOrderDetailModel
            {
                CommBidMstrs = bidMstrModel,
                CommBidClntBidders = bidClntModel
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetBidPartyListAjax(string id)
        {
            List<CommBidClntBidder> bidPartyList = await (from bidList in dbContext.CommBidClntBidders.Include(o => o.Party)
                                                   where bidList.BidId == Int32.Parse(id)
                                                   orderby bidList.Party.PartyName
                                                   select new CommBidClntBidder
                                                   {
                                                       SysId = bidList.SysId,
                                                       Party = new CommPartyInfo { PartyName = bidList.Party.PartyName },
                                                       SmsContNo = bidList.SmsContNo,
                                                       BidQnty = bidList.BidQnty,
                                                       BidRate = bidList.BidRate,
                                                       AllocQnty = bidList.AllocQnty,
                                                       AllocRate = bidList.AllocRate,
                                                       SmsSendStat = bidList.SmsSendStat,
                                                       BidAttnStat = bidList.BidAttnStat
                                                   }).AsNoTracking().ToListAsync();

            // Calculate the total result count
            int totalCount = bidPartyList.Count;

            // Create an anonymous object to include additional data
            var response = new
            {
                recordsTotal = totalCount,
                recordsFiltered = totalCount,
                data = bidPartyList
            };
            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> ActivateBid(string id, CommBidMstr model)
        {
            using (var context = new AppDbContext())
            {
                using (var transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        CommBidMstr bidPartyDet = await dbContext.CommBidMstrs.FirstOrDefaultAsync(s => s.BidId == Int32.Parse(id));
                        bidPartyDet.BidDate = model.BidDate;
                        bidPartyDet.BidStrtTm = model.BidStrtTm;
                        bidPartyDet.ProdId = model.ProdId;
                        bidPartyDet.WareId = model.WareId;
                        bidPartyDet.BidQnty = model.BidQnty;
                        bidPartyDet.BidRate = model.BidRate;
                        bidPartyDet.BidEndTm = model.BidEndTm;
                        bidPartyDet.BidNote = model.BidNote;
                        bidPartyDet.BidStat = 1;
                        bidPartyDet.ActivatedBy = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userName")?.Value;
                        bidPartyDet.ActivationTm = DateTime.Now;
                        dbContext.Update(bidPartyDet);
                        await dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        await CreateNotificationAsync("Bid # " + id + " Successfully <strong>Activated</strong>", 2);
                        return Json(new { success = true, message = "Status updated successfully." });
                    }
                    catch (DbUpdateException ex1)
                    {
                        await transaction.RollbackAsync();
                        var Message = ex1.Message;
                        if (ex1.InnerException != null)
                        {
                            Message += " : " + ex1.InnerException.Message;
                        }
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

        [HttpPost]
        public async Task<IActionResult> FinalizeCloseBid(string id, BidOrderDetailModel model)
        {
            ModelState.Clear();
            using (var context = new AppDbContext())
            {
                using (var transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        CommBidMstr bidPartyDet = await dbContext.CommBidMstrs.FirstOrDefaultAsync(s => s.BidId == Int32.Parse(id));
                        bidPartyDet.AllocTime = DateTime.Now;
                        bidPartyDet.AllocQnty = model.CommBidMstrs.AllocQnty;
                        bidPartyDet.AllocRate = model.CommBidMstrs.AllocRate;
                        bidPartyDet.BidStat = 2;
                        bidPartyDet.CloseBy = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userName")?.Value;
                        bidPartyDet.CloseTm = DateTime.Now;
                        dbContext.Update(bidPartyDet);
                        await dbContext.SaveChangesAsync();

                        List<CommBidClntBidder> bidClnt = new List<CommBidClntBidder>();
                        foreach (var clntBidList in model.CommBidClntBidders)
                        {
                            var existingItem = await dbContext.CommBidClntBidders
                                                .FirstOrDefaultAsync(b => b.SysId == clntBidList.SysId && b.BidAttnStat == 1);
                            if (existingItem != null)
                            {
                                // Update properties of the existing item
                                existingItem.AllocQnty = clntBidList.AllocQnty;
                                existingItem.AllocRate = clntBidList.AllocRate;
                                // Mark the entity as modified
                                dbContext.Entry(existingItem).State = EntityState.Modified;
                            }
                        }
                        await dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        await CreateNotificationAsync("Bid # " + id + " Successfully <strong>Closed</strong>", 4);
                        return Json(new { success = true, message = "Status updated successfully." });
                    }
                    catch (DbUpdateException ex1)
                    {
                        await transaction.RollbackAsync();
                        var Message = ex1.Message;
                        if (ex1.InnerException != null)
                        {
                            Message += " : " + ex1.InnerException.Message;
                        }
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

        [HttpPost]
        public async Task<IActionResult> UpdateSMSStatus(string id, string smsSendMsg, string returnStr)
        {
            using (var context = new AppDbContext())
            {
                using (var transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        CommBidClntBidder bidPartyDet = await dbContext.CommBidClntBidders.FirstOrDefaultAsync(s => s.SysId == Int32.Parse(id));
                        bidPartyDet.SmsSendStat = 1;
                        bidPartyDet.SmsSendTxt = smsSendMsg;
                        bidPartyDet.SmsRplyApi = returnStr?.Trim() ?? string.Empty;
                        dbContext.Update(bidPartyDet);
                        await dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return Json(new { success = true, message = "Status updated successfully." });
                    }
                    catch (DbUpdateException ex1)
                    {
                        await transaction.RollbackAsync();
                        var Message = ex1.Message;
                        if (ex1.InnerException != null)
                        {
                            Message += " : " + ex1.InnerException.Message;
                        }
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

        [HttpPost]
        public async Task<IActionResult> UpdateSMSAllocStatus(string id, string smsSendMsg, string returnStr)
        {
            using (var context = new AppDbContext())
            {
                using (var transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        CommBidClntBidder bidPartyDet = await dbContext.CommBidClntBidders.FirstOrDefaultAsync(s => s.SysId == Int32.Parse(id));
                        bidPartyDet.SmsAllocStat = 1;
                        dbContext.Update(bidPartyDet);
                        await dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return Json(new { success = true, message = "Status updated successfully." });
                    }
                    catch (DbUpdateException ex1)
                    {
                        await transaction.RollbackAsync();
                        var Message = ex1.Message;
                        if (ex1.InnerException != null)
                        {
                            Message += " : " + ex1.InnerException.Message;
                        }
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

        [HttpPost]
        public async Task<IActionResult> DeleteBidParty(string id)
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
                return Json(new { success = false, message = "Party information not found !!!" });
            }
            // Retrieve the Bid based on the Sys Id And Checking Whether The Bid Is In Draft Or Not
            // var category = await dbContext.CommBidClntBidders.FirstOrDefaultAsync(s => s.SysId == Int32.Parse(id));
            var category = await dbContext.CommBidClntBidders
                                .Include(b => b.Bid)
                                .Where(s => s.SysId == Int32.Parse(id) && s.Bid.BidStat == 0)
                                .FirstOrDefaultAsync();

            if (category == null)
            {
                // Handle the case when the category is not found
                return Json(new { success = false, message = "Party not found in Bid Table !!!" });
            }

            // Remove the category from the context
            dbContext.CommBidClntBidders.Remove(category);

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
            return Json(new { success = true, message = "Party deleted successfully from Bid !!!" });
        }

        [HttpGet]
        public async Task<IActionResult> BidFinalizer()
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            IEnumerable<CommBidMstr> bidMstrModel;
            bidMstrModel = await (from invMstr in dbContext.CommBidMstrs.Include(o => o.Prod).Include(w => w.Warehouse)
                            where invMstr.BidStat == 1
                            where invMstr.CompId == Int32.Parse(CompanyId)
                            select new CommBidMstr
                            {
                                BidId = invMstr.BidId,
                                BidDate = invMstr.BidDate,
                                BidStrtTm = invMstr.BidStrtTm,
                                BidEndTm = invMstr.BidEndTm,
                                Prod = new CommProdInfo { ProdName = invMstr.Prod.ProdName },
                                Warehouse = new CommWarehsInfo { WareName = invMstr.Warehouse.WareName },
                                BidQnty = invMstr.BidQnty,
                                BidRate = invMstr.BidRate,
                                AllocRate = invMstr.AllocRate,
                                BidNote = invMstr.BidNote,
                                BidStat = invMstr.BidStat,
                                BidSmsProcStat = invMstr.BidSmsProcStat,
                                NoOfPartyEng = dbContext.CommBidClntBidders
                                   .Where(bInfo => bInfo.BidId == invMstr.BidId)
                                   .Any() ? dbContext.CommBidClntBidders
                                               .Count(bInfo => bInfo.BidId == invMstr.BidId) : 0
                            }).AsNoTracking().ToListAsync();
            return View(bidMstrModel);
        }

        [HttpPost]
        public async Task<IActionResult> FinalizeBid(string id)
        {
            using (var context = new AppDbContext())
            {
                // Starting Setup
                var bidMstrList = await dbContext.CommBidMstrs.FirstOrDefaultAsync(bid => bid.BidId == Int32.Parse(id) && bid.BidStat == 1);
                if (bidMstrList == null)
                {
                    return RedirectToAction("PageNotFound", "Dashboard");
                }
                using (var transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        string smsContNo = "";
                        bool dataUpdate = false;
                        // Resetting to previous state
                        var bidClntList = await dbContext.CommBidClntBidders.Where(bid => bid.BidId == Int32.Parse(id)).ToListAsync();
                        foreach (var bidClnt in bidClntList)
                        {
                            dataUpdate = false;
                            smsContNo = '+' + bidClnt.SmsContNo.Trim();
                            // var smsLogs = dbContext.CommSmsLogs.Where(sms => sms.SmsPhone.Trim() == smsContNo && (sms.SmsDtTm >= bidMstrList.BidStrtTm && sms.SmsDtTm <= bidMstrList.BidEndTm)).OrderByDescending(log => log.SmsDtTm).FirstOrDefault();
                            var smsLogs = dbContext.CommSmsLogs.Where(sms => sms.SmsPhone.Trim() == smsContNo && (sms.SmsDtTm >= bidMstrList.BidStrtTm && sms.SmsDtTm <= bidMstrList.BidEndTm)).OrderByDescending(log => log.SmsDtTm).ToList();
                            foreach (var smsLog in smsLogs)
                            {
                                if (dataUpdate == true)
                                {
                                    break;
                                }
                                // Replacing any Multi Space or Line Break with Single Space
                                var smsMsgParts = Regex.Replace(smsLog.SmsMsg, @"\s+", " ").Split(' ');
                                if (smsMsgParts.Length >= 4)
                                {
                                    string bidIdString = smsMsgParts[0];
                                    string bidProdString = smsMsgParts[1];
                                    string bidQntyString = smsMsgParts[2];
                                    string bidRateString = smsMsgParts[3];
                                    if (string.Equals(id.Trim(), bidIdString.Trim()) == true) { 
                                        if (long.TryParse(bidIdString, out long bidId) && decimal.TryParse(bidQntyString, out decimal bidQnty) && decimal.TryParse(bidRateString, out decimal bidRate))
                                        {
                                            bidClnt.BidQnty = bidQnty;
                                            bidClnt.BidRate = bidRate;
                                            bidClnt.SmsRecRef = smsLog.AutoId;
                                            bidClnt.SmsRecTm = smsLog.SmsDtTm;
                                            bidClnt.SmsRawMsg = smsLog.SmsMsg.Trim();
                                            bidClnt.BidAttnStat = 1;
                                            dbContext.Entry(bidClnt).State = EntityState.Modified;
                                            dataUpdate = true;
                                        }
                                    }
                                }
                            }
                            if (dataUpdate == false)
                            { 
                                bidClnt.BidQnty = 0;
                                bidClnt.BidRate = 0;
                                bidClnt.BidAttnStat = 0;
                                dbContext.Entry(bidClnt).State = EntityState.Modified;
                            }
                        }
                        await dbContext.SaveChangesAsync();

                        if (bidMstrList.BidSmsProcStat != 1) // Checking Whether The Process Running First Time
                        {
                            bidMstrList.ProcessBy = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userName")?.Value;
                            bidMstrList.ProcessTm = DateTime.Now;
                        }
                        bidMstrList.BidSmsProcStat = 1;
                        dbContext.Entry(bidMstrList).State = EntityState.Modified;

                        await dbContext.SaveChangesAsync();

                        await transaction.CommitAsync();
                        await CreateNotificationAsync("Bid # " + id + " Successfully <strong>Processed</strong>", 3);
                        return Json(new { success = true, message = "SMS processed successfully." });
                    }
                    catch (DbUpdateException ex1)
                    {
                        await transaction.RollbackAsync();
                        var Message = ex1.Message;
                        if (ex1.InnerException != null)
                        {
                            Message += " : " + ex1.InnerException.Message;
                        }
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

        [HttpGet]
        public async Task<IActionResult> BidAllocate(string id)
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            CommBidMstr bidMstrModel;
            List<CommBidClntBidder> bidClntModel;
            using (var context = new AppDbContext())
            {
                bidMstrModel = await (from invMstr in dbContext.CommBidMstrs.Include(o => o.Prod).Include(w => w.Warehouse)
                                where invMstr.BidId == Int32.Parse(id) && invMstr.BidStat == 1 && invMstr.BidSmsProcStat == 1
                                where invMstr.CompId == Int32.Parse(CompanyId)
                                select invMstr).AsNoTracking().FirstOrDefaultAsync();
                if (bidMstrModel == null)
                {
                    return RedirectToAction("PageNotFound", "Dashboard");
                }
                bidClntModel = await (from invClnt in dbContext.CommBidClntBidders.Include(o => o.Party)
                                where invClnt.BidId == bidMstrModel.BidId
                                orderby invClnt.BidRate descending
                                select invClnt).AsNoTracking().ToListAsync();
            }
            BidOrderDetailModel model = new BidOrderDetailModel
            {
                CommBidMstrs = bidMstrModel,
                CommBidClntBidders = bidClntModel
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> BidAllocateSave(string id, BidOrderDetailModel model)
        {
            ModelState.Clear();
            using (var context = new AppDbContext())
            {
                using (var transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
                        string CompanyStartId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyStartId")?.Value.ToUpper();
                        CommBidMstr bidMstr = await dbContext.CommBidMstrs.FirstOrDefaultAsync(s => s.BidId == Int32.Parse(id));
                        bidMstr.AllocTime = DateTime.Now;
                        bidMstr.AllocQnty = model.CommBidMstrs.AllocQnty;
                        bidMstr.AllocRate = model.CommBidMstrs.AllocRate;
                        dbContext.Update(bidMstr);
                        await dbContext.SaveChangesAsync();

                        List<CommBidClntBidder> bidClnt = new List<CommBidClntBidder>();
                        foreach (var clntBidList in model.CommBidClntBidders)
                        {
                            var existingItem = await dbContext.CommBidClntBidders
                                                .FirstOrDefaultAsync(b => b.SysId == clntBidList.SysId && b.BidAttnStat == 1);
                            if (existingItem != null)
                            {
                                // Update properties of the existing item
                                existingItem.AllocQnty = clntBidList.AllocQnty;
                                existingItem.AllocRate = clntBidList.AllocRate;
                                // Mark the entity as modified
                                dbContext.Entry(existingItem).State = EntityState.Modified;
                            }
                        }
                        await dbContext.AddRangeAsync(bidClnt);
                        await dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        await CreateNotificationAsync("Bid # " + id + " Allocation Successfully <strong>Updated</strong>", 0);
                        return Json(new { result = true, message = "Allocation Updated Successfully" });
                    }
                    catch (DbUpdateException ex1)
                    {
                        await transaction.RollbackAsync();
                        var Message = ex1.Message;
                        if (ex1.InnerException != null)
                        {
                            Message += " : " + ex1.InnerException.Message;
                        }
                        ViewBag.ErrorMessage = "Exception: " + Message;
                        // throw ex1;
                        return Json(new { result = false, message = "Exception: " + Message });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ViewBag.ErrorMessage = "Exception: " + ex.Message;
                        // throw ex;
                        return Json(new { result = false, message = "Exception: " + ex.Message });
                    }
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> SmsList()
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            // Checking whether Bidding Ongoing ?
            DateTime currentTime = DateTime.Now;
            var bidMstrModel = await(from bidList in dbContext.CommBidMstrs
                                     where bidList.BidStrtTm < currentTime && bidList.BidEndTm > currentTime
                                     where bidList.CompId == Int32.Parse(CompanyId)
                                     select bidList).AsNoTracking().FirstOrDefaultAsync();
            if (bidMstrModel != null)
            {
                return RedirectToAction("BiddingX", "Dashboard");
            } else { 
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetSMSLog(DateTime startDate, DateTime endDate, [FromBody] DtParameters dtParameters)
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            endDate = endDate.Date.AddDays(1).AddSeconds(-1); // Set the time component to 23:59:59
            // Checking whether Bidding Ongoing ?
            DateTime currentTime = DateTime.Now;
            var bidMstrModel = await (from bidList in dbContext.CommBidMstrs
                                      where bidList.BidStrtTm <= currentTime && bidList.BidEndTm >= currentTime
                                      where bidList.CompId == Int32.Parse(CompanyId)
                                      select bidList).AsNoTracking().FirstOrDefaultAsync();
            if (bidMstrModel != null)
            {
                return RedirectToAction("BiddingX", "Dashboard");
            }
            // https://stackoverflow.com/questions/64292529/datatables-server-side-processing-in-asp-net-core-3-1
            var searchBy = dtParameters.Search?.Value;

            IQueryable<CommSmsLog> qrySMSList = (from smsList in dbContext.CommSmsLogs
                                                  where smsList.SmsDtTm >= startDate && smsList.SmsDtTm <= endDate
                                                  orderby smsList.SmsDtTm descending
                                                  select new CommSmsLog
                                                  {
                                                      AutoId = smsList.AutoId,
                                                      SmsDtTm = smsList.SmsDtTm,
                                                      SmsPhone = smsList.SmsPhone,
                                                      SmsMsg = smsList.SmsMsg
                                                  }).AsNoTracking();

            if (!string.IsNullOrEmpty(searchBy))
            {
                qrySMSList = qrySMSList.Where(r => r.SmsPhone != null && r.SmsPhone.Contains(searchBy.ToUpper()) ||
                                            r.SmsMsg != null && r.SmsMsg.ToUpper().Contains(searchBy.ToUpper()));
            }

            // now just get the count of items (without the skip and take) - eg how many could be returned with filtering
            var filteredResultsCount = await qrySMSList.CountAsync();
            var totalResultsCount = await dbContext.CommSmsLogs.CountAsync();

            // Apply sorting
            if (dtParameters.Order != null && dtParameters.Order.Length > 0)
            {
                DtOrder order = dtParameters.Order[0];
                string sortField = dtParameters.Columns[order.Column].Data;
                bool ascending = order.Dir == DtOrderDir.Asc;
                qrySMSList = ApplySortingSms(qrySMSList, sortField, ascending);
            }

            int start = dtParameters.Start >= 0 ? dtParameters.Start : 0;
            int length = dtParameters.Length > 0 ? dtParameters.Length : 10;

            return Json(new DtResult<CommSmsLog>
            {
                Draw = dtParameters.Draw,
                RecordsTotal = totalResultsCount,
                RecordsFiltered = filteredResultsCount,
                Data = await qrySMSList
                    .Skip(dtParameters.Start)
                    .Take(dtParameters.Length)
                    .ToListAsync()
            });
        }

        // Helper method to apply sorting based on column name and direction
        private IQueryable<CommSmsLog> ApplySortingSms(IQueryable<CommSmsLog> query, string sortField, bool ascending)
        {
            switch (sortField)
            {
                case "autoId":
                    return ascending ? query.OrderBy(g => g.AutoId) : query.OrderByDescending(g => g.AutoId);
                case "smsDtTm":
                    return ascending ? query.OrderBy(g => g.SmsDtTm) : query.OrderByDescending(g => g.SmsDtTm);
                case "smsPhone":
                    return ascending ? query.OrderBy(g => g.SmsPhone) : query.OrderByDescending(g => g.SmsPhone);
                case "smsMsg":
                    return ascending ? query.OrderBy(g => g.SmsMsg) : query.OrderByDescending(g => g.SmsMsg);
                // Add other sorting options for additional columns if needed
                default:
                    return query;
            }
        }

        [HttpGet]
        public async Task<IActionResult> BidPrint(string id)
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            CommBidMstr bidMstrModel;
            List<CommBidClntBidder> bidClntModel;
            using (var context = new AppDbContext())
            {
                bidMstrModel = await (from invMstr in dbContext.CommBidMstrs.Include(o => o.Prod).Include(w => w.Warehouse)
                                      where invMstr.BidId == Int32.Parse(id)
                                      where invMstr.CompId == Int32.Parse(CompanyId)
                                      select invMstr).AsNoTracking().FirstOrDefaultAsync();
                if (bidMstrModel == null)
                {
                    return RedirectToAction("PageNotFound", "Dashboard");
                }
                bidClntModel = await (from invClnt in dbContext.CommBidClntBidders.Include(o => o.Party)
                                      where invClnt.BidId == bidMstrModel.BidId
                                      orderby invClnt.BidRate descending
                                      select invClnt).AsNoTracking().ToListAsync();
            }
            BidOrderDetailModel model = new BidOrderDetailModel
            {
                CommBidMstrs = bidMstrModel,
                CommBidClntBidders = bidClntModel
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetBidListExcelAjax(DateTime startDate, DateTime endDate)
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            endDate = endDate.Date.AddDays(1).AddSeconds(-1); // Set the time component to 23:59:59

            IQueryable<CommBidMstr> qryBidList = (from bidList in dbContext.CommBidMstrs.Include(o => o.Prod)
                                                  where bidList.CompId == Int32.Parse(CompanyId)
                                                  where bidList.BidDate >= startDate && bidList.BidDate <= endDate
                                                  orderby bidList.BidId descending
                                                  select new CommBidMstr
                                                  {
                                                      BidId = bidList.BidId,
                                                      BidDate = bidList.BidDate,
                                                      BidStrtTm = bidList.BidStrtTm,
                                                      BidEndTm = bidList.BidEndTm,
                                                      Prod = new CommProdInfo { ProdName = bidList.Prod.ProdName },
                                                      BidQnty = bidList.BidQnty,
                                                      BidRate = bidList.BidRate,
                                                      AllocRate = bidList.AllocRate,
                                                      BidNote = bidList.BidNote,
                                                      BidStat = bidList.BidStat,
                                                      NoOfPartyEng = dbContext.CommBidClntBidders
                                                                 .Where(bInfo => bInfo.BidId == bidList.BidId)
                                                                 .Any() ? dbContext.CommBidClntBidders
                                                                             .Count(bInfo => bInfo.BidId == bidList.BidId) : 0
                                                  }).AsNoTracking();



            // Create a new Excel package
            using (var package = new ExcelPackage())
            {
                // Add data to the Excel worksheet
                var worksheet = package.Workbook.Worksheets.Add("Bid Data");

                // TODO: Populate the worksheet with the data from your database
                // Set column headers
                worksheet.Cells[1, 1].Value = "Bid ID";
                worksheet.Cells[1, 2].Value = "Bid Date";
                worksheet.Cells[1, 3].Value = "Bid Start Time";
                worksheet.Cells[1, 4].Value = "Bid End Time";
                worksheet.Cells[1, 5].Value = "Product Name";
                worksheet.Cells[1, 6].Value = "Bid Quantity";
                worksheet.Cells[1, 7].Value = "Bid Rate";
                worksheet.Cells[1, 8].Value = "Allocation Rate";
                worksheet.Cells[1, 9].Value = "Bid Note";
                worksheet.Cells[1, 10].Value = "Bid Status";
                worksheet.Cells[1, 11].Value = "Number of Engaged Parties";

                int rowIndex = 2;
                foreach (var bidItem in qryBidList)
                {
                    worksheet.Cells[rowIndex, 1].Value = bidItem.BidId;
                    worksheet.Cells[rowIndex, 2].Value = bidItem.BidDate.ToString("dd-MMM-yyyy");
                    worksheet.Cells[rowIndex, 3].Value = bidItem.BidStrtTm.ToString("hh:mm tt");
                    worksheet.Cells[rowIndex, 4].Value = bidItem.BidEndTm.ToString("hh:mm tt");
                    worksheet.Cells[rowIndex, 5].Value = bidItem.Prod.ProdName;
                    worksheet.Cells[rowIndex, 6].Value = bidItem.BidQnty;
                    worksheet.Cells[rowIndex, 7].Value = bidItem.BidRate;
                    worksheet.Cells[rowIndex, 8].Value = bidItem.AllocRate;
                    worksheet.Cells[rowIndex, 9].Value = bidItem.BidNote;
                    worksheet.Cells[rowIndex, 10].Value = bidItem.BidStat;
                    worksheet.Cells[rowIndex, 11].Value = bidItem.NoOfPartyEng;

                    rowIndex++;
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Save the Excel package to a memory stream
                MemoryStream stream = new MemoryStream();
                await package.SaveAsAsync(stream);

                // Return the Excel file to the client
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BidsList.xlsx");
            }
        }

        [HttpGet]
        public IActionResult NotifyList()
        {
            return View();
        }

        // Action method to return List based on date range
        [HttpGet]
        public async Task<IActionResult> GetNotifyListAjax(DateTime startDate, DateTime endDate)
        {
            string CompanyId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "companyId")?.Value;
            endDate = endDate.Date.AddDays(1).AddSeconds(-1); // Set the time component to 23:59:59
            List<CommNotifyList> notifyLists = await (from notifyList in dbContext.CommNotifyLists.Include(o => o.NotByNavigation)
                                                    where notifyList.CreatTm >= startDate && notifyList.CreatTm <= endDate
                                                    select new CommNotifyList
                                                    {
                                                        NotId = notifyList.NotId,
                                                        NotMsg = notifyList.NotMsg,
                                                        NotByNavigation = new CommLoginInfo { UserNm = notifyList.NotByNavigation.UserNm },
                                                        CreatTm = notifyList.CreatTm
                                                  }).AsNoTracking().ToListAsync();
            // Calculate the total result count
            int totalCount = notifyLists.Count;

            // Create an anonymous object to include additional data
            var response = new
            {
                recordsTotal = totalCount,
                recordsFiltered = totalCount,
                data = notifyLists
            };

            return Json(response);
        }

        [HttpGet]
        public IActionResult SmsSendLog()
        {
            return View();
        }

        public async Task CreateNotificationAsync(string message, int NotType)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId")?.Value;
            CommNotifyList notification = new CommNotifyList
            {
                NotBy = Int32.Parse(userId),
                NotMsg = message,
                NotType = NotType,
                IsRead = 0
            };

            dbContext.CommNotifyLists.Add(notification);
            await dbContext.SaveChangesAsync();
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var unreadNotifications = await dbContext.CommNotifyLists
                .Where(n => n.IsRead == 0)
                .OrderByDescending(n => n.CreatTm)
                .Take(5)
                .ToListAsync();

            return Json(unreadNotifications);
        }

    }
}
