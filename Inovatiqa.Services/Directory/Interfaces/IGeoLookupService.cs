namespace Inovatiqa.Services.Directory.Interfaces
{
    public partial interface IGeoLookupService
    {
        string LookupCountryIsoCode(string ipAddress);

        string LookupCountryName(string ipAddress);
    }
}