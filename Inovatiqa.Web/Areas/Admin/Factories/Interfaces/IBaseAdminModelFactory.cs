using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial interface IBaseAdminModelFactory
    {
        void PrepareWarehouses(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareProductTemplates(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareDeliveryDates(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareProductAvailabilityRanges(IList<SelectListItem> items,
            bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareVendors(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PreparePaymentModes(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PreparePaymentTerms(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareCategories(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareManufacturers(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareOrderStatuses(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PreparePaymentStatuses(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareShippingStatuses(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareStores(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareProductTypes(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PreparePaymentMethods(IList<SelectListItem> items);

        void PrepareCountries(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareStatesAndProvinces(IList<SelectListItem> items, int? countryId,
            bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareCategoryTemplates(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareManufacturerTemplates(IList<SelectListItem> items,
            bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareReturnRequestStatuses(IList<SelectListItem> items,
            bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareShoppingCartTypes(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareTimeZones(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareActivityLogTypes(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);

        void PrepareCustomerRoles(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);
    }
}