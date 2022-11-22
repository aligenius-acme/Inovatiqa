using Inovatiqa.Database.Models;
using Inovatiqa.Web.Areas.Admin.Models.Customers;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial interface ICustomerRoleModelFactory
    {
        CustomerRoleSearchModel PrepareCustomerRoleSearchModel(CustomerRoleSearchModel searchModel);

        CustomerRoleListModel PrepareCustomerRoleListModel(CustomerRoleSearchModel searchModel);

        CustomerRoleModel PrepareCustomerRoleModel(CustomerRoleModel model, CustomerRole customerRole, bool excludeProperties = false);

        CustomerRoleProductSearchModel PrepareCustomerRoleProductSearchModel(CustomerRoleProductSearchModel searchModel);

        CustomerRoleProductListModel PrepareCustomerRoleProductListModel(CustomerRoleProductSearchModel searchModel);
    }
}