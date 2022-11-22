using System;
using Inovatiqa.Database.Models;
using Microsoft.AspNetCore.Http;

namespace Inovatiqa.Services.Media.Interfaces
{
    public partial interface IDownloadService
    {
        Download GetDownloadById(int downloadId);

        Download GetDownloadByGuid(Guid downloadGuid);

        void DeleteDownload(Download download);

        void InsertDownload(Download download);

        void UpdateDownload(Download download);

        byte[] GetDownloadBits(IFormFile file);
    }
}