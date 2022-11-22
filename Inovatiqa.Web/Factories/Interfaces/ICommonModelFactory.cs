using Inovatiqa.Web.Models.Common;

namespace Inovatiqa.Web.Factories.Interfaces
{
    public partial interface ICommonModelFactory
    {
        HeaderLinksModel PrepareHeaderLinksModel();

        AdminHeaderLinksModel PrepareAdminHeaderLinksModel();

        ContactUsModel PrepareContactUsModel(ContactUsModel model, bool excludeProperties);

    }
}
