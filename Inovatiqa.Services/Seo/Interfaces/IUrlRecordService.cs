using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Seo.Interfaces
{
    public partial interface IUrlRecordService
    {
        string GetActiveSlug(int entityId, string entityName, int languageId);

        UrlRecord GetBySlug(string slug);

        IPagedList<UrlRecord> GetAllUrlRecords(string slug = "", int? languageId = null, bool? isActive = null, int pageIndex = 0, int pageSize = int.MaxValue);

        void SaveManufacturerSlug(Manufacturer entity, string slug, int languageId);

        void SaveVendorSlug(Vendor entity, string slug, int languageId);

        void UpdateUrlRecord(UrlRecord urlRecord);

        void InsertUrlRecord(UrlRecord urlRecord);
    }
}