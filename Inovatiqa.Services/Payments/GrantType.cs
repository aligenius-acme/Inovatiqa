using System.Runtime.Serialization;

namespace Inovatiqa.Services.Payments
{
    public enum GrantType
    {
        [EnumMember(Value = "authorization_code")]
        New,

        [EnumMember(Value = "refresh_token")]
        Refresh,

        [EnumMember(Value = "migration_token")]
        Migration
    }
}