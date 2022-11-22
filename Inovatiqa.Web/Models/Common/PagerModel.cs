namespace Inovatiqa.Web.Models.Common
{
    public partial class PagerModel
    {
        #region Ctor

        #endregion Constructors

        #region Fields

        private int individualPagesDisplayedCount;
        private int pageIndex = -2;
        private int pageSize;

        private bool? showFirst;
        private bool? showIndividualPages;
        private bool? showLast;
        private bool? showNext;
        private bool? showPagerItems;
        private bool? showPrevious;
        private bool? showTotalSummary;

        private string firstButtonText;
        private string lastButtonText;
        private string nextButtonText;
        private string previousButtonText;
        private string currentPageText;

        #endregion Fields

        #region Properties

        public int CurrentPage => PageIndex + 1;

        public int IndividualPagesDisplayedCount
        {
            get
            {
                if (individualPagesDisplayedCount <= 0)
                    return 5;

                return individualPagesDisplayedCount;
            }
            set => individualPagesDisplayedCount = value;
        }

        public int PageIndex
        {
            get
            {
                if (pageIndex < 0)
                {
                    return 0;
                }
                return pageIndex;
            }
            set => pageIndex = value;
        }

        public int PageSize
        {
            get => (pageSize <= 0) ? 10 : pageSize;
            set => pageSize = value;
        }

        public bool ShowFirst
        {
            get => showFirst ?? true;
            set => showFirst = value;
        }

        public bool ShowIndividualPages
        {
            get => showIndividualPages ?? true;
            set => showIndividualPages = value;
        }

        public bool ShowLast
        {
            get => showLast ?? true;
            set => showLast = value;
        }

        public bool ShowNext
        {
            get => showNext ?? true;
            set => showNext = value;
        }

        public bool ShowPagerItems
        {
            get => showPagerItems ?? true;
            set => showPagerItems = value;
        }

        public bool ShowPrevious
        {
            get => showPrevious ?? true;
            set => showPrevious = value;
        }

        public bool ShowTotalSummary
        {
            get => showTotalSummary ?? false;
            set => showTotalSummary = value;
        }

        public int TotalPages
        {
            get
            {
                if ((TotalRecords == 0) || (PageSize == 0))
                {
                    return 0;
                }
                var num = TotalRecords / PageSize;
                if ((TotalRecords % PageSize) > 0)
                {
                    num++;
                }
                return num;
            }
        }

        public int TotalRecords { get; set; }

        public string FirstButtonText
        {
            get => (!string.IsNullOrEmpty(firstButtonText)) ?
                    firstButtonText :
                    "First";
            set => firstButtonText = value;
        }

        public string LastButtonText
        {
            get => (!string.IsNullOrEmpty(lastButtonText)) ?
                    lastButtonText :
                    "Last";
            set => lastButtonText = value;
        }

        public string NextButtonText
        {
            get => (!string.IsNullOrEmpty(nextButtonText)) ?
                    nextButtonText :
                    "Next";
            set => nextButtonText = value;
        }

        public string PreviousButtonText
        {
            get => (!string.IsNullOrEmpty(previousButtonText)) ?
                    previousButtonText :
                    "Previous";
            set => previousButtonText = value;
        }

        public string CurrentPageText
        {
            get => (!string.IsNullOrEmpty(currentPageText)) ?
                    currentPageText :
                    "Page {0} of {1} ({2} total)";
            set => currentPageText = value;
        }

        public string RouteActionName { get; set; }

        public bool UseRouteLinks { get; set; }

        public IRouteValues RouteValues { get; set; }

        #endregion Properties

        #region Methods

        public int GetFirstIndividualPageIndex()
        {
            if ((TotalPages < IndividualPagesDisplayedCount) ||
                ((PageIndex - (IndividualPagesDisplayedCount / 2)) < 0))
            {
                return 0;
            }
            if ((PageIndex + (IndividualPagesDisplayedCount / 2)) >= TotalPages)
            {
                return (TotalPages - IndividualPagesDisplayedCount);
            }
            return (PageIndex - (IndividualPagesDisplayedCount / 2));
        }

        public int GetLastIndividualPageIndex()
        {
            var num = IndividualPagesDisplayedCount / 2;
            if ((IndividualPagesDisplayedCount % 2) == 0)
            {
                num--;
            }
            if ((TotalPages < IndividualPagesDisplayedCount) ||
                ((PageIndex + num) >= TotalPages))
            {
                return (TotalPages - 1);
            }
            if ((PageIndex - (IndividualPagesDisplayedCount / 2)) < 0)
            {
                return (IndividualPagesDisplayedCount - 1);
            }
            return (PageIndex + num);
        }

        #endregion Methods
    }

    #region Classes

    public interface IRouteValues
    {
        int pageNumber { get; set; }
    }

    public partial class RouteValues : IRouteValues
    {
        public int id { get; set; }
        public string slug { get; set; }
        public int pageNumber { get; set; }
    }

    public partial class ForumSearchRouteValues : IRouteValues
    {
        public string searchterms { get; set; }
        public string adv { get; set; }
        public string forumId { get; set; }
        public string within { get; set; }
        public string limitDays { get; set; }
        public int pageNumber { get; set; }
    }

    public partial class PrivateMessageRouteValues : IRouteValues
    {
        public string tab { get; set; }
        public int pageNumber { get; set; }
    }

    public partial class ForumActiveDiscussionsRouteValues : IRouteValues
    {
        public int pageNumber { get; set; }
    }

    public partial class ForumSubscriptionsRouteValues : IRouteValues
    {        
        public int pageNumber { get; set; }
    }

    public partial class BackInStockSubscriptionsRouteValues : IRouteValues
    {
        public int pageNumber { get; set; }
    }

    public partial class RewardPointsRouteValues : IRouteValues
    {
        public int pageNumber { get; set; }
    }

    #endregion Classes
}