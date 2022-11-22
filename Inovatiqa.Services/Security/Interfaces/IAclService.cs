using Inovatiqa.Core;

namespace Inovatiqa.Services.Security
{
    public partial interface IAclService
    {
        int[] GetCustomerRoleIdsWithAccess<T>(T entity, int id) where T : IAclSupported;
    }
}