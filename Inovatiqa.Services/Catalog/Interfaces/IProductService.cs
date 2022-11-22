using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Services.Catalog.Interfaces
{
    public partial interface IProductService
    {
        #region Products

        IList<Product> GetAssociatedProducts(int parentGroupedProductId,
            int storeId = 0, int vendorId = 0, bool showHidden = false);

        Product GetProductBySku(string sku);

        void AdjustInventory(Product product, int quantityToChange, string attributesXml = "", string message = "");

        void ReserveInventory(Product product, int quantity);

        void UnblockReservedInventory(Product product, int quantity);

        IPagedList<Product> SearchProducts(
            out IList<int> filterableSpecificationAttributeOptionIds,
            bool loadFilterableSpecificationAttributeOptionIds = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            int manufacturerId = 0,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool markedAsNewOnly = false,
            bool? featuredProducts = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            IList<int> filteredSpecs = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null);

        Product GetProductById(int productId);

        string FormatStockMessage(Product product, string attributesXml);

        int GetTotalStockQuantity(Product product, bool useReservedQuantity = true, int warehouseId = 0);

        int[] ParseAllowedQuantities(Product product);

        IList<Product> GetAllProductsDisplayedOnHomepage();

        bool ProductIsAvailable(Product product, DateTime? dateTime = null);

        int GetNumberOfProductsInCategory(IList<int> categoryIds = null, int storeId = 0);

        IPagedList<Product> SearchProducts(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            int manufacturerId = 0,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool markedAsNewOnly = false,
            bool? featuredProducts = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            IList<int> filteredSpecs = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null);

        IList<Product> GetProductsByIds(int[] productIds);

        int[] ParseRequiredProductIds(Product product);

        string FormatSku(Product product, string attributesXml = null);

        void UpdateProductViews(Product product);

        void UpdateProductReviewTotals(Product product);

        void UpdateProduct(Product product);

        Product GetProductByMSku(int mSKU);

        IPagedList<Product> GetLowStockProducts(int? vendorId = null, bool? loadPublishedOnly = true,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);

        IPagedList<ProductAttributeCombination> GetLowStockProductCombinations(int? vendorId = null, bool? loadPublishedOnly = true,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);
        //void IndexProducts();

        #endregion

        #region Cross-sell products

        IList<Product> GetCrosssellProductsByShoppingCart(IList<ShoppingCartItem> cart, int numberOfProducts);

        IList<CrossSellProduct> GetCrossSellProductsByProductIds(int[] productIds, bool showHidden = false);

        #endregion

        #region Most viewed products

        IList<Product> GetMostViewedProducts(int number);

        #endregion

        #region Product pictures

        void DeleteProductPicture(ProductPictureMapping productPicture);

        ProductPictureMapping GetProductPictureById(int productPictureId);

        IList<ProductPictureMapping> GetProductPicturesByProductId(int productId);

        void UpdateProductPicture(ProductPictureMapping productPicture);

        #endregion

        #region Product reviews

        IPagedList<ProductReview> GetAllProductReviews(int customerId = 0, bool? approved = null,
            DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = null, int storeId = 0, int productId = 0, int vendorId = 0, bool showHidden = false,
            int pageIndex = 0, int pageSize = int.MaxValue);

        void InsertProductReview(ProductReview productReview);

        ProductReview GetProductReviewById(int productReviewId);

        void SetProductReviewHelpfulness(ProductReview productReview, bool helpfulness);

        void InsertProductReviewHelpfulness(ProductReviewHelpfulness productReviewHelpfulness);

        void UpdateProductReviewHelpfulnessTotals(ProductReview productReview);

        (int usefulCount, int notUsefulCount) GetHelpfulnessCounts(ProductReview productReview);

        #endregion

        #region Product Warehouses

        Warehouse GetWarehousesById(int warehouseId);

        IList<ProductWarehouseInventory> GetAllProductWarehouseInventoryRecords(int productId);

        void UpdateProductWarehouseInventory(ProductWarehouseInventory pwi);

        #endregion

        #region COVID-19 products

        IList<Product> GetAllCovid19ProductsDisplayedOnHomepage();

        #endregion

        #region Stock quantity history

        void AddStockQuantityHistoryEntry(Product product, int quantityAdjustment, int stockQuantity,
            int warehouseId = 0, string message = "", int? combinationId = null);
        List<Product> GetAllProducts();

        #endregion

        #region Inventory management methods

        void BookReservedInventory(Product product, int warehouseId, int quantity, string message = "");

        void BalanceInventory(Product product, int warehouseId, int quantity);

        int ReverseBookedInventory(Product product, ShipmentItem shipmentItem, string message = "");

        #endregion

        #region Tier prices

        IList<TierPrice> GetTierPricesByProduct(int productId);

        TierPrice GetPreferredTierPrice(Product product, Customer customer, int storeId, int quantity);

        IList<TierPrice> GetTierPrices(Product product, Customer customer, int storeId);

        void InsertTierPrice(TierPrice tierPrice);

        void DeleteTierPrice(TierPrice tierPrice);

        void UpdateTierPrice(TierPrice tierPrice);

        void UpdateHasTierPricesProperty(Product product);

        TierPrice GetTierPriceById(int tierPriceId);

        #endregion
    }
}