
namespace Inovatiqa.Web.Framework.Models
{
    public partial interface IPagingRequestModel
    {
        int Page { get; }

        int PageSize { get; }
    }
}