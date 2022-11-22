using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;

namespace Inovatiqa.Services.Common.Interfaces
{
    public partial interface ISearchTermService
    {
        IPagedList<SearchTermReportLine> GetStats(int pageIndex = 0, int pageSize = int.MaxValue);
    }
}