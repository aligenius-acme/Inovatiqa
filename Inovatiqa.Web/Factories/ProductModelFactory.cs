using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Extensions;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Services.Media.Interfaces;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.Vendors.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Catalog;
using Inovatiqa.Web.Models.Common;
using Inovatiqa.Web.Models.Media;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Factories
{
    public partial class ProductModelFactory : IProductModelFactory
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPictureService _pictureService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IDateRangeService _dateRangeService;
        private readonly IVendorService _vendorService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IWorkContextService _workContextService;
        private readonly ICustomerService _customerService;
        private readonly IProductAttributeParserService _productAttributeParserService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IReviewTypeService _reviewTypeService;
        private readonly IDateTimeHelperService _dateTimeHelperService;
        private readonly ICompareProductsService _compareProductsService;

        #endregion

        #region Ctor

        public ProductModelFactory(ICategoryService categoryService,
            IProductService productService,
            IUrlRecordService urlRecordService,
            IPriceFormatter priceFormatter,
            IPictureService pictureService,
            ISpecificationAttributeService specificationAttributeService,
            IDateRangeService dateRangeService,
            IVendorService vendorService,
            IProductAttributeService productAttributeService,
            IWorkContextService workContextService,
            ICustomerService customerService,
            IProductAttributeParserService productAttributeParserService,
            IManufacturerService manufacturerService,
            IPriceCalculationService priceCalculationService,
            IReviewTypeService reviewTypeService,
            IDateTimeHelperService dateTimeHelperService,
            ICompareProductsService compareProductsService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _priceFormatter = priceFormatter;
            _pictureService = pictureService;
            _specificationAttributeService = specificationAttributeService;
            _dateRangeService = dateRangeService;
            _vendorService = vendorService;
            _productAttributeService = productAttributeService;
            _workContextService = workContextService;
            _customerService = customerService;
            _productAttributeParserService = productAttributeParserService;
            _manufacturerService = manufacturerService;
            _priceCalculationService = priceCalculationService;
            _reviewTypeService = reviewTypeService;
            _dateTimeHelperService = dateTimeHelperService;
            _compareProductsService = compareProductsService;
        }

        #endregion

        #region Utilities

        protected virtual ProductOverviewModel.ProductPriceModel PrepareProductOverviewPriceModel(Product product, bool forceRedirectionAfterAddingToCart = false)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var priceModel = new ProductOverviewModel.ProductPriceModel
            {
                ForceRedirectionAfterAddingToCart = forceRedirectionAfterAddingToCart
            };

            PrepareSimpleProductOverviewPriceModel(product, priceModel);

            return priceModel;
        }

        protected virtual void PrepareSimpleProductOverviewPriceModel(Product product, ProductOverviewModel.ProductPriceModel priceModel)
        {
            priceModel.DisableBuyButton = false;

            priceModel.DisableWishlistButton = false;
            priceModel.DisableAddToCompareListButton = false;

            priceModel.IsRental = false;

            var customer = _workContextService.CurrentCustomer;

            var minPossiblePriceWithDiscount = _priceCalculationService.GetFinalPrice(product, customer);

            if (product.HasTierPrices)
            {
                minPossiblePriceWithDiscount = Math.Min(minPossiblePriceWithDiscount,
                    _priceCalculationService.GetFinalPrice(product, customer, includeDiscounts: true, quantity: int.MaxValue));
            }

            var finalPriceWithDiscountBase = minPossiblePriceWithDiscount;

            priceModel.Price = _priceFormatter.FormatPrice(finalPriceWithDiscountBase);
            ////priceModel.PriceValue = product.Price;

            priceModel.PriceValue = finalPriceWithDiscountBase;


            priceModel.DisplayTaxShippingInfo = false;
        }

        public virtual PictureModel PrepareProductOverviewPictureModel(Product product, int? productThumbPictureSize = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productName = product.Name;            
            var pictureSize = productThumbPictureSize ?? InovatiqaDefaults.ProductThumbPictureSize;

            var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
            var pictureModel = new PictureModel
            {
                ImageUrl = _pictureService.GetPictureUrl(ref picture, pictureSize),
                FullSizeImageUrl = _pictureService.GetPictureUrl(ref picture),
                Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                    ? picture.TitleAttribute
                    : string.Format("Show details for {0}",
                        productName),
                AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                    ? picture.AltAttribute
                    : string.Format("Picture of {0}",
                        productName)
            };

            return pictureModel;
        }

        protected virtual ProductReviewOverviewModel PrepareProductReviewOverviewModel(Product product)
        {
            ProductReviewOverviewModel productReview;

            productReview = new ProductReviewOverviewModel
            {
                RatingSum = product.ApprovedRatingSum,
                TotalReviews = product.ApprovedTotalReviews
            };

            if (productReview != null)
            {
                productReview.ProductId = product.Id;
                productReview.AllowCustomerReviews = product.AllowCustomerReviews;
            }

            return productReview;
        }

        protected virtual ProductDetailsModel.ProductBreadcrumbModel PrepareProductBreadcrumbModel(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var breadcrumbModel = new ProductDetailsModel.ProductBreadcrumbModel
            {
                Enabled = true,
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId)
            };
            var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id);
            if (!productCategories.Any())
                return breadcrumbModel;

            var category = _categoryService.GetCategoryById(productCategories[productCategories.Count - 1].CategoryId);
            if (category == null)
                return breadcrumbModel;

            foreach (var catBr in _categoryService.GetCategoryBreadCrumb(category))
            {
                breadcrumbModel.CategoryBreadcrumb.Add(new CategorySimpleModel
                {
                    Id = catBr.Id,
                    Name = catBr.Name,
                    SeName = _urlRecordService.GetActiveSlug(catBr.Id, InovatiqaDefaults.CategorySlugName, InovatiqaDefaults.LanguageId),
                    IncludeInTopMenu = false
                });
            }

            return breadcrumbModel;
        }

        protected virtual PictureModel PrepareProductDetailsPictureModel(Product product, bool isAssociatedProduct, out IList<PictureModel> allPictureModels)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var defaultPictureSize = isAssociatedProduct ?
                InovatiqaDefaults.AssociatedProductPictureSize :
                InovatiqaDefaults.ProductDetailsPictureSize;

            var productName = product.Name;

            var pictures = _pictureService.GetPicturesByProductId(product.Id);
            var defaultPicture = pictures.FirstOrDefault();
            var defaultPictureModel = new PictureModel
            {
                ImageUrl = _pictureService.GetPictureUrl(ref defaultPicture, defaultPictureSize, !isAssociatedProduct),
                FullSizeImageUrl = _pictureService.GetPictureUrl(ref defaultPicture, 0, !isAssociatedProduct)
            };
            defaultPictureModel.Title = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.TitleAttribute)) ?
                defaultPicture.TitleAttribute :
                string.Format("Picture of {0}", productName);
            defaultPictureModel.AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
                defaultPicture.AltAttribute :
                string.Format("Picture of {0}", productName);

            var pictureModels = new List<PictureModel>();
            for (var i = 0; i < pictures.Count(); i++)
            {
                var picture = pictures[i];
                var pictureModel = new PictureModel
                {
                    ImageUrl = _pictureService.GetPictureUrl(ref picture, defaultPictureSize, !isAssociatedProduct),
                    ThumbImageUrl = _pictureService.GetPictureUrl(ref picture, InovatiqaDefaults.ProductThumbPictureSizeOnProductDetailsPage),
                    FullSizeImageUrl = _pictureService.GetPictureUrl(ref picture),
                    Title = string.Format("Picture of {0}", productName),
                    AlternateText = string.Format("Picture of {0}", productName),
                };
                pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                    picture.TitleAttribute :
                    string.Format("Picture of {0}", productName);
                pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                    picture.AltAttribute :
                    string.Format("Picture of {0}", productName);

                pictureModels.Add(pictureModel);
            }

            allPictureModels = pictureModels;
            return defaultPictureModel;
        }

        protected virtual ProductDetailsModel.ProductPriceModel PrepareProductPriceModel(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new ProductDetailsModel.ProductPriceModel
            {
                ProductId = product.Id
            };

            model.HidePrices = false;

            var customer = _workContextService.CurrentCustomer;

            var minPossiblePriceWithDiscount = _priceCalculationService.GetFinalPrice(product, customer);

            if (product.HasTierPrices)
            {
                minPossiblePriceWithDiscount = Math.Min(minPossiblePriceWithDiscount,
                    _priceCalculationService.GetFinalPrice(product, customer, includeDiscounts: true, quantity: int.MaxValue));
            }

            var finalPriceWithDiscountBase = minPossiblePriceWithDiscount;

            model.Price = _priceFormatter.FormatPrice(finalPriceWithDiscountBase);

            ///////model.PriceValue = product.Price;
            model.PriceValue = finalPriceWithDiscountBase;

            model.DisplayTaxShippingInfo = false;

            model.OrignalPrice = product.Price;

            model.BasePricePAngV = null;
            model.CurrencyCode = InovatiqaDefaults.CurrencyCode;

            return model;
        }

        protected virtual ProductDetailsModel.AddToCartModel PrepareProductAddToCartModel(Product product, ShoppingCartItem updatecartitem)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new ProductDetailsModel.AddToCartModel
            {
                ProductId = product.Id
            };

            if (updatecartitem != null)
            {
                model.UpdatedShoppingCartItemId = updatecartitem.Id;
                model.UpdateShoppingCartItemTypeId = updatecartitem.ShoppingCartTypeId;
            }

            model.EnteredQuantity = updatecartitem != null ? updatecartitem.Quantity : product.OrderMinimumQuantity;
            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            foreach (var qty in allowedQuantities)
            {
                model.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString(),
                    Selected = updatecartitem != null && updatecartitem.Quantity == qty
                });
            }
            if (product.OrderMinimumQuantity > 1)
            {
                model.MinimumQuantityNotification = string.Format("This product has a minimum quantity of {0}", product.OrderMinimumQuantity);
            }

            model.DisableBuyButton = false;
            model.DisableWishlistButton = false;

            model.IsRental = product.IsRental;

            model.CustomerEntersPrice = product.CustomerEntersPrice;

            return model;
        }

        //protected virtual IList<ProductDetailsModel.ProductAttributeModel> PrepareProductAttributeModels(Product product, ShoppingCartItem updatecartitem)
        //{
        //    if (product == null)
        //        throw new ArgumentNullException(nameof(product));

        //    var model = new List<ProductDetailsModel.ProductAttributeModel>();

        //    var productAttributeMapping = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
        //    foreach (var attribute in productAttributeMapping)
        //    {
        //        var productAttrubute = _productAttributeService.GetProductAttributeById(attribute.ProductAttributeId);

        //        var attributeModel = new ProductDetailsModel.ProductAttributeModel
        //        {
        //            Id = attribute.Id,
        //            ProductId = product.Id,
        //            ProductAttributeId = attribute.ProductAttributeId,
        //            Name = productAttrubute.Name,
        //            Description = productAttrubute.Description,
        //            TextPrompt = attribute.TextPrompt,
        //            IsRequired = attribute.IsRequired,
        //            AttributeControlTypeId = attribute.AttributeControlTypeId,
        //            DefaultValue = updatecartitem != null ? null : attribute.DefaultValue,
        //            HasCondition = !string.IsNullOrEmpty(attribute.ConditionAttributeXml)
        //        };
        //        if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
        //        {
        //            attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
        //                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        //                .ToList();
        //        }

        //        if (attribute.ShouldHaveValues())
        //        {
        //            var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
        //            foreach (var attributeValue in attributeValues)
        //            {
        //                var valueModel = new ProductDetailsModel.ProductAttributeValueModel
        //                {
        //                    Id = attributeValue.Id,
        //                    Name = attributeValue.Name,
        //                    ColorSquaresRgb = attributeValue.ColorSquaresRgb,
        //                    IsPreSelected = attributeValue.IsPreSelected,
        //                    CustomerEntersQty = attributeValue.CustomerEntersQty,
        //                    Quantity = attributeValue.Quantity,
        //                    MSku = attributeValue.Msku,
        //                    //ParentProductURL = _urlRecordService.GetActiveSlug(attributeValue.ParentProductId, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId)
        //                };
        //                attributeModel.Values.Add(valueModel);

        //                if (InovatiqaDefaults.DisplayPrices)
        //                {
        //                    var customer = updatecartitem?.CustomerId is null ? _workContextService.CurrentCustomer : _customerService.GetCustomerById(updatecartitem.CustomerId);

        //                    var attributeValuePriceAdjustment = _priceCalculationService.GetProductAttributeValuePriceAdjustment(product, attributeValue, customer);
        //                    var priceAdjustmentBase = attributeValuePriceAdjustment;
        //                    var priceAdjustment = priceAdjustmentBase;

        //                    if (attributeValue.PriceAdjustmentUsePercentage)
        //                    {
        //                        var priceAdjustmentStr = attributeValue.PriceAdjustment.ToString("G29");
        //                        if (attributeValue.PriceAdjustment > decimal.Zero)
        //                            valueModel.PriceAdjustment = "+";
        //                        valueModel.PriceAdjustment += priceAdjustmentStr + "%";
        //                    }
        //                    else
        //                    {
        //                        if (priceAdjustmentBase > decimal.Zero)
        //                            valueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment);
        //                        else if (priceAdjustmentBase < decimal.Zero)
        //                            valueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment);
        //                    }

        //                    valueModel.PriceAdjustmentValue = priceAdjustment;
        //                }

        //                if (attributeValue.ImageSquaresPictureId > 0)
        //                {
        //                    var imageSquaresPicture = _pictureService.GetPictureById(attributeValue.ImageSquaresPictureId);
        //                    var pictureModel = new PictureModel
        //                    {
        //                        FullSizeImageUrl = _pictureService.GetPictureUrl(ref imageSquaresPicture),
        //                        ImageUrl = _pictureService.GetPictureUrl(ref imageSquaresPicture, InovatiqaDefaults.ImageSquarePictureSize)
        //                    };

        //                    valueModel.ImageSquaresPictureModel = pictureModel;
        //                }

        //                valueModel.PictureId = attributeValue.PictureId;
        //            }
        //        }

        //        if (updatecartitem != null)
        //        {
        //            switch (attribute.AttributeControlTypeId)
        //            {
        //                case 1:
        //                case 2:
        //                case 3:
        //                case 40:
        //                case 45:
        //                    {
        //                        if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
        //                        {
        //                            foreach (var item in attributeModel.Values)
        //                                item.IsPreSelected = false;

        //                            var selectedValues = _productAttributeParserService.ParseProductAttributeValues(updatecartitem.AttributesXml);
        //                            foreach (var attributeValue in selectedValues)
        //                                foreach (var item in attributeModel.Values)
        //                                    if (attributeValue.Id == item.Id)
        //                                    {
        //                                        item.IsPreSelected = true;

        //                                        if (attributeValue.CustomerEntersQty)
        //                                            item.Quantity = attributeValue.Quantity;
        //                                    }
        //                        }
        //                    }

        //                    break;
        //                case 50:
        //                    {
        //                        if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
        //                        {
        //                            foreach (var attributeValue in _productAttributeParserService.ParseProductAttributeValues(updatecartitem.AttributesXml)
        //                                .Where(value => value.CustomerEntersQty))
        //                            {
        //                                var item = attributeModel.Values.FirstOrDefault(value => value.Id == attributeValue.Id);
        //                                if (item != null)
        //                                    item.Quantity = attributeValue.Quantity;
        //                            }
        //                        }
        //                    }

        //                    break;
        //                case 4:
        //                case 10:
        //                    {
        //                        if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
        //                        {
        //                            var enteredText = _productAttributeParserService.ParseValues(updatecartitem.AttributesXml, attribute.Id);
        //                            if (enteredText.Any())
        //                                attributeModel.DefaultValue = enteredText[0];
        //                        }
        //                    }

        //                    break;
        //                case 20:
        //                    {
        //                        var selectedDateStr = _productAttributeParserService.ParseValues(updatecartitem.AttributesXml, attribute.Id);
        //                        if (selectedDateStr.Any())
        //                        {
        //                            if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture, DateTimeStyles.None, out var selectedDate))
        //                            {
        //                                attributeModel.SelectedDay = selectedDate.Day;
        //                                attributeModel.SelectedMonth = selectedDate.Month;
        //                                attributeModel.SelectedYear = selectedDate.Year;
        //                            }
        //                        }
        //                    }

        //                    break;
        //                default:
        //                    break;
        //            }
        //        }

        //        model.Add(attributeModel);
        //    }

        //    return model;
        //}

        //by hamza for elasticsearch
        public virtual IList<ManufacturerBriefInfoModel> PrepareManufacturersModels(List<Manufacturer> manufacturer)
        {
            if (manufacturer == null)
            {
                throw new ArgumentNullException(nameof(manufacturer));
            }
            var model = new List<ManufacturerBriefInfoModel>();
            foreach (var manu in manufacturer)
            {
                var manufacturers = _manufacturerService.GetManufacturerById(manu.Id);
                model.Add(new ManufacturerBriefInfoModel
                {
                    Id = manufacturers.Id,
                    Name = manufacturers.Name
                });
            }
            return model;
        }

        protected virtual IList<ManufacturerBriefInfoModel> PrepareProductManufacturerModels(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = _manufacturerService.GetProductManufacturersByProductId(product.Id)
                .Select(pm =>
                {
                    var manufacturer = _manufacturerService.GetManufacturerById(pm.ManufacturerId);
                    var modelMan = new ManufacturerBriefInfoModel
                    {
                        Id = manufacturer.Id,
                        Name = manufacturer.Name,
                        SeName = _urlRecordService.GetActiveSlug(manufacturer.Id, InovatiqaDefaults.ManufacturerSlugName, InovatiqaDefaults.LanguageId)
                    };

                    return modelMan;
                }).ToList();

            return model;
        }

        // by hamza for elasticsearch

        public virtual IList<CategoryModel> PrepareCategoryModel(IList<Category> categories)
        {
            if (categories == null)
            {
                throw new ArgumentNullException(nameof(categories));
            }
            var model = new List<CategoryModel>();
            foreach (var cat in categories)
            {
                var category = _categoryService.GetCategoryById(cat.Id);
                var catModel = new CategoryModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    MetaKeywords = category.MetaKeywords,
                    MetaDescription = category.MetaDescription,
                    MetaTitle = category.MetaTitle,
                    SeName = _urlRecordService.GetActiveSlug(category.Id, InovatiqaDefaults.CategorySlugName, InovatiqaDefaults.LanguageId),
                    ChildCategories = _categoryService.GetChildCategories(category.Id).ToList()
                };
                foreach (var cc in catModel.ChildCategories)
                {
                    catModel.childCategoriesLinks.Add(new CategoryModel
                    {
                        SeName = _urlRecordService.GetActiveSlug(cc.Id, InovatiqaDefaults.CategorySlugName, InovatiqaDefaults.LanguageId),
                        Name = cc.Name
                    });
                }
                var picture = _pictureService.GetPictureById(category.PictureId);
                var pictureModel = new PictureModel
                {
                    FullSizeImageUrl = _pictureService.GetPictureUrl(ref picture),
                    ImageUrl = _pictureService.GetPictureUrl(ref picture, 0),
                    Title = string.Format(
                        "Show products in category {0}",
                        catModel.Name),
                    AlternateText =
                        string.Format(
                            "Picture for category {0}",
                            catModel.Name)
                };
                catModel.PictureModel = pictureModel;
                model.Add(catModel);
            }
            
            return model;
        }
        public virtual IList<CategoryModel> PrepareProductCategoriesModel (Product product)
        {
            if(product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }
            var model = _categoryService.GetProductCategoriesByProductId(product.Id)
                .Select(pc =>
                {
                    var categories = _categoryService.GetCategoryById(pc.CategoryId);
                    var modelCat = new CategoryModel
                    {
                        Id = categories.Id,
                        Name = categories.Name,
                        childCategory = _categoryService.GetChildCategories(categories.Id).Select(s => new KeyValuePair<string, int> (s.Name, s.Id)).ToList(),
                        ParentCategoriesId = categories.ParentCategoryId
                    };
                    return modelCat;
                }).ToList();
            return model;
        }

        #endregion

        //by hamza for elasticsearch
        public virtual IList<ProductDetailsModel.ProductAttributeModel> PrepareAttributeModels(IList<ProductAttribute> attributes)
        {
            if (attributes == null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }
            var model = new List<ProductDetailsModel.ProductAttributeModel>();
            foreach (var attribute in attributes)
            {
                var attributeModel = new ProductDetailsModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.Name,
                    Description = attribute.Description
                };
                var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                foreach (var attributeValue in attributeValues)
                {
                    var valueModel = new ProductDetailsModel.ProductAttributeValueModel
                    {
                        Id = attributeValue.Id,
                        Name = attributeValue.Name,
                        ColorSquaresRgb = attributeValue.ColorSquaresRgb,
                        IsPreSelected = attributeValue.IsPreSelected,
                        CustomerEntersQty = attributeValue.CustomerEntersQty,
                        Quantity = attributeValue.Quantity,
                        MSku = attributeValue.Msku
                    };
                    attributeModel.Values.Add(valueModel);
                }
                model.Add(attributeModel);
            }

            return model;
        }

        #region Methods
        public virtual IList<ProductDetailsModel.ProductAttributeModel> PrepareProductAttributeModels(Product product, ShoppingCartItem updatecartitem)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new List<ProductDetailsModel.ProductAttributeModel>();

            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            var customer = updatecartitem?.CustomerId is null ? _workContextService.CurrentCustomer : _customerService.GetCustomerById(updatecartitem.CustomerId);
            foreach (var attribute in productAttributeMapping)
            {
                var productAttrubute = _productAttributeService.GetProductAttributeById(attribute.ProductAttributeId);

                var attributeModel = new ProductDetailsModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductId = product.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = productAttrubute.Name,
                    Description = productAttrubute.Description,
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlTypeId = attribute.AttributeControlTypeId,
                    DefaultValue = updatecartitem != null ? null : attribute.DefaultValue,
                    HasCondition = !string.IsNullOrEmpty(attribute.ConditionAttributeXml)
                };
                if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new ProductDetailsModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb,
                            IsPreSelected = attributeValue.IsPreSelected,
                            CustomerEntersQty = attributeValue.CustomerEntersQty,
                            Quantity = attributeValue.Quantity,
                            MSku = attributeValue.Msku,
                            //ParentProductURL = _urlRecordService.GetActiveSlug(attributeValue.ParentProductId, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId)
                        };
                        attributeModel.Values.Add(valueModel);

                        if (InovatiqaDefaults.DisplayPrices)
                        {

                            var attributeValuePriceAdjustment = _priceCalculationService.GetProductAttributeValuePriceAdjustment(product, attributeValue, customer);
                            var priceAdjustmentBase = attributeValuePriceAdjustment;
                            var priceAdjustment = priceAdjustmentBase;

                            if (attributeValue.PriceAdjustmentUsePercentage)
                            {
                                var priceAdjustmentStr = attributeValue.PriceAdjustment.ToString("G29");
                                if (attributeValue.PriceAdjustment > decimal.Zero)
                                    valueModel.PriceAdjustment = "+";
                                valueModel.PriceAdjustment += priceAdjustmentStr + "%";
                            }
                            else
                            {
                                if (priceAdjustmentBase > decimal.Zero)
                                    valueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment);
                                else if (priceAdjustmentBase < decimal.Zero)
                                    valueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment);
                            }

                            valueModel.PriceAdjustmentValue = priceAdjustment;
                        }

                        if (attributeValue.ImageSquaresPictureId > 0)
                        {
                            var imageSquaresPicture = _pictureService.GetPictureById(attributeValue.ImageSquaresPictureId);
                            var pictureModel = new PictureModel
                            {
                                FullSizeImageUrl = _pictureService.GetPictureUrl(ref imageSquaresPicture),
                                ImageUrl = _pictureService.GetPictureUrl(ref imageSquaresPicture, InovatiqaDefaults.ImageSquarePictureSize)
                            };

                            valueModel.ImageSquaresPictureModel = pictureModel;
                        }

                        valueModel.PictureId = attributeValue.PictureId;
                    }
                }

                if (updatecartitem != null)
                {
                    switch (attribute.AttributeControlTypeId)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 40:
                        case 45:
                            {
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    foreach (var item in attributeModel.Values)
                                        item.IsPreSelected = false;

                                    var selectedValues = _productAttributeParserService.ParseProductAttributeValues(updatecartitem.AttributesXml);
                                    foreach (var attributeValue in selectedValues)
                                        foreach (var item in attributeModel.Values)
                                            if (attributeValue.Id == item.Id)
                                            {
                                                item.IsPreSelected = true;

                                                if (attributeValue.CustomerEntersQty)
                                                    item.Quantity = attributeValue.Quantity;
                                            }
                                }
                            }

                            break;
                        case 50:
                            {
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    foreach (var attributeValue in _productAttributeParserService.ParseProductAttributeValues(updatecartitem.AttributesXml)
                                        .Where(value => value.CustomerEntersQty))
                                    {
                                        var item = attributeModel.Values.FirstOrDefault(value => value.Id == attributeValue.Id);
                                        if (item != null)
                                            item.Quantity = attributeValue.Quantity;
                                    }
                                }
                            }

                            break;
                        case 4:
                        case 10:
                            {
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    var enteredText = _productAttributeParserService.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                    if (enteredText.Any())
                                        attributeModel.DefaultValue = enteredText[0];
                                }
                            }

                            break;
                        case 20:
                            {
                                var selectedDateStr = _productAttributeParserService.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                if (selectedDateStr.Any())
                                {
                                    if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture, DateTimeStyles.None, out var selectedDate))
                                    {
                                        attributeModel.SelectedDay = selectedDate.Day;
                                        attributeModel.SelectedMonth = selectedDate.Month;
                                        attributeModel.SelectedYear = selectedDate.Year;
                                    }
                                }
                            }

                            break;
                        default:
                            break;
                    }
                }

                model.Add(attributeModel);
            }

            return model;
        }

        public virtual IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false, bool prepareProductAttributes = true, bool prepareProductManufacturer = true, bool prepareProductCategories = false) 
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));
            var ComprisonList = _compareProductsService.GetComparedProducts();
            var models = new List<ProductOverviewModel>();
            foreach (var product in products)
            {
                var model = new ProductOverviewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    ShortDescription = product.ShortDescription,
                    FullDescription = product.FullDescription,
                    SeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId),
                    Sku = product.Sku,
                    stockStatus = product.StockQuantity > 0 ? "In Stock" : "Out of Stock",
                    quantity = product.StockQuantity ,
                    ProductTypeId = product.ProductTypeId,
                    MarkAsNew = product.MarkAsNew &&
                        (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow),
                    ManufacturerPartNumber = product.ManufacturerPartNumber,
                    IsInCompareList = ComprisonList.Where(prod => prod.Id == product.Id).ToList().Count > 0
                };
                if (prepareProductManufacturer)
                {
                    model.ProductManufacturers = PrepareProductManufacturerModels(product);
                }
                if (prepareProductAttributes)
                {
                    model.ProductAttributes = PrepareProductAttributeModels(product, null);
                }
                if (preparePriceModel)
                {
                    model.ProductPrice = PrepareProductOverviewPriceModel(product, forceRedirectionAfterAddingToCart);
                    model.ProductPrice.OrignalPrice = product.Price;
                }

                if (preparePictureModel)
                {
                    model.DefaultPictureModel = PrepareProductOverviewPictureModel(product, productThumbPictureSize);
                }

                if (prepareSpecificationAttributes)
                {
                    model.SpecificationAttributeModels = PrepareProductSpecificationModel(product);
                }
                if (prepareProductCategories)
                {
                    model.ProductCategories = PrepareProductCategoriesModel(product);
                }

                model.ReviewOverviewModel = PrepareProductReviewOverviewModel(product);

                models.Add(model);
            }

             return models;
        }

        public virtual IList<ProductSpecificationModel> PrepareProductSpecificationModel(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            return _specificationAttributeService.GetProductSpecificationAttributes(product.Id, 0, null, true)
                .Select(psa =>
                {
                    var specAttributeOption =
                        _specificationAttributeService.GetSpecificationAttributeOptionById(
                            psa.SpecificationAttributeOptionId);
                    var specAttribute =
                        _specificationAttributeService.GetSpecificationAttributeById(specAttributeOption
                            .SpecificationAttributeId);

                    var m = new ProductSpecificationModel
                    {
                        SpecificationAttributeId = specAttribute.Id,
                        SpecificationAttributeName = specAttribute.Name,
                        ColorSquaresRgb = specAttributeOption.ColorSquaresRgb,
                        AttributeTypeId = psa.AttributeTypeId
                    };

                    switch (psa.AttributeTypeId)
                    {
                        case 0:
                            m.ValueRaw =
                                WebUtility.HtmlEncode(
                                    specAttributeOption.Name);
                            break;
                        case 10:
                            m.ValueRaw =
                                WebUtility.HtmlEncode(psa.CustomValue);
                            break;
                        case 20:
                            m.ValueRaw = psa.CustomValue;
                            break;
                        case 30:
                            m.ValueRaw = $"<a href='{psa.CustomValue}' target='_blank'>{psa.CustomValue}</a>";
                            break;
                        default:
                            break;
                    }

                    return m;
                }).ToList();
        }

        public virtual ProductDetailsModel PrepareProductDetailsModel(Product product,
            ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false, decimal unitPrice = 0, bool editing = false)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            
            var model = new ProductDetailsModel
            {
                Id = product.Id,
                //add by hamza
                editing = editing,
                unitPrice = unitPrice,
                updatecartitem = updatecartitem,
                cartQuantity = updatecartitem == null ? 0: updatecartitem.Quantity,
                quantity = product.StockQuantity,
                stockStatus = product.StockQuantity > 0 ? "In Stock" : "Out of Stock",
                Name = product.Name,
                ShortDescription = product.ShortDescription,
                FullDescription = product.FullDescription,
                MetaKeywords = product.MetaKeywords,
                MetaDescription = product.MetaDescription,
                MetaTitle = product.MetaTitle,
                SeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId),
                ProductTypeId = product.ProductTypeId,
                ShowSku = InovatiqaDefaults.ShowSkuOnProductDetailsPage,
                Sku = product.Sku,
                ShowManufacturerPartNumber = InovatiqaDefaults.ShowManufacturerPartNumber,
                FreeShippingNotificationEnabled = InovatiqaDefaults.ShowFreeShippingNotification,
                ManufacturerPartNumber = product.ManufacturerPartNumber,
                ShowGtin = InovatiqaDefaults.ShowGtin,
                Gtin = product.Gtin,
                ManageInventoryMethodId = product.ManageInventoryMethodId,
                StockAvailability = _productService.FormatStockMessage(product, string.Empty),
                HasSampleDownload = product.IsDownload && product.HasSampleDownload,
                DisplayDiscontinuedMessage = !product.Published && InovatiqaDefaults.DisplayDiscontinuedMessageForUnpublishedProducts,
                AvailableEndDate = product.AvailableEndDateTimeUtc
            };

            model.MetaDescription = model.ShortDescription;


            model.IsShipEnabled = product.IsShipEnabled;
            if (product.IsShipEnabled)
            {
                model.IsFreeShipping = product.IsFreeShipping;
                var deliveryDate = _dateRangeService.GetDeliveryDateById(product.DeliveryDateId);
                if (deliveryDate != null)
                {
                    model.DeliveryDate = deliveryDate.Name;
                }
            }

            model.EmailAFriendEnabled = InovatiqaDefaults.EmailAFriendEnabled;
            model.CompareProductsEnabled = InovatiqaDefaults.CompareProductsEnabled;
            model.CurrentStoreName = InovatiqaDefaults.CurrentStoreName;


            if (InovatiqaDefaults.ShowVendorOnProductDetailsPage)
            {
                var vendor = _vendorService.GetVendorById(product.VendorId);
                if (vendor != null && !vendor.Deleted && vendor.Active)
                {
                    model.ShowVendor = true;

                    model.VendorModel = new VendorBriefInfoModel
                    {
                        Id = vendor.Id,
                        Name = vendor.Name,
                        SeName = _urlRecordService.GetActiveSlug(vendor.Id, InovatiqaDefaults.VendorSlugName, InovatiqaDefaults.LanguageId),
                    };
                }
            }

            model.PageShareCode = string.Empty;

            model.DisplayBackInStockSubscription = InovatiqaDefaults.DisplayBackInStockSubscription;

            model.Breadcrumb = PrepareProductBreadcrumbModel(product);

            model.DefaultPictureZoomEnabled = InovatiqaDefaults.DefaultPictureZoomEnabled;
            model.DefaultPictureModel = PrepareProductDetailsPictureModel(product, isAssociatedProduct, out var allPictureModels);
            model.PictureModels = allPictureModels;

            model.ProductPrice = PrepareProductPriceModel(product);

            model.AddToCart = PrepareProductAddToCartModel(product, updatecartitem);

            model.ProductAttributes = PrepareProductAttributeModels(product, updatecartitem);

            if (!isAssociatedProduct)
            {
                model.ProductSpecifications = PrepareProductSpecificationModel(product);
            }

            model.ProductReviewOverview = PrepareProductReviewOverviewModel(product);

            model.ProductManufacturers = PrepareProductManufacturerModels(product);

            if (product.IsRental)
            {
                model.IsRental = true;
                if (updatecartitem != null)
                {
                    model.RentalStartDate = updatecartitem.RentalStartDateUtc;
                    model.RentalEndDate = updatecartitem.RentalEndDateUtc;
                }
            }

            return model;
        }

        public virtual ProductReviewsModel PrepareProductReviewsModel(ProductReviewsModel model, Product product)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            model.ProductId = product.Id;
            model.ProductName = product.Name;
            model.ProductSeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId);

            var productReviews = _productService.GetAllProductReviews(
                approved: true,
                productId: product.Id,
                storeId: InovatiqaDefaults.StoreId).AsEnumerable();

            productReviews = InovatiqaDefaults.ProductReviewsSortByCreatedDateAscending
                ? productReviews.OrderBy(pr => pr.CreatedOnUtc)
                : productReviews.OrderByDescending(pr => pr.CreatedOnUtc);

            foreach (var reviewType in _reviewTypeService.GetAllReviewTypes())
            {
                model.ReviewTypeList.Add(new ReviewTypeModel
                {
                    Id = reviewType.Id,
                    Name = reviewType.Name,
                    Description = reviewType.Description,
                    VisibleToAllCustomers = reviewType.VisibleToAllCustomers,
                    DisplayOrder = reviewType.DisplayOrder,
                    IsRequired = reviewType.IsRequired,
                });
            }

            foreach (var pr in productReviews)
            {
                var customer = _customerService.GetCustomerById(pr.CustomerId);

                var productReviewModel = new ProductReviewModel
                {
                    Id = pr.Id,
                    CustomerId = pr.CustomerId,
                    CustomerName = _customerService.FormatUsername(customer),
                    AllowViewingProfiles = InovatiqaDefaults.AllowViewingProfiles && customer != null && !_customerService.IsGuest(customer),
                    Title = pr.Title,
                    ReviewText = pr.ReviewText,
                    ReplyText = pr.ReplyText,
                    Rating = pr.Rating,
                    Helpfulness = new ProductReviewHelpfulnessModel
                    {
                        ProductReviewId = pr.Id,
                        HelpfulYesTotal = pr.HelpfulYesTotal,
                        HelpfulNoTotal = pr.HelpfulNoTotal,
                    },
                    WrittenOnStr = _dateTimeHelperService.ConvertToUserTime(pr.CreatedOnUtc, DateTimeKind.Utc).ToString("g"),
                };

                foreach (var q in _reviewTypeService.GetProductReviewReviewTypeMappingsByProductReviewId(pr.Id))
                {
                    var reviewType = _reviewTypeService.GetReviewTypeById(q.ReviewTypeId);

                    productReviewModel.AdditionalProductReviewList.Add(new ProductReviewReviewTypeMappingModel
                    {
                        ReviewTypeId = q.ReviewTypeId,
                        ProductReviewId = pr.Id,
                        Rating = q.Rating,
                        Name = reviewType.Name,
                        VisibleToAllCustomers = reviewType.VisibleToAllCustomers || _workContextService.CurrentCustomer.Id == pr.CustomerId,
                    });
                }

                model.Items.Add(productReviewModel);
            }

            foreach (var rt in model.ReviewTypeList)
            {
                if (model.ReviewTypeList.Count <= model.AddAdditionalProductReviewList.Count)
                    continue;
                var reviewType = _reviewTypeService.GetReviewTypeById(rt.Id);
                var reviewTypeMappingModel = new AddProductReviewReviewTypeMappingModel
                {
                    ReviewTypeId = rt.Id,
                    Name = reviewType.Name,
                    Description = reviewType.Description,
                    DisplayOrder = rt.DisplayOrder,
                    IsRequired = rt.IsRequired,
                };

                model.AddAdditionalProductReviewList.Add(reviewTypeMappingModel);
            }

            foreach (var rtm in model.ReviewTypeList)
            {
                var totalRating = 0;
                var totalCount = 0;
                foreach (var item in model.Items)
                {
                    foreach (var q in item.AdditionalProductReviewList.Where(w => w.ReviewTypeId == rtm.Id))
                    {
                        totalRating += q.Rating;
                        totalCount = ++totalCount;
                    }
                }

                rtm.AverageRating = (double)totalRating / (totalCount > 0 ? totalCount : 1);
            }

            model.AddProductReview.CanCurrentCustomerLeaveReview = InovatiqaDefaults.AllowAnonymousUsersToReviewProduct || !_customerService.IsGuest(_workContextService.CurrentCustomer);
            model.AddProductReview.DisplayCaptcha = InovatiqaDefaults.Enabled && InovatiqaDefaults.ShowOnProductReviewPage;

            return model;
        }

        public virtual CustomerProductReviewsModel PrepareCustomerProductReviewsModel(int? page)
        {
            var pageSize = InovatiqaDefaults.ProductReviewsPageSizeOnAccountPage;
            var pageIndex = 0;

            if (page > 0)
            {
                pageIndex = page.Value - 1;
            }

            var list = _productService.GetAllProductReviews(customerId: _workContextService.CurrentCustomer.Id,
                approved: null,
                storeId: InovatiqaDefaults.StoreId,
                pageIndex: pageIndex,
                pageSize: pageSize);

            var productReviews = new List<CustomerProductReviewModel>();

            foreach (var review in list)
            {
                var product = _productService.GetProductById(review.ProductId);

                var productReviewModel = new CustomerProductReviewModel
                {
                    Title = review.Title,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductSeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId),
                    Rating = review.Rating,
                    ReviewText = review.ReviewText,
                    ReplyText = review.ReplyText,
                    WrittenOnStr = _dateTimeHelperService.ConvertToUserTime(review.CreatedOnUtc, DateTimeKind.Utc).ToString("g")
                };

                if (InovatiqaDefaults.ProductReviewsMustBeApproved)
                {
                    productReviewModel.ApprovalStatus = review.IsApproved
                        ? "Approved"
                        : "Pending";
                }

                foreach (var q in _reviewTypeService.GetProductReviewReviewTypeMappingsByProductReviewId(review.Id))
                {
                    var reviewType = _reviewTypeService.GetReviewTypeById(q.ReviewTypeId);

                    productReviewModel.AdditionalProductReviewList.Add(new ProductReviewReviewTypeMappingModel
                    {
                        ReviewTypeId = q.ReviewTypeId,
                        ProductReviewId = review.Id,
                        Rating = q.Rating,
                        Name = reviewType.Name,
                    });
                }

                productReviews.Add(productReviewModel);
            }

            var pagerModel = new PagerModel
            {
                PageSize = list.PageSize,
                TotalRecords = list.TotalCount,
                PageIndex = list.PageIndex,
                ShowTotalSummary = false,
                RouteActionName = "CustomerProductReviewsPaged",
                UseRouteLinks = true,
                RouteValues = new CustomerProductReviewsModel.CustomerProductReviewsRouteValues { pageNumber = pageIndex }
            };

            var model = new CustomerProductReviewsModel
            {
                ProductReviews = productReviews,
                PagerModel = pagerModel
            };

            return model;
        }

        public virtual NewProductsModel PrepareNewProductsModel(CatalogPagingFilteringModel command)
        {
            var model = new NewProductsModel();

            PreparePageSizeOptions(model.PagingFilteringContext, command,
                InovatiqaDefaults.AllowCustomersToSelectPageSize,
                InovatiqaDefaults.PageSizeOptions,
                InovatiqaDefaults.PageSize);

            var products = _productService.SearchProducts(
                storeId: InovatiqaDefaults.StoreId,
                visibleIndividuallyOnly: true,
                markedAsNewOnly: true,
                orderBy: ProductSortingEnum.CreatedOn,
                pageIndex: command.PageNumber - 1,
                pageSize: command.PageSize);

            model.Products = PrepareProductOverviewModels(products).ToList();

            model.PagingFilteringContext.LoadPagedList(products);
            return model;
        }

        public virtual void PreparePageSizeOptions(CatalogPagingFilteringModel pagingFilteringModel, CatalogPagingFilteringModel command,
            bool allowCustomersToSelectPageSize, string pageSizeOptions, int fixedPageSize)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException(nameof(pagingFilteringModel));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (command.PageNumber <= 0)
                command.PageNumber = 1;

            command.PageSize = fixedPageSize;
        }

        #endregion
    }
}