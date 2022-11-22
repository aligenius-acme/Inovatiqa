using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Inovatiqa.Services.Media.Interfaces
{
    public partial interface IPictureService
    {
        IList<Picture> GetPicturesByProductId(int productId, int recordsToReturn = 0);

        string GetPictureUrl(ref Picture picture,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity);

        string GetDefaultPictureUrl(int targetSize = 0,
            PictureType defaultPictureType = PictureType.Entity,
            string storeLocation = null);

        string GetThumbLocalPath(Picture picture, int targetSize = 0, bool showDefaultPicture = true);

        string GetFileExtensionFromMimeType(string mimeType);

        byte[] LoadPictureBinary(Picture picture);

        PictureBinary GetPictureBinaryByPictureId(int pictureId);

        Picture GetProductPicture(Product product, string attributesXml);

        Picture GetPictureById(int pictureId);

        string GetPictureUrl(int pictureId,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity);

        void DeletePicture(Picture picture);

        Picture UpdatePicture(int pictureId, byte[] pictureBinary, string mimeType,
            string seoFilename, string altAttribute = null, string titleAttribute = null,
            bool isNew = true, bool validateBinary = true);

        byte[] ValidatePicture(byte[] pictureBinary, string mimeType);

        Picture InsertPicture(IFormFile formFile, string defaultFileName = "", string virtualPath = "");

        Picture InsertPicture(byte[] pictureBinary, string mimeType, string seoFilename,
            string altAttribute = null, string titleAttribute = null,
            bool isNew = true, bool validateBinary = true);

        Picture UpdatePicture(Picture picture);
    }
}