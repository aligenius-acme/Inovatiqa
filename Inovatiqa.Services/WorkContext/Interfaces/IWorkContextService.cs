using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.WorkContext.Interfaces
{
    public interface IWorkContextService
    {
        Customer CurrentCustomer { get; set; }

        Customer OriginalCustomerIfImpersonated { get; }

        Vendor CurrentVendor { get; }
    }
}
