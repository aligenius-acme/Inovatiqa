using System;
using Inovatiqa.Core;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Logging.Interfaces;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Exceptions;
using MaxMind.GeoIP2.Responses;

namespace Inovatiqa.Services.Directory
{
    public partial class GeoLookupService : IGeoLookupService
    {
        #region Fields

        private readonly ILoggerService _loggerService;
        private readonly IInovatiqaFileProvider _fileProvider;

        #endregion

        #region Ctor

        public GeoLookupService(ILoggerService loggerService,
            IInovatiqaFileProvider fileProvider)
        {
            _loggerService = loggerService;
            _fileProvider = fileProvider;
        }

        #endregion

        #region Utilities

        protected virtual CountryResponse GetInformation(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return null;

            try
            {
                var databasePath = _fileProvider.MapPath("~/App_Data/GeoLite2-Country.mmdb");
                var reader = new DatabaseReader(databasePath);
                var omni = reader.Country(ipAddress);
                return omni;
            }
            catch (GeoIP2Exception)
            {
                return null;
            }
            catch (Exception exc)
            {
                _loggerService.Warning("Cannot load MaxMind record", exc);
                return null;
            }
        }

        #endregion

        #region Methods

        public virtual string LookupCountryIsoCode(string ipAddress)
        {
            var response = GetInformation(ipAddress);
            if (response?.Country != null)
                return response.Country.IsoCode;

            return string.Empty;
        }

        public virtual string LookupCountryName(string ipAddress)
        {
            var response = GetInformation(ipAddress);
            if (response?.Country != null)
                return response.Country.Name;

            return string.Empty;
        }

        #endregion
    }
}