using Inovatiqa.Core;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Media.Interfaces;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static SixLabors.ImageSharp.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Inovatiqa.Services.Catalog.Interfaces;

namespace Inovatiqa.Services.Media
{
    public partial class PictureService : IPictureService
    {
        #region Fields

        private readonly IRepository<Picture> _pictureRepository;
        private readonly IRepository<ProductPictureMapping> _productPictureRepository;
        private readonly IInovatiqaFileProvider _fileProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<PictureBinary> _pictureBinaryRepository;
        private readonly IProductAttributeParserService _productAttributeParserService;
        private readonly IDownloadService _downloadService;

        #endregion

        #region Ctor

        public PictureService(IRepository<Picture> pictureRepository,
            IRepository<ProductPictureMapping> productPictureRepository,
            IInovatiqaFileProvider fileProvider,
            IHttpContextAccessor httpContextAccessor,
            IRepository<PictureBinary> pictureBinaryRepository,
            IProductAttributeParserService productAttributeParserService,
            IDownloadService downloadService)
        {
            _pictureRepository = pictureRepository;
            _productPictureRepository = productPictureRepository;
            _fileProvider = fileProvider;
            _httpContextAccessor = httpContextAccessor;
            _pictureBinaryRepository = pictureBinaryRepository;
            _productAttributeParserService = productAttributeParserService;
            _downloadService = downloadService;
        }

        #endregion

        #region Utilities

        protected virtual PictureBinary UpdatePictureBinary(Picture picture, byte[] binaryData)
        {
            if (picture == null)
                throw new ArgumentNullException(nameof(picture));

            var pictureBinary = GetPictureBinaryByPictureId(picture.Id);

            var isNew = pictureBinary == null;

            if (isNew)
                pictureBinary = new PictureBinary
                {
                    PictureId = picture.Id
                };

            pictureBinary.BinaryData = binaryData;

            if (isNew)
            {
                _pictureBinaryRepository.Insert(pictureBinary);

                //event notification
                //_eventPublisher.EntityInserted(pictureBinary);
            }
            else
            {
                _pictureBinaryRepository.Update(pictureBinary);

                //event notification
                //_eventPublisher.EntityUpdated(pictureBinary);
            }

            return pictureBinary;
        }

        protected virtual void DeletePictureThumbs(Picture picture)
        {
            var filter = $"{picture.Id:0000000}*.*";
            var currentFiles = _fileProvider.GetFiles(_fileProvider.GetAbsolutePath(InovatiqaDefaults.ImageThumbsPath), filter, false);
            foreach (var currentFileName in currentFiles)
            {
                var thumbFilePath = GetThumbLocalPath(currentFileName);
                _fileProvider.DeleteFile(thumbFilePath);
            }
        }

        protected virtual void DeletePictureOnFileSystem(Picture picture)
        {
            if (picture == null)
                throw new ArgumentNullException(nameof(picture));

            var lastPart = GetFileExtensionFromMimeType(picture.MimeType);
            var fileName = $"{picture.Id:0000000}_0.{lastPart}";
            var filePath = GetPictureLocalPath(fileName);
            _fileProvider.DeleteFile(filePath);
        }

        protected virtual string GetPictureLocalPath(string fileName)
        {
            return _fileProvider.GetAbsolutePath("images", fileName);
        }

        protected virtual string GetImagesPathUrl(string storeLocation = null)
        {
            var imagesPathUrl = string.Empty;

            imagesPathUrl = "/images/";

            return imagesPathUrl;
        }

        protected virtual string GetThumbLocalPath(string thumbFileName)
        {
            var thumbsDirectoryPath = _fileProvider.GetAbsolutePath(InovatiqaDefaults.ImageThumbsPath);

            var thumbFilePath = _fileProvider.Combine(thumbsDirectoryPath, thumbFileName);
            return thumbFilePath;
        }

        protected virtual bool GeneratedThumbExists(string thumbFilePath, string thumbFileName)
        {
            return _fileProvider.FileExists(thumbFilePath);
        }

        protected virtual string GetThumbUrl(string thumbFileName, string storeLocation = null)
        {
            var url = GetImagesPathUrl(storeLocation) + "thumbs/";

            url = url + thumbFileName;
            return url;
        }

        protected virtual byte[] LoadPictureBinary(Picture picture, bool fromDb)
        {
            //check
            if (picture == null)
                throw new ArgumentNullException(nameof(picture));

            var result = fromDb
                ? GetPictureBinaryByPictureId(picture.Id)?.BinaryData ?? Array.Empty<byte>()
                : LoadPictureFromFile(picture.Id, picture.MimeType);

            return result;
        }

        protected virtual byte[] LoadPictureFromFile(int pictureId, string mimeType)
        {
            var lastPart = GetFileExtensionFromMimeType(mimeType);
            var fileName = $"{pictureId:0000000}_0.{lastPart}";
            var filePath = GetPictureLocalPath(fileName);

            return _fileProvider.ReadAllBytes(filePath);
        }

        protected virtual byte[] EncodeImage<TPixel>(Image<TPixel> image, IImageFormat imageFormat, int? quality = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using var stream = new MemoryStream();
            var imageEncoder = Default.ImageFormatsManager.FindEncoder(imageFormat);
            switch (imageEncoder)
            {
                case JpegEncoder jpegEncoder:
                    jpegEncoder.Subsample = JpegSubsample.Ratio444;
                    jpegEncoder.Quality = quality ?? InovatiqaDefaults.DefaultImageQuality;
                    jpegEncoder.Encode(image, stream);
                    break;

                case PngEncoder pngEncoder:
                    pngEncoder.ColorType = PngColorType.RgbWithAlpha;
                    pngEncoder.Encode(image, stream);
                    break;

                case BmpEncoder bmpEncoder:
                    bmpEncoder.BitsPerPixel = BmpBitsPerPixel.Pixel32;
                    bmpEncoder.Encode(image, stream);
                    break;

                case GifEncoder gifEncoder:
                    gifEncoder.Encode(image, stream);
                    break;

                default:
                    imageEncoder.Encode(image, stream);
                    break;
            }

            return stream.ToArray();
        }

        protected virtual Size CalculateDimensions(Size originalSize, int targetSize,
           ResizeType resizeType = ResizeType.LongestSide, bool ensureSizePositive = true)
        {
            float width, height;

            switch (resizeType)
            {
                case ResizeType.LongestSide:
                    if (originalSize.Height > originalSize.Width)
                    {
                        width = originalSize.Width * (targetSize / (float)originalSize.Height);
                        height = targetSize;
                    }
                    else
                    {
                        width = targetSize;
                        height = originalSize.Height * (targetSize / (float)originalSize.Width);
                    }

                    break;
                case ResizeType.Width:
                    width = targetSize;
                    height = originalSize.Height * (targetSize / (float)originalSize.Width);
                    break;
                case ResizeType.Height:
                    width = originalSize.Width * (targetSize / (float)originalSize.Height);
                    height = targetSize;
                    break;
                default:
                    throw new Exception("Not supported ResizeType");
            }

            if (!ensureSizePositive)
                return new Size((int)Math.Round(width), (int)Math.Round(height));

            if (width < 1)
                width = 1;
            if (height < 1)
                height = 1;

            return new Size((int)Math.Round(width), (int)Math.Round(height));
        }

        protected virtual void SaveThumb(string thumbFilePath, string thumbFileName, string mimeType, byte[] binary)
        {
            var thumbsDirectoryPath = _fileProvider.GetAbsolutePath(InovatiqaDefaults.ImageThumbsPath);
            _fileProvider.CreateDirectory(thumbsDirectoryPath);

            _fileProvider.WriteAllBytes(thumbFilePath, binary);
        }

        #endregion

        #region Getting picture local path/URL methods

        public virtual Picture InsertPicture(byte[] pictureBinary, string mimeType, string seoFilename,
            string altAttribute = null, string titleAttribute = null,
            bool isNew = true, bool validateBinary = true)
        {
            mimeType = CommonHelper.EnsureNotNull(mimeType);
            mimeType = CommonHelper.EnsureMaximumLength(mimeType, 20);

            seoFilename = CommonHelper.EnsureMaximumLength(seoFilename, 100);

            if (validateBinary)
                pictureBinary = ValidatePicture(pictureBinary, mimeType);

            var picture = new Picture
            {
                MimeType = mimeType,
                SeoFilename = seoFilename,
                AltAttribute = altAttribute,
                TitleAttribute = titleAttribute,
                IsNew = isNew
            };
            _pictureRepository.Insert(picture);
            UpdatePictureBinary(picture, InovatiqaDefaults.StoreInDb ? pictureBinary : Array.Empty<byte>());

            //if (!StoreInDb)
            //    SavePictureInFile(picture.Id, pictureBinary, mimeType);

            //event notification
            //_eventPublisher.EntityInserted(picture);

            return picture;
        }

        public virtual Picture InsertPicture(IFormFile formFile, string defaultFileName = "", string virtualPath = "")
        {
            var imgExt = new List<string>
            {
                ".bmp",
                ".gif",
                ".jpeg",
                ".jpg",
                ".jpe",
                ".jfif",
                ".pjpeg",
                ".pjp",
                ".png",
                ".tiff",
                ".tif"
            } as IReadOnlyCollection<string>;

            var fileName = formFile.FileName;
            if (string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(defaultFileName))
                fileName = defaultFileName;

            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);

            var contentType = formFile.ContentType;

            var fileExtension = _fileProvider.GetFileExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            if (imgExt.All(ext => !ext.Equals(fileExtension, StringComparison.CurrentCultureIgnoreCase)))
                return null;

            //contentType is not always available 
            //that's why we manually update it here
            //http://www.sfsu.edu/training/mimetype.htm
            if (string.IsNullOrEmpty(contentType))
            {
                switch (fileExtension)
                {
                    case ".bmp":
                        contentType = MimeTypes.ImageBmp;
                        break;
                    case ".gif":
                        contentType = MimeTypes.ImageGif;
                        break;
                    case ".jpeg":
                    case ".jpg":
                    case ".jpe":
                    case ".jfif":
                    case ".pjpeg":
                    case ".pjp":
                        contentType = MimeTypes.ImageJpeg;
                        break;
                    case ".png":
                        contentType = MimeTypes.ImagePng;
                        break;
                    case ".tiff":
                    case ".tif":
                        contentType = MimeTypes.ImageTiff;
                        break;
                    default:
                        break;
                }
            }

            var picture = InsertPicture(_downloadService.GetDownloadBits(formFile), contentType, _fileProvider.GetFileNameWithoutExtension(fileName));

            if (string.IsNullOrEmpty(virtualPath))
                return picture;

            picture.VirtualPath = _fileProvider.GetVirtualPath(virtualPath);
            UpdatePicture(picture);

            return picture;
        }

        public virtual Picture UpdatePicture(Picture picture)
        {
            if (picture == null)
                return null;

            var seoFilename = CommonHelper.EnsureMaximumLength(picture.SeoFilename, 100);

            if (seoFilename != picture.SeoFilename)
                DeletePictureThumbs(picture);

            picture.SeoFilename = seoFilename;

            _pictureRepository.Update(picture);
            UpdatePictureBinary(picture, InovatiqaDefaults.StoreInDb ? GetPictureBinaryByPictureId(picture.Id).BinaryData : Array.Empty<byte>());

            //if (!StoreInDb)
            //    SavePictureInFile(picture.Id, GetPictureBinaryByPictureId(picture.Id).BinaryData, picture.MimeType);

            //event notification
            //_eventPublisher.EntityUpdated(picture);

            return picture;
        }

        public virtual byte[] ValidatePicture(byte[] pictureBinary, string mimeType)
        {
            using var image = Image.Load<Rgba32>(pictureBinary, out var imageFormat);
            if (Math.Max(image.Height, image.Width) > InovatiqaDefaults.MaximumImageSize)
            {
                image.Mutate(imageProcess => imageProcess.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(InovatiqaDefaults.MaximumImageSize)
                }));
            }

            return EncodeImage(image, imageFormat);
        }

        public virtual Picture UpdatePicture(int pictureId, byte[] pictureBinary, string mimeType,
            string seoFilename, string altAttribute = null, string titleAttribute = null,
            bool isNew = true, bool validateBinary = true)
        {
            mimeType = CommonHelper.EnsureNotNull(mimeType);
            mimeType = CommonHelper.EnsureMaximumLength(mimeType, 20);

            seoFilename = CommonHelper.EnsureMaximumLength(seoFilename, 100);

            if (validateBinary)
                pictureBinary = ValidatePicture(pictureBinary, mimeType);

            var picture = GetPictureById(pictureId);
            if (picture == null)
                return null;

            if (seoFilename != picture.SeoFilename)
                DeletePictureThumbs(picture);

            picture.MimeType = mimeType;
            picture.SeoFilename = seoFilename;
            picture.AltAttribute = altAttribute;
            picture.TitleAttribute = titleAttribute;
            picture.IsNew = isNew;

            _pictureRepository.Update(picture);
            UpdatePictureBinary(picture, InovatiqaDefaults.StoreInDb ? pictureBinary : Array.Empty<byte>());

            //if (!StoreInDb)
            //    SavePictureInFile(picture.Id, pictureBinary, mimeType);

            //event notification
            //_eventPublisher.EntityUpdated(picture);

            return picture;
        }

        public virtual IList<Picture> GetPicturesByProductId(int productId, int recordsToReturn = 0)
        {
            if (productId == 0)
                return new List<Picture>();

            var query = from p in _pictureRepository.Query()
                        join pp in _productPictureRepository.Query() on p.Id equals pp.PictureId
                        orderby pp.DisplayOrder, pp.Id
                        where pp.ProductId == productId
                        select p;

            if (recordsToReturn > 0)
                query = query.Take(recordsToReturn);

            var pics = query.ToList();
            return pics;
        }

        public virtual string GetPictureUrl(ref Picture picture,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity)
        {
            if (picture == null)
                return showDefaultPicture ? GetDefaultPictureUrl(targetSize, defaultPictureType, storeLocation) : string.Empty;

            byte[] pictureBinary = null;
           
            //changes by hamza for thumbs
            var name = picture.SeoFilename;
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            var seoFileName = System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
            //var seoFileName = Guid.NewGuid().ToString();
            var lastPart = GetFileExtensionFromMimeType(picture.MimeType);
            string thumbFileName;
            if (targetSize == 0)
            {
                //thumbFileName = !string.IsNullOrEmpty(seoFileName)
                //    ? $"{picture.Id:0000000}_{seoFileName}.{lastPart}"
                //    : $"{picture.Id:0000000}.{lastPart}";
                thumbFileName = $"{picture.Id:0000000}.{lastPart}";
            }
            else
            {
                //thumbFileName = !string.IsNullOrEmpty(seoFileName)
                //    ? $"{picture.Id:0000000}_{seoFileName}_{targetSize}.{lastPart}"
                //    : $"{picture.Id:0000000}_{targetSize}.{lastPart}";
                thumbFileName = $"{picture.Id:0000000}_{targetSize}.{lastPart}";
            }

            var thumbFilePath = GetThumbLocalPath(thumbFileName);

            using (var mutex = new Mutex(false, thumbFileName))
            {
                if (GeneratedThumbExists(thumbFilePath, thumbFileName))
                    return GetThumbUrl(thumbFileName, storeLocation);

                mutex.WaitOne();

                if (!GeneratedThumbExists(thumbFilePath, thumbFileName))
                {
                    pictureBinary ??= LoadPictureBinary(picture);

                    if ((pictureBinary?.Length ?? 0) == 0)
                        return showDefaultPicture ? GetDefaultPictureUrl(targetSize, defaultPictureType, storeLocation) : string.Empty;

                    byte[] pictureBinaryResized;
                    if (targetSize != 0)
                    {
                        using var image = Image.Load<Rgba32>(pictureBinary, out var imageFormat);
                        image.Mutate(imageProcess => imageProcess.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Max,
                            Size = CalculateDimensions(image.Size(), targetSize)
                        }));

                        pictureBinaryResized = EncodeImage(image, imageFormat);
                    }
                    else
                    {
                        pictureBinaryResized = pictureBinary.ToArray();
                    }

                    SaveThumb(thumbFilePath, thumbFileName, picture.MimeType, pictureBinaryResized);
                }

                mutex.ReleaseMutex();
            }

            return GetThumbUrl(thumbFileName, storeLocation);
        }

        public virtual string GetDefaultPictureUrl(int targetSize = 0,
            PictureType defaultPictureType = PictureType.Entity,
            string storeLocation = null)
        {
            var defaultImageFileName = defaultPictureType switch
            {
                PictureType.Avatar => InovatiqaDefaults.DefaultAvatarFileName,
                _ => InovatiqaDefaults.DefaultImageFileName,
            };
            var filePath = GetPictureLocalPath(defaultImageFileName);
            if (!_fileProvider.FileExists(filePath))
            {
                return string.Empty;
            }

            if (targetSize == 0)
            {
                var url = GetImagesPathUrl(storeLocation) + defaultImageFileName;

                return url;
            }
            else
            {
                var fileExtension = _fileProvider.GetFileExtension(filePath);
                var thumbFileName = $"{_fileProvider.GetFileNameWithoutExtension(filePath)}_{targetSize}{fileExtension}";
                var thumbFilePath = GetThumbLocalPath(thumbFileName);

                var url = GetThumbUrl(thumbFileName, storeLocation);
                return url;
            }
        }

        public virtual string GetPictureUrl(int pictureId,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity)
        {
            var picture = GetPictureById(pictureId);
            return GetPictureUrl(ref picture, targetSize, showDefaultPicture, storeLocation, defaultPictureType);
        }

        public virtual string GetThumbLocalPath(Picture picture, int targetSize = 0, bool showDefaultPicture = true)
        {
            var url = GetPictureUrl(ref picture, targetSize, showDefaultPicture);
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            return GetThumbLocalPath(_fileProvider.GetFileName(url));
        }

        public virtual string GetFileExtensionFromMimeType(string mimeType)
        {
            if (mimeType == null)
                return null;

            var parts = mimeType.Split('/');
            var lastPart = parts[parts.Length - 1];
            switch (lastPart)
            {
                case "pjpeg":
                    lastPart = "jpg";
                    break;
                case "x-png":
                    lastPart = "png";
                    break;
                case "x-icon":
                    lastPart = "ico";
                    break;
            }

            return lastPart;
        }

        public virtual byte[] LoadPictureBinary(Picture picture)
        {
            return LoadPictureBinary(picture, InovatiqaDefaults.LoadPictureFromDb);
        }

        public virtual PictureBinary GetPictureBinaryByPictureId(int pictureId)
        {
            //check
            return _pictureBinaryRepository.Query().FirstOrDefault(pb => pb.PictureId == pictureId);
        }

        #endregion

        #region CRUD methods

        public virtual void DeletePicture(Picture picture)
        {
            if (picture == null)
                throw new ArgumentNullException(nameof(picture));

            DeletePictureThumbs(picture);

            DeletePictureOnFileSystem(picture);

            _pictureRepository.Delete(picture);

            //event notification
            //_eventPublisher.EntityDeleted(picture);
        }

        public virtual Picture GetPictureById(int pictureId)
        {
            if (pictureId == 0)
                return null;

            return _pictureRepository.GetById(pictureId);
        }

        public virtual Picture GetProductPicture(Product product, string attributesXml)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var combination = _productAttributeParserService.FindProductAttributeCombination(product, attributesXml);
            var combinationPicture = GetPictureById(combination?.PictureId ?? 0);
            if (combinationPicture != null)
                return combinationPicture;

            var attributePicture = _productAttributeParserService.ParseProductAttributeValues(attributesXml)
                .Select(attributeValue => GetPictureById(attributeValue?.PictureId ?? 0))
                .FirstOrDefault(picture => picture != null);
            if (attributePicture != null)
                return attributePicture;

            var productPicture = GetPicturesByProductId(product.Id, 1).FirstOrDefault();
            if (productPicture != null)
                return productPicture;

            if (product.VisibleIndividually || product.ParentGroupedProductId <= 0)
                return null;

            var parentGroupedProductPicture = GetPicturesByProductId(product.ParentGroupedProductId, 1).FirstOrDefault();
            return parentGroupedProductPicture;
        }

        #endregion

        #region Properties


        #endregion
    }
}