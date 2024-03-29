using orgBidAplctn.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace orgBidAplctn.Models.DataViewModel
{
    public class BidOrderDetailModel
    {
        public CommBidMstr CommBidMstrs { get; set; }
        public ICollection<CommBidClntBidder> CommBidClntBidders { get; set; }
    }

    public class ViewModalCalendarEvent
    {
        public long EventId { get; set; }
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public byte Status { get; set; }
    }

    public class ViewDashboardEvent
    {
        public long ActiveBid { get; set; }
        public long DraftBid { get; set; }
        public long TotalBid { get; set; }
    }

}
