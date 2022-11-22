using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Security.Interfaces
{
    public interface IPermissionProvider
    {
        IEnumerable<PermissionRecord> GetPermissions();

        HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions();
    }
}
