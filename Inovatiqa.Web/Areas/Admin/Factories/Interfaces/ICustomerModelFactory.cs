using Inovatiqa.Database.Models;
using Inovatiqa.Web.Areas.Admin.Models.Customers;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial interface ICustomerModelFactory
    {
        CustomerSearchModel PrepareCustomerSearchModel(CustomerSearchModel searchModel);

        CustomerListModel PrepareCustomerListModel(CustomerSearchModel searchModel);

        CustomerModel PrepareCustomerModel(CustomerModel model, Customer customer, bool excludeProperties = false);

        CustomerAddressListModel PrepareCustomerAddressListModel(CustomerAddressSearchModel searchModel, Customer customer);

        CustomerAddressModel PrepareCustomerAddressModel(CustomerAddressModel model,
            Customer customer, Address address, bool excludeProperties = false);

        CustomerOrderListModel PrepareCustomerOrderListModel(CustomerOrderSearchModel searchModel, Customer customer);

        CustomerShoppingCartListModel PrepareCustomerShoppingCartListModel(CustomerShoppingCartSearchModel searchModel,
            Customer customer);

        CustomerActivityLogListModel PrepareCustomerActivityLogListModel(CustomerActivityLogSearchModel searchModel, Customer customer);

        OnlineCustomerSearchModel PrepareOnlineCustomerSearchModel(OnlineCustomerSearchModel searchModel);

        OnlineCustomerListModel PrepareOnlineCustomerListModel(OnlineCustomerSearchModel searchModel);
    }
}