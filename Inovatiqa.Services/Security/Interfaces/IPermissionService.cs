using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Security.Interfaces
{
    public partial interface IPermissionService
    {
        bool Authorize(PermissionRecord permission);
        bool Authorize(PermissionRecord permission, Customer customer);

        bool Authorize(string permissionRecordSystemName);

        bool Authorize(string permissionRecordSystemName, Customer customer);

        bool Authorize(string permissionRecordSystemName, int customerRoleId);
    }
}