using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Web.Framework.Models;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Web.Extensions
{
    public static class ModelExtensions
    {
        public static IPagedList<T> ToPagedList<T>(this IList<T> list, IPagingRequestModel pagingRequestModel)
        {
            return new PagedList<T>(list, pagingRequestModel.Page - 1, pagingRequestModel.PageSize);
        }

        public static TListModel PrepareToGrid<TListModel, TModel, TObject>(this TListModel listModel,
            BaseSearchModel searchModel, IPagedList<TObject> objectList, Func<IEnumerable<TModel>> dataFillFunction) 
            where TListModel : BasePagedListModel<TModel>
            where TModel : BaseInovatiqaModel
        {
            if (listModel == null)
                throw new ArgumentNullException(nameof(listModel));

            listModel.Data = dataFillFunction?.Invoke();
            listModel.Draw = searchModel?.Draw;
            listModel.RecordsTotal = objectList?.TotalCount ?? 0;
            listModel.RecordsFiltered = objectList?.TotalCount ?? 0;

            return listModel;
        }
    }
}