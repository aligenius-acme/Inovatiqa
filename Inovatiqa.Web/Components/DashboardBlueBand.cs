using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Media.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Inovatiqa.Web.Models.Catalog;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;

namespace Inovatiqa.Web.Components
{
   public class DashboardBlueBandViewComponent : ViewComponent
    {
        private readonly IWorkContextService _workContextService;
        private readonly IAddressService _addressService;

        private readonly IStateProvinceService _stateProvinceService;
        public DashboardBlueBandViewComponent(
            IWorkContextService workContextService,
            IAddressService addressService,
            IStateProvinceService stateProvinceService)
        {
            _workContextService = workContextService;
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
        }

        public IViewComponentResult Invoke()
        {
            var customer = _workContextService.CurrentCustomer;
            var BillIngAddress = _addressService.GetAddressById(Convert.ToInt32(customer.BillingAddressId));
            var ShippingAddress = _addressService.GetAddressById(Convert.ToInt32(customer.ShippingAddressId));
            var model = new Database.Models.Customer();
            model.BillingAddress = BillIngAddress;
            model.ShippingAddress = ShippingAddress;
            model.Id = customer != null ? customer.Id : 0;
            ViewBag.BillingAddressState = _stateProvinceService.GetStateProvinceById(Convert.ToInt32(model.BillingAddress?.StateProvinceId));
            ViewBag.ShippingAddressState = _stateProvinceService.GetStateProvinceById(Convert.ToInt32(model.ShippingAddress?.StateProvinceId));
            return View(model);
            
        }

    }
}
