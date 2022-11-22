using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Common.Interfaces;
using System.Linq;

namespace Inovatiqa.Services.Common
{
    public partial class SearchTermService : ISearchTermService
    {
        #region Fields

        private readonly IRepository<SearchTerm> _searchTermRepository;

        #endregion

        #region Ctor

        public SearchTermService(IRepository<SearchTerm> searchTermRepository)
        {
            _searchTermRepository = searchTermRepository;
        }

        #endregion

        #region Methods

        public virtual IPagedList<SearchTermReportLine> GetStats(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = (from st in _searchTermRepository.Query()
                         group st by st.Keyword into groupedResult
                         select new
                         {
                             Keyword = groupedResult.Key,
                             Count = groupedResult.Sum(o => o.Count)
                         })
                        .OrderByDescending(m => m.Count)
                        .Select(r => new SearchTermReportLine
                        {
                            Keyword = r.Keyword,
                            Count = r.Count
                        });

            var result = new PagedList<SearchTermReportLine>(query, pageIndex, pageSize);
            return result;
        }

        #endregion
    }
}