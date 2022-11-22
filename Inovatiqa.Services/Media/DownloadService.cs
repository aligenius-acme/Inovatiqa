using System;
using System.IO;
using System.Linq;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Events.Interfaces;
using Inovatiqa.Services.Media.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Inovatiqa.Services.Media
{
    public partial class DownloadService : IDownloadService
    {
        #region Fields

        private readonly IEventPublisher _eventPubisher;
        private readonly IRepository<Download> _downloadRepository;
        #endregion

        #region Ctor

        public DownloadService(IEventPublisher eventPubisher,
            IRepository<Download> downloadRepository)
        {
            _eventPubisher = eventPubisher;
            _downloadRepository = downloadRepository;
        }

        #endregion

        #region Methods

        public virtual Download GetDownloadById(int downloadId)
        {
            if (downloadId == 0)
                return null;

            return _downloadRepository.GetById(downloadId);
        }

        public virtual Download GetDownloadByGuid(Guid downloadGuid)
        {
            if (downloadGuid == Guid.Empty)
                return null;

            var query = from o in _downloadRepository.Query()
                        where o.DownloadGuid == downloadGuid
                        select o;

            return query.FirstOrDefault();
        }

        public virtual void DeleteDownload(Download download)
        {
            if (download == null)
                throw new ArgumentNullException(nameof(download));

            _downloadRepository.Delete(download);

            //_eventPubisher.EntityDeleted(download);
        }

        public virtual void InsertDownload(Download download)
        {
            if (download == null)
                throw new ArgumentNullException(nameof(download));

            _downloadRepository.Insert(download);

            //_eventPubisher.EntityInserted(download);
        }

        public virtual void UpdateDownload(Download download)
        {
            if (download == null)
                throw new ArgumentNullException(nameof(download));

            _downloadRepository.Update(download);

            //_eventPubisher.EntityUpdated(download);
        }

        public virtual byte[] GetDownloadBits(IFormFile file)
        {
            using var fileStream = file.OpenReadStream();
            using var ms = new MemoryStream();
            fileStream.CopyTo(ms);
            var fileBytes = ms.ToArray();
            return fileBytes;
        }

        #endregion
    }
}