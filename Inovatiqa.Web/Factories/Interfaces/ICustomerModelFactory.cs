using Inovatiqa.Database.Models;
using Inovatiqa.Web.Models.Customer;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Inovatiqa.Web.Factories.Interfaces
{
    public partial interface ICustomerModelFactory
    {
        RegisterModel PrepareRegisterModel(RegisterModel model, bool excludeProperties, 
            string overrideCustomCustomerAttributesXml = "", bool setDefaultValues = false);

        IList<CustomerAttributeModel> PrepareCustomCustomerAttributes(Customer customer, string overrideAttributesXml = "");

        LoginModel PrepareLoginModel(bool? checkoutAsGuest);

        RegisterResultModel PrepareRegisterResultModel(int resultId);

        CustomerInfoModel PrepareCustomerInfoModel(CustomerInfoModel model, Customer customer,
            bool excludeProperties, string overrideCustomCustomerAttributesXml = "");

        CustomerAddressListModel PrepareCustomerAddressListModel();

        PasswordRecoveryModel PreparePasswordRecoveryModel(PasswordRecoveryModel model);

        PasswordRecoveryConfirmModel PreparePasswordRecoveryConfirmModel();

        ChangePasswordModel PrepareChangePasswordModel();
		Customer PrepareChildCustomerModel(IFormCollection collection);
        Address PrepareChildAddressModel(IFormCollection collection);
        ChildAccountModel GetAllChildAccounts();
		ChildAccountModel PrepareChildDetailsModel(Customer model);
        CustomerAccountInfoUpdateResultModel UpdateCustomerAccountInformationResultModel(CustomerAccountInfoUpdateResultModel model, IFormCollection form);
    }
}
