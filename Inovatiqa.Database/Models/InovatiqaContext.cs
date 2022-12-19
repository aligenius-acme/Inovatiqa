using Microsoft.EntityFrameworkCore;

namespace Inovatiqa.Database.Models
{
    public partial class InovatiqaContext : DbContext
    {

        public InovatiqaContext(DbContextOptions<InovatiqaContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AclRecord> AclRecord { get; set; }
        public virtual DbSet<ActivityLog> ActivityLog { get; set; }
        public virtual DbSet<ActivityLogType> ActivityLogType { get; set; }
        public virtual DbSet<Address> Address { get; set; }
        public virtual DbSet<AddressAttribute> AddressAttribute { get; set; }
        public virtual DbSet<AddressAttributeValue> AddressAttributeValue { get; set; }
        public virtual DbSet<Affiliate> Affiliate { get; set; }
        public virtual DbSet<BackInStockSubscription> BackInStockSubscription { get; set; }
        public virtual DbSet<BlogComment> BlogComment { get; set; }
        public virtual DbSet<BlogPost> BlogPost { get; set; }
        public virtual DbSet<Campaign> Campaign { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<CategoryTemplate> CategoryTemplate { get; set; }
        public virtual DbSet<CheckoutAttribute> CheckoutAttribute { get; set; }
        public virtual DbSet<CheckoutAttributeValue> CheckoutAttributeValue { get; set; }
        public virtual DbSet<Country> Country { get; set; }
        public virtual DbSet<CrossSellProduct> CrossSellProduct { get; set; }
        public virtual DbSet<Currency> Currency { get; set; }
        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<CustomerAddresses> CustomerAddresses { get; set; }
        public virtual DbSet<CustomerAttribute> CustomerAttribute { get; set; }
        public virtual DbSet<CustomerAttributeValue> CustomerAttributeValue { get; set; }
        public virtual DbSet<CustomerCustomerRoleMapping> CustomerCustomerRoleMapping { get; set; }
        public virtual DbSet<CustomerPassword> CustomerPassword { get; set; }
        public virtual DbSet<CustomerReturnRequest> CustomerReturnRequest { get; set; }
        public virtual DbSet<CustomerRole> CustomerRole { get; set; }
        public virtual DbSet<DeliveryDate> DeliveryDate { get; set; }
        public virtual DbSet<Discount> Discount { get; set; }
        public virtual DbSet<DiscountAppliedToCategories> DiscountAppliedToCategories { get; set; }
        public virtual DbSet<DiscountAppliedToManufacturers> DiscountAppliedToManufacturers { get; set; }
        public virtual DbSet<DiscountAppliedToProducts> DiscountAppliedToProducts { get; set; }
        public virtual DbSet<DiscountRequirement> DiscountRequirement { get; set; }
        public virtual DbSet<DiscountUsageHistory> DiscountUsageHistory { get; set; }
        public virtual DbSet<Download> Download { get; set; }
        public virtual DbSet<EmailAccount> EmailAccount { get; set; }
        public virtual DbSet<EntityTierPrice> EntityTierPrice { get; set; }
        public virtual DbSet<ExternalAuthenticationRecord> ExternalAuthenticationRecord { get; set; }
        public virtual DbSet<ForumsForum> ForumsForum { get; set; }
        public virtual DbSet<ForumsGroup> ForumsGroup { get; set; }
        public virtual DbSet<ForumsPost> ForumsPost { get; set; }
        public virtual DbSet<ForumsPostVote> ForumsPostVote { get; set; }
        public virtual DbSet<ForumsPrivateMessage> ForumsPrivateMessage { get; set; }
        public virtual DbSet<ForumsSubscription> ForumsSubscription { get; set; }
        public virtual DbSet<ForumsTopic> ForumsTopic { get; set; }
        public virtual DbSet<GdprConsent> GdprConsent { get; set; }
        public virtual DbSet<GdprLog> GdprLog { get; set; }
        public virtual DbSet<GenericAttribute> GenericAttribute { get; set; }
        public virtual DbSet<GiftCard> GiftCard { get; set; }
        public virtual DbSet<GiftCardUsageHistory> GiftCardUsageHistory { get; set; }
        public virtual DbSet<Language> Language { get; set; }
        public virtual DbSet<LocaleStringResource> LocaleStringResource { get; set; }
        public virtual DbSet<LocalizedProperty> LocalizedProperty { get; set; }
        public virtual DbSet<Log> Log { get; set; }
        public virtual DbSet<Manufacturer> Manufacturer { get; set; }
        public virtual DbSet<ManufacturerTemplate> ManufacturerTemplate { get; set; }
        public virtual DbSet<MeasureDimension> MeasureDimension { get; set; }
        public virtual DbSet<MeasureWeight> MeasureWeight { get; set; }
        public virtual DbSet<MessageTemplate> MessageTemplate { get; set; }
        public virtual DbSet<MigrationVersionInfo> MigrationVersionInfo { get; set; }
        public virtual DbSet<News> News { get; set; }
        public virtual DbSet<NewsComment> NewsComment { get; set; }
        public virtual DbSet<NewsLetterSubscription> NewsLetterSubscription { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<OrderItem> OrderItem { get; set; }
        public virtual DbSet<OrderNote> OrderNote { get; set; }
        public virtual DbSet<PermissionRecord> PermissionRecord { get; set; }
        public virtual DbSet<PermissionRecordRoleMapping> PermissionRecordRoleMapping { get; set; }
        public virtual DbSet<Picture> Picture { get; set; }
        public virtual DbSet<PictureBinary> PictureBinary { get; set; }
        public virtual DbSet<Poll> Poll { get; set; }
        public virtual DbSet<PollAnswer> PollAnswer { get; set; }
        public virtual DbSet<PollVotingRecord> PollVotingRecord { get; set; }
        public virtual DbSet<PredefinedProductAttributeValue> PredefinedProductAttributeValue { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<ProductAttribute> ProductAttribute { get; set; }
        public virtual DbSet<ProductAttributeCombination> ProductAttributeCombination { get; set; }
        public virtual DbSet<ProductAttributeValue> ProductAttributeValue { get; set; }
        public virtual DbSet<ProductAvailabilityRange> ProductAvailabilityRange { get; set; }
        public virtual DbSet<ProductCategoryMapping> ProductCategoryMapping { get; set; }
        public virtual DbSet<ProductManufacturerMapping> ProductManufacturerMapping { get; set; }
        public virtual DbSet<ProductPictureMapping> ProductPictureMapping { get; set; }
        public virtual DbSet<ProductProductAttributeMapping> ProductProductAttributeMapping { get; set; }
        public virtual DbSet<ProductProductTagMapping> ProductProductTagMapping { get; set; }
        public virtual DbSet<ProductReview> ProductReview { get; set; }
        public virtual DbSet<ProductReviewHelpfulness> ProductReviewHelpfulness { get; set; }
        public virtual DbSet<ProductReviewReviewTypeMapping> ProductReviewReviewTypeMapping { get; set; }
        public virtual DbSet<ProductSpecificationAttributeMapping> ProductSpecificationAttributeMapping { get; set; }
        public virtual DbSet<ProductTag> ProductTag { get; set; }
        public virtual DbSet<ProductTemplate> ProductTemplate { get; set; }
        public virtual DbSet<ProductWarehouseInventory> ProductWarehouseInventory { get; set; }
        public virtual DbSet<QueuedEmail> QueuedEmail { get; set; }
        public virtual DbSet<RecurringPayment> RecurringPayment { get; set; }
        public virtual DbSet<RecurringPaymentHistory> RecurringPaymentHistory { get; set; }
        public virtual DbSet<RelatedProduct> RelatedProduct { get; set; }
        public virtual DbSet<ReturnRequest> ReturnRequest { get; set; }
        public virtual DbSet<ReturnRequestAction> ReturnRequestAction { get; set; }
        public virtual DbSet<ReturnRequestReason> ReturnRequestReason { get; set; }
        public virtual DbSet<ReviewType> ReviewType { get; set; }
        public virtual DbSet<RewardPointsHistory> RewardPointsHistory { get; set; }
        public virtual DbSet<ScheduleTask> ScheduleTask { get; set; }
        public virtual DbSet<ScrappingLastRecord> ScrappingLastRecord { get; set; }
        public virtual DbSet<SearchTerm> SearchTerm { get; set; }
        public virtual DbSet<Setting> Setting { get; set; }
        public virtual DbSet<Shipment> Shipment { get; set; }
        public virtual DbSet<ShipmentItem> ShipmentItem { get; set; }
        public virtual DbSet<ShippingMethod> ShippingMethod { get; set; }
        public virtual DbSet<ShippingMethodRestrictions> ShippingMethodRestrictions { get; set; }
        public virtual DbSet<ShoppingCartItem> ShoppingCartItem { get; set; }
        public virtual DbSet<SpecificationAttribute> SpecificationAttribute { get; set; }
        public virtual DbSet<SpecificationAttributeOption> SpecificationAttributeOption { get; set; }
        public virtual DbSet<StateProvince> StateProvince { get; set; }
        public virtual DbSet<StockQuantityHistory> StockQuantityHistory { get; set; }
        public virtual DbSet<Store> Store { get; set; }
        public virtual DbSet<StoreMapping> StoreMapping { get; set; }
        public virtual DbSet<SuspendedCart> SuspendedCart { get; set; }
        public virtual DbSet<TaxCategory> TaxCategory { get; set; }
        public virtual DbSet<TierPrice> TierPrice { get; set; }
        public virtual DbSet<Topic> Topic { get; set; }
        public virtual DbSet<TopicTemplate> TopicTemplate { get; set; }
        public virtual DbSet<UrlRecord> UrlRecord { get; set; }
        public virtual DbSet<Vendor> Vendor { get; set; }
        public virtual DbSet<VendorAttribute> VendorAttribute { get; set; }
        public virtual DbSet<VendorAttributeValue> VendorAttributeValue { get; set; }
        public virtual DbSet<VendorNote> VendorNote { get; set; }
        public virtual DbSet<Warehouse> Warehouse { get; set; }
        public virtual DbSet<WishList> WishList { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=.;Database=Inovatiqa;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AclRecord>(entity =>
            {
                entity.HasIndex(e => e.CustomerRoleId);

                entity.HasIndex(e => new { e.EntityId, e.EntityName });

                entity.Property(e => e.EntityName)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.HasOne(d => d.CustomerRole)
                    .WithMany(p => p.AclRecord)
                    .HasForeignKey(d => d.CustomerRoleId)
                    .HasConstraintName("FK_AclRecord_CustomerRoleId_CustomerRole_Id");
            });

            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.HasIndex(e => e.ActivityLogTypeId);

                entity.HasIndex(e => e.CreatedOnUtc);

                entity.HasIndex(e => e.CustomerId);

                entity.Property(e => e.Comment).IsRequired();

                entity.Property(e => e.EntityName).HasMaxLength(400);

                entity.Property(e => e.IpAddress).HasMaxLength(200);

                entity.HasOne(d => d.ActivityLogType)
                    .WithMany(p => p.ActivityLog)
                    .HasForeignKey(d => d.ActivityLogTypeId)
                    .HasConstraintName("FK_ActivityLog_ActivityLogTypeId_ActivityLogType_Id");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.ActivityLog)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_ActivityLog_CustomerId_Customer_Id");
            });

            modelBuilder.Entity<ActivityLogType>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.SystemKeyword)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasIndex(e => e.CountryId);

                entity.HasIndex(e => e.StateProvinceId);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Address)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("FK_Address_CountryId_Country_Id");

                entity.HasOne(d => d.StateProvince)
                    .WithMany(p => p.Address)
                    .HasForeignKey(d => d.StateProvinceId)
                    .HasConstraintName("FK_Address_StateProvinceId_StateProvince_Id");
            });

            modelBuilder.Entity<AddressAttribute>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<AddressAttributeValue>(entity =>
            {
                entity.HasIndex(e => e.AddressAttributeId);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.HasOne(d => d.AddressAttribute)
                    .WithMany(p => p.AddressAttributeValue)
                    .HasForeignKey(d => d.AddressAttributeId)
                    .HasConstraintName("FK_AddressAttributeValue_AddressAttributeId_AddressAttribute_Id");
            });

            modelBuilder.Entity<Affiliate>(entity =>
            {
                entity.HasIndex(e => e.AddressId);

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.Affiliate)
                    .HasForeignKey(d => d.AddressId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Affiliate_AddressId_Address_Id");
            });

            modelBuilder.Entity<BackInStockSubscription>(entity =>
            {
                entity.HasIndex(e => e.CustomerId);

                entity.HasIndex(e => e.ProductId);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.BackInStockSubscription)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_BackInStockSubscription_CustomerId_Customer_Id");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.BackInStockSubscription)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_BackInStockSubscription_ProductId_Product_Id");
            });

            modelBuilder.Entity<BlogComment>(entity =>
            {
                entity.HasIndex(e => e.BlogPostId);

                entity.HasIndex(e => e.CustomerId);

                entity.HasIndex(e => e.StoreId);

                entity.HasOne(d => d.BlogPost)
                    .WithMany(p => p.BlogComment)
                    .HasForeignKey(d => d.BlogPostId)
                    .HasConstraintName("FK_BlogComment_BlogPostId_BlogPost_Id");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.BlogComment)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_BlogComment_CustomerId_Customer_Id");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.BlogComment)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_BlogComment_StoreId_Store_Id");
            });

            modelBuilder.Entity<BlogPost>(entity =>
            {
                entity.HasIndex(e => e.LanguageId);

                entity.Property(e => e.Body).IsRequired();

                entity.Property(e => e.MetaKeywords).HasMaxLength(400);

                entity.Property(e => e.MetaTitle).HasMaxLength(400);

                entity.Property(e => e.Title).IsRequired();

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.BlogPost)
                    .HasForeignKey(d => d.LanguageId)
                    .HasConstraintName("FK_BlogPost_LanguageId_Language_Id");
            });

            modelBuilder.Entity<Campaign>(entity =>
            {
                entity.Property(e => e.Body).IsRequired();

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.Subject).IsRequired();
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(e => e.DisplayOrder);

                entity.HasIndex(e => e.LimitedToStores);

                entity.HasIndex(e => e.ParentCategoryId);

                entity.HasIndex(e => e.SubjectToAcl);

                entity.HasIndex(e => new { e.Id, e.Name, e.SubjectToAcl, e.LimitedToStores, e.Published, e.Deleted })
                    .HasName("IX_Category_Deleted_Extended");

                entity.Property(e => e.MetaKeywords).HasMaxLength(400);

                entity.Property(e => e.MetaTitle).HasMaxLength(400);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.PageSizeOptions).HasMaxLength(200);

                entity.Property(e => e.PriceRanges).HasMaxLength(400);
            });

            modelBuilder.Entity<CategoryTemplate>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.ViewPath)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<CheckoutAttribute>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<CheckoutAttributeValue>(entity =>
            {
                entity.HasIndex(e => e.CheckoutAttributeId);

                entity.Property(e => e.ColorSquaresRgb).HasMaxLength(100);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.PriceAdjustment).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.WeightAdjustment).HasColumnType("decimal(18, 4)");

                entity.HasOne(d => d.CheckoutAttribute)
                    .WithMany(p => p.CheckoutAttributeValue)
                    .HasForeignKey(d => d.CheckoutAttributeId)
                    .HasConstraintName("FK_CheckoutAttributeValue_CheckoutAttributeId_CheckoutAttribute_Id");
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasIndex(e => e.DisplayOrder);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ThreeLetterIsoCode).HasMaxLength(3);

                entity.Property(e => e.TwoLetterIsoCode).HasMaxLength(2);
            });

            modelBuilder.Entity<Currency>(entity =>
            {
                entity.HasIndex(e => e.DisplayOrder);

                entity.Property(e => e.CurrencyCode)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.Property(e => e.CustomFormatting).HasMaxLength(50);

                entity.Property(e => e.DisplayLocale).HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Rate).HasColumnType("decimal(18, 4)");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => e.BillingAddressId);

                entity.HasIndex(e => e.CreatedOnUtc);

                entity.HasIndex(e => e.CustomerGuid);

                entity.HasIndex(e => e.Email);

                entity.HasIndex(e => e.ShippingAddressId);

                entity.HasIndex(e => e.SystemName);

                entity.HasIndex(e => e.Username);

                entity.Property(e => e.BillingAddressId).HasColumnName("BillingAddress_Id");

                entity.Property(e => e.CanPurchaseCart).HasDefaultValueSql("((0))");

                entity.Property(e => e.CreditLimit).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Email).HasMaxLength(1000);

                entity.Property(e => e.EmailToRevalidate).HasMaxLength(1000);

                entity.Property(e => e.MaxOrderApprovalValue).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.MaxOrderWithoutApproval).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ShippingAddressId).HasColumnName("ShippingAddress_Id");

                entity.Property(e => e.SystemName).HasMaxLength(400);

                entity.Property(e => e.Username).HasMaxLength(1000);

                entity.HasOne(d => d.BillingAddress)
                    .WithMany(p => p.CustomerBillingAddress)
                    .HasForeignKey(d => d.BillingAddressId)
                    .HasConstraintName("FK_Customer_BillingAddress_Id_Address_Id");

                entity.HasOne(d => d.ShippingAddress)
                    .WithMany(p => p.CustomerShippingAddress)
                    .HasForeignKey(d => d.ShippingAddressId)
                    .HasConstraintName("FK_Customer_ShippingAddress_Id_Address_Id");
            });

            modelBuilder.Entity<CustomerAddresses>(entity =>
            {
                entity.HasKey(e => new { e.AddressId, e.CustomerId });

                entity.HasIndex(e => e.AddressId);

                entity.HasIndex(e => e.CustomerId);

                entity.Property(e => e.AddressId).HasColumnName("Address_Id");

                entity.Property(e => e.CustomerId).HasColumnName("Customer_Id");

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.CustomerAddresses)
                    .HasForeignKey(d => d.AddressId)
                    .HasConstraintName("FK_CustomerAddresses_Address_Id_Address_Id");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerAddresses)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_CustomerAddresses_Customer_Id_Customer_Id");
            });

            modelBuilder.Entity<CustomerAttribute>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<CustomerAttributeValue>(entity =>
            {
                entity.HasIndex(e => e.CustomerAttributeId);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.HasOne(d => d.CustomerAttribute)
                    .WithMany(p => p.CustomerAttributeValue)
                    .HasForeignKey(d => d.CustomerAttributeId)
                    .HasConstraintName("FK_CustomerAttributeValue_CustomerAttributeId_CustomerAttribute_Id");
            });

            modelBuilder.Entity<CustomerCustomerRoleMapping>(entity =>
            {
                entity.HasKey(e => new { e.CustomerId, e.CustomerRoleId });

                entity.ToTable("Customer_CustomerRole_Mapping");

                entity.HasIndex(e => e.CustomerId);

                entity.HasIndex(e => e.CustomerRoleId);

                entity.Property(e => e.CustomerId).HasColumnName("Customer_Id");

                entity.Property(e => e.CustomerRoleId).HasColumnName("CustomerRole_Id");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerCustomerRoleMapping)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_Customer_CustomerRole_Mapping_Customer_Id_Customer_Id");

                entity.HasOne(d => d.CustomerRole)
                    .WithMany(p => p.CustomerCustomerRoleMapping)
                    .HasForeignKey(d => d.CustomerRoleId)
                    .HasConstraintName("FK_Customer_CustomerRole_Mapping_CustomerRole_Id_CustomerRole_Id");
            });

            modelBuilder.Entity<CustomerPassword>(entity =>
            {
                entity.HasIndex(e => e.CustomerId);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerPassword)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_CustomerPassword_CustomerId_Customer_Id");
            });

            modelBuilder.Entity<CustomerReturnRequest>(entity =>
            {
                entity.Property(e => e.CreatedDateUtc).HasColumnType("datetime");

                entity.Property(e => e.PrimaryContantEmail).HasMaxLength(50);

                entity.Property(e => e.SecondaryContactEmail).HasMaxLength(50);

                entity.Property(e => e.UpdatedDateUtc).HasColumnType("datetime");
            });

            modelBuilder.Entity<CustomerRole>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.SystemName).HasMaxLength(255);
            });

            modelBuilder.Entity<DeliveryDate>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<Discount>(entity =>
            {
                entity.Property(e => e.CouponCode).HasMaxLength(100);

                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DiscountPercentage).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.MaximumDiscountAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<DiscountAppliedToCategories>(entity =>
            {
                entity.HasKey(e => new { e.DiscountId, e.CategoryId });

                entity.ToTable("Discount_AppliedToCategories");

                entity.HasIndex(e => e.CategoryId);

                entity.HasIndex(e => e.DiscountId);

                entity.Property(e => e.DiscountId).HasColumnName("Discount_Id");

                entity.Property(e => e.CategoryId).HasColumnName("Category_Id");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.DiscountAppliedToCategories)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK_Discount_AppliedToCategories_Category_Id_Category_Id");

                entity.HasOne(d => d.Discount)
                    .WithMany(p => p.DiscountAppliedToCategories)
                    .HasForeignKey(d => d.DiscountId)
                    .HasConstraintName("FK_Discount_AppliedToCategories_Discount_Id_Discount_Id");
            });

            modelBuilder.Entity<DiscountAppliedToManufacturers>(entity =>
            {
                entity.HasKey(e => new { e.DiscountId, e.ManufacturerId });

                entity.ToTable("Discount_AppliedToManufacturers");

                entity.HasIndex(e => e.DiscountId);

                entity.HasIndex(e => e.ManufacturerId);

                entity.Property(e => e.DiscountId).HasColumnName("Discount_Id");

                entity.Property(e => e.ManufacturerId).HasColumnName("Manufacturer_Id");

                entity.HasOne(d => d.Discount)
                    .WithMany(p => p.DiscountAppliedToManufacturers)
                    .HasForeignKey(d => d.DiscountId)
                    .HasConstraintName("FK_Discount_AppliedToManufacturers_Discount_Id_Discount_Id");

                entity.HasOne(d => d.Manufacturer)
                    .WithMany(p => p.DiscountAppliedToManufacturers)
                    .HasForeignKey(d => d.ManufacturerId)
                    .HasConstraintName("FK_Discount_AppliedToManufacturers_Manufacturer_Id_Manufacturer_Id");
            });

            modelBuilder.Entity<DiscountAppliedToProducts>(entity =>
            {
                entity.HasKey(e => new { e.DiscountId, e.ProductId });

                entity.ToTable("Discount_AppliedToProducts");

                entity.HasIndex(e => e.DiscountId);

                entity.HasIndex(e => e.ProductId);

                entity.Property(e => e.DiscountId).HasColumnName("Discount_Id");

                entity.Property(e => e.ProductId).HasColumnName("Product_Id");

                entity.HasOne(d => d.Discount)
                    .WithMany(p => p.DiscountAppliedToProducts)
                    .HasForeignKey(d => d.DiscountId)
                    .HasConstraintName("FK_Discount_AppliedToProducts_Discount_Id_Discount_Id");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.DiscountAppliedToProducts)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_Discount_AppliedToProducts_Product_Id_Product_Id");
            });

            modelBuilder.Entity<DiscountRequirement>(entity =>
            {
                entity.HasIndex(e => e.DiscountId);

                entity.HasIndex(e => e.ParentId);

                entity.HasOne(d => d.Discount)
                    .WithMany(p => p.DiscountRequirement)
                    .HasForeignKey(d => d.DiscountId)
                    .HasConstraintName("FK_DiscountRequirement_DiscountId_Discount_Id");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_DiscountRequirement_ParentId_DiscountRequirement_Id");
            });

            modelBuilder.Entity<DiscountUsageHistory>(entity =>
            {
                entity.HasIndex(e => e.DiscountId);

                entity.HasIndex(e => e.OrderId);

                entity.HasOne(d => d.Discount)
                    .WithMany(p => p.DiscountUsageHistory)
                    .HasForeignKey(d => d.DiscountId)
                    .HasConstraintName("FK_DiscountUsageHistory_DiscountId_Discount_Id");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.DiscountUsageHistory)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_DiscountUsageHistory_OrderId_Order_Id");
            });

            modelBuilder.Entity<EmailAccount>(entity =>
            {
                entity.Property(e => e.DisplayName).HasMaxLength(255);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Host)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<EntityTierPrice>(entity =>
            {
                entity.ToTable("Entity_Tier_Price");

                entity.Property(e => e.EntityName).HasMaxLength(50);

                entity.Property(e => e.Rate).HasColumnType("decimal(18, 4)");
            });

            modelBuilder.Entity<ExternalAuthenticationRecord>(entity =>
            {
                entity.HasIndex(e => e.CustomerId);

                entity.Property(e => e.OauthAccessToken).HasColumnName("OAuthAccessToken");

                entity.Property(e => e.OauthToken).HasColumnName("OAuthToken");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.ExternalAuthenticationRecord)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_ExternalAuthenticationRecord_CustomerId_Customer_Id");
            });

            modelBuilder.Entity<ForumsForum>(entity =>
            {
                entity.ToTable("Forums_Forum");

                entity.HasIndex(e => e.DisplayOrder);

                entity.HasIndex(e => e.ForumGroupId);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasOne(d => d.ForumGroup)
                    .WithMany(p => p.ForumsForum)
                    .HasForeignKey(d => d.ForumGroupId)
                    .HasConstraintName("FK_Forums_Forum_ForumGroupId_Forums_Group_Id");
            });

            modelBuilder.Entity<ForumsGroup>(entity =>
            {
                entity.ToTable("Forums_Group");

                entity.HasIndex(e => e.DisplayOrder);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<ForumsPost>(entity =>
            {
                entity.ToTable("Forums_Post");

                entity.HasIndex(e => e.CustomerId);

                entity.HasIndex(e => e.TopicId);

                entity.Property(e => e.Ipaddress)
                    .HasColumnName("IPAddress")
                    .HasMaxLength(100);

                entity.Property(e => e.Text).IsRequired();

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.ForumsPost)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Forums_Post_CustomerId_Customer_Id");

                entity.HasOne(d => d.Topic)
                    .WithMany(p => p.ForumsPost)
                    .HasForeignKey(d => d.TopicId)
                    .HasConstraintName("FK_Forums_Post_TopicId_Forums_Topic_Id");
            });

            modelBuilder.Entity<ForumsPostVote>(entity =>
            {
                entity.ToTable("Forums_PostVote");

                entity.HasIndex(e => e.ForumPostId);

                entity.HasOne(d => d.ForumPost)
                    .WithMany(p => p.ForumsPostVote)
                    .HasForeignKey(d => d.ForumPostId)
                    .HasConstraintName("FK_Forums_PostVote_ForumPostId_Forums_Post_Id");
            });

            modelBuilder.Entity<ForumsPrivateMessage>(entity =>
            {
                entity.ToTable("Forums_PrivateMessage");

                entity.HasIndex(e => e.FromCustomerId);

                entity.HasIndex(e => e.ToCustomerId);

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.Text).IsRequired();

                entity.HasOne(d => d.FromCustomer)
                    .WithMany(p => p.ForumsPrivateMessageFromCustomer)
                    .HasForeignKey(d => d.FromCustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Forums_PrivateMessage_FromCustomerId_Customer_Id");

                entity.HasOne(d => d.ToCustomer)
                    .WithMany(p => p.ForumsPrivateMessageToCustomer)
                    .HasForeignKey(d => d.ToCustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Forums_PrivateMessage_ToCustomerId_Customer_Id");
            });

            modelBuilder.Entity<ForumsSubscription>(entity =>
            {
                entity.ToTable("Forums_Subscription");

                entity.HasIndex(e => e.CustomerId);

                entity.HasIndex(e => e.ForumId);

                entity.HasIndex(e => e.TopicId);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.ForumsSubscription)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Forums_Subscription_CustomerId_Customer_Id");
            });

            modelBuilder.Entity<ForumsTopic>(entity =>
            {
                entity.ToTable("Forums_Topic");

                entity.HasIndex(e => e.CustomerId);

                entity.HasIndex(e => e.ForumId);

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.ForumsTopic)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Forums_Topic_CustomerId_Customer_Id");

                entity.HasOne(d => d.Forum)
                    .WithMany(p => p.ForumsTopic)
                    .HasForeignKey(d => d.ForumId)
                    .HasConstraintName("FK_Forums_Topic_ForumId_Forums_Forum_Id");
            });

            modelBuilder.Entity<GdprConsent>(entity =>
            {
                entity.Property(e => e.Message).IsRequired();
            });

            modelBuilder.Entity<GenericAttribute>(entity =>
            {
                entity.HasIndex(e => new { e.EntityId, e.KeyGroup })
                    .HasName("IX_GenericAttribute_EntityId_and_KeyGroup");

                entity.Property(e => e.CreatedOrUpdatedDateUtc).HasColumnName("CreatedOrUpdatedDateUTC");

                entity.Property(e => e.Key)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.KeyGroup)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.Value).IsRequired();
            });

            modelBuilder.Entity<GiftCard>(entity =>
            {
                entity.HasIndex(e => e.PurchasedWithOrderItemId);

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");

                entity.HasOne(d => d.PurchasedWithOrderItem)
                    .WithMany(p => p.GiftCard)
                    .HasForeignKey(d => d.PurchasedWithOrderItemId)
                    .HasConstraintName("FK_GiftCard_PurchasedWithOrderItemId_OrderItem_Id");
            });

            modelBuilder.Entity<GiftCardUsageHistory>(entity =>
            {
                entity.HasIndex(e => e.GiftCardId);

                entity.HasIndex(e => e.UsedWithOrderId);

                entity.Property(e => e.UsedValue).HasColumnType("decimal(18, 4)");

                entity.HasOne(d => d.GiftCard)
                    .WithMany(p => p.GiftCardUsageHistory)
                    .HasForeignKey(d => d.GiftCardId)
                    .HasConstraintName("FK_GiftCardUsageHistory_GiftCardId_GiftCard_Id");

                entity.HasOne(d => d.UsedWithOrder)
                    .WithMany(p => p.GiftCardUsageHistory)
                    .HasForeignKey(d => d.UsedWithOrderId)
                    .HasConstraintName("FK_GiftCardUsageHistory_UsedWithOrderId_Order_Id");
            });

            modelBuilder.Entity<Language>(entity =>
            {
                entity.HasIndex(e => e.DisplayOrder);

                entity.Property(e => e.FlagImageFileName).HasMaxLength(50);

                entity.Property(e => e.LanguageCulture)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.UniqueSeoCode).HasMaxLength(2);
            });

            modelBuilder.Entity<LocaleStringResource>(entity =>
            {
                entity.HasIndex(e => e.LanguageId);

                entity.HasIndex(e => new { e.ResourceName, e.LanguageId })
                    .HasName("IX_LocaleStringResource");

                entity.Property(e => e.ResourceName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.ResourceValue).IsRequired();

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.LocaleStringResource)
                    .HasForeignKey(d => d.LanguageId)
                    .HasConstraintName("FK_LocaleStringResource_LanguageId_Language_Id");
            });

            modelBuilder.Entity<LocalizedProperty>(entity =>
            {
                entity.HasIndex(e => e.LanguageId);

                entity.Property(e => e.LocaleKey)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.LocaleKeyGroup)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.LocaleValue).IsRequired();

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.LocalizedProperty)
                    .HasForeignKey(d => d.LanguageId)
                    .HasConstraintName("FK_LocalizedProperty_LanguageId_Language_Id");
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.HasIndex(e => e.CreatedOnUtc);

                entity.HasIndex(e => e.CustomerId);

                entity.Property(e => e.IpAddress).HasMaxLength(200);

                entity.Property(e => e.ShortMessage).IsRequired();

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Log)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Log_CustomerId_Customer_Id");
            });

            modelBuilder.Entity<Manufacturer>(entity =>
            {
                entity.HasIndex(e => e.DisplayOrder);

                entity.HasIndex(e => e.LimitedToStores);

                entity.HasIndex(e => e.SubjectToAcl);

                entity.Property(e => e.MetaKeywords).HasMaxLength(400);

                entity.Property(e => e.MetaTitle).HasMaxLength(400);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.PageSizeOptions).HasMaxLength(200);

                entity.Property(e => e.PriceRanges).HasMaxLength(400);
            });

            modelBuilder.Entity<ManufacturerTemplate>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.ViewPath)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<MeasureDimension>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Ratio).HasColumnType("decimal(18, 8)");

                entity.Property(e => e.SystemKeyword)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<MeasureWeight>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Ratio).HasColumnType("decimal(18, 8)");

                entity.Property(e => e.SystemKeyword)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<MessageTemplate>(entity =>
            {
                entity.Property(e => e.BccEmailAddresses).HasMaxLength(200);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Subject).HasMaxLength(1000);
            });

            modelBuilder.Entity<MigrationVersionInfo>(entity =>
            {
                entity.HasNoKey();

                entity.HasIndex(e => e.Version)
                    .HasName("UC_Version")
                    .IsUnique()
                    .IsClustered();

                entity.Property(e => e.AppliedOn).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(1024);
            });

            modelBuilder.Entity<News>(entity =>
            {
                entity.HasIndex(e => e.LanguageId);

                entity.Property(e => e.Full).IsRequired();

                entity.Property(e => e.MetaKeywords).HasMaxLength(400);

                entity.Property(e => e.MetaTitle).HasMaxLength(400);

                entity.Property(e => e.Short).IsRequired();

                entity.Property(e => e.Title).IsRequired();

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.News)
                    .HasForeignKey(d => d.LanguageId)
                    .HasConstraintName("FK_News_LanguageId_Language_Id");
            });

            modelBuilder.Entity<NewsComment>(entity =>
            {
                entity.HasIndex(e => e.CustomerId);

                entity.HasIndex(e => e.NewsItemId);

                entity.HasIndex(e => e.StoreId);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.NewsComment)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_NewsComment_CustomerId_Customer_Id");

                entity.HasOne(d => d.NewsItem)
                    .WithMany(p => p.NewsComment)
                    .HasForeignKey(d => d.NewsItemId)
                    .HasConstraintName("FK_NewsComment_NewsItemId_News_Id");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.NewsComment)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_NewsComment_StoreId_Store_Id");
            });

            modelBuilder.Entity<NewsLetterSubscription>(entity =>
            {
                entity.HasIndex(e => new { e.Email, e.StoreId })
                    .HasName("IX_NewsletterSubscription_Email_StoreId");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(e => e.BillingAddressId);

                entity.HasIndex(e => e.CreatedOnUtc);

                entity.HasIndex(e => e.CustomerId);

                entity.HasIndex(e => e.PickupAddressId);

                entity.HasIndex(e => e.ShippingAddressId);

                entity.Property(e => e.CurrencyRate).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CustomOrderNumber).IsRequired();

                entity.Property(e => e.OrderDiscount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OrderShippingExclTax).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OrderShippingInclTax).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OrderSubTotalDiscountExclTax).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OrderSubTotalDiscountInclTax).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OrderSubtotalExclTax).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OrderSubtotalInclTax).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OrderTax).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OrderTotal).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaymentMethodAdditionalFeeExclTax).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaymentMethodAdditionalFeeInclTax).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RefundedAmount).HasColumnType("decimal(18, 4)");

                entity.HasOne(d => d.BillingAddress)
                    .WithMany(p => p.OrderBillingAddress)
                    .HasForeignKey(d => d.BillingAddressId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Order_BillingAddressId_Address_Id");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Order)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Order_CustomerId_Customer_Id");

                entity.HasOne(d => d.PickupAddress)
                    .WithMany(p => p.OrderPickupAddress)
                    .HasForeignKey(d => d.PickupAddressId)
                    .HasConstraintName("FK_Order_PickupAddressId_Address_Id");

                entity.HasOne(d => d.RewardPointsHistoryEntry)
                    .WithMany(p => p.Order)
                    .HasForeignKey(d => d.RewardPointsHistoryEntryId)
                    .HasConstraintName("FK_Order_RewardPointsHistoryEntryId_RewardPointsHistory_Id");

                entity.HasOne(d => d.ShippingAddress)
                    .WithMany(p => p.OrderShippingAddress)
                    .HasForeignKey(d => d.ShippingAddressId)
                    .HasConstraintName("FK_Order_ShippingAddressId_Address_Id");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasIndex(e => e.OrderId);

                entity.HasIndex(e => e.ProductId);

                entity.Property(e => e.DiscountAmountExclTax).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DiscountAmountInclTax).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ItemWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OriginalProductCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PriceExclTax).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PriceInclTax).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnitPriceExclTax).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UnitPriceInclTax).HasColumnType("decimal(18, 4)");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderItem)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_OrderItem_OrderId_Order_Id");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.OrderItem)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_OrderItem_ProductId_Product_Id");
            });

            modelBuilder.Entity<OrderNote>(entity =>
            {
                entity.HasIndex(e => e.OrderId);

                entity.Property(e => e.Note).IsRequired();

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderNote)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_OrderNote_OrderId_Order_Id");
            });

            modelBuilder.Entity<PermissionRecord>(entity =>
            {
                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.SystemName)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<PermissionRecordRoleMapping>(entity =>
            {
                entity.HasKey(e => new { e.PermissionRecordId, e.CustomerRoleId });

                entity.ToTable("PermissionRecord_Role_Mapping");

                entity.HasIndex(e => e.CustomerRoleId);

                entity.HasIndex(e => e.PermissionRecordId);

                entity.Property(e => e.PermissionRecordId).HasColumnName("PermissionRecord_Id");

                entity.Property(e => e.CustomerRoleId).HasColumnName("CustomerRole_Id");

                entity.HasOne(d => d.CustomerRole)
                    .WithMany(p => p.PermissionRecordRoleMapping)
                    .HasForeignKey(d => d.CustomerRoleId)
                    .HasConstraintName("FK_PermissionRecord_Role_Mapping_CustomerRole_Id_CustomerRole_Id");

                entity.HasOne(d => d.PermissionRecord)
                    .WithMany(p => p.PermissionRecordRoleMapping)
                    .HasForeignKey(d => d.PermissionRecordId)
                    .HasConstraintName("FK_PermissionRecord_Role_Mapping_PermissionRecord_Id_PermissionRecord_Id");
            });

            modelBuilder.Entity<Picture>(entity =>
            {
                entity.Property(e => e.MimeType)
                    .IsRequired()
                    .HasMaxLength(40);

                entity.Property(e => e.SeoFilename).HasMaxLength(800);
            });

            modelBuilder.Entity<PictureBinary>(entity =>
            {
                entity.HasIndex(e => e.PictureId);

                entity.HasOne(d => d.Picture)
                    .WithMany(p => p.PictureBinary)
                    .HasForeignKey(d => d.PictureId)
                    .HasConstraintName("FK_PictureBinary_PictureId_Picture_Id");
            });

            modelBuilder.Entity<Poll>(entity =>
            {
                entity.HasIndex(e => e.LanguageId);

                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.Poll)
                    .HasForeignKey(d => d.LanguageId)
                    .HasConstraintName("FK_Poll_LanguageId_Language_Id");
            });

            modelBuilder.Entity<PollAnswer>(entity =>
            {
                entity.HasIndex(e => e.PollId);

                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(d => d.Poll)
                    .WithMany(p => p.PollAnswer)
                    .HasForeignKey(d => d.PollId)
                    .HasConstraintName("FK_PollAnswer_PollId_Poll_Id");
            });

            modelBuilder.Entity<PollVotingRecord>(entity =>
            {
                entity.HasIndex(e => e.CustomerId);

                entity.HasIndex(e => e.PollAnswerId);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.PollVotingRecord)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_PollVotingRecord_CustomerId_Customer_Id");

                entity.HasOne(d => d.PollAnswer)
                    .WithMany(p => p.PollVotingRecord)
                    .HasForeignKey(d => d.PollAnswerId)
                    .HasConstraintName("FK_PollVotingRecord_PollAnswerId_PollAnswer_Id");
            });

            modelBuilder.Entity<PredefinedProductAttributeValue>(entity =>
            {
                entity.HasIndex(e => e.ProductAttributeId);

                entity.Property(e => e.Cost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.PriceAdjustment).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.WeightAdjustment).HasColumnType("decimal(18, 4)");

                entity.HasOne(d => d.ProductAttribute)
                    .WithMany(p => p.PredefinedProductAttributeValue)
                    .HasForeignKey(d => d.ProductAttributeId)
                    .HasConstraintName("FK_PredefinedProductAttributeValue_ProductAttributeId_ProductAttribute_Id");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(e => e.LimitedToStores);

                entity.HasIndex(e => e.ParentGroupedProductId);

                entity.HasIndex(e => e.Published);

                entity.HasIndex(e => e.ShowOnHomepage);

                entity.HasIndex(e => e.SubjectToAcl);

                entity.HasIndex(e => e.VisibleIndividually);

                entity.HasIndex(e => new { e.Deleted, e.Id })
                    .HasName("IX_Product_Delete_Id");

                entity.HasIndex(e => new { e.Published, e.Deleted })
                    .HasName("IX_Product_Deleted_and_Published");

                entity.HasIndex(e => new { e.Price, e.AvailableStartDateTimeUtc, e.AvailableEndDateTimeUtc, e.Published, e.Deleted })
                    .HasName("IX_Product_PriceDatesEtc");

                entity.HasIndex(e => new { e.Deleted, e.VendorId, e.ProductTypeId, e.ManageInventoryMethodId, e.MinStockQuantity, e.UseMultipleWarehouses })
                    .HasName("IX_GetLowStockProducts");

                entity.HasIndex(e => new { e.Id, e.AvailableStartDateTimeUtc, e.AvailableEndDateTimeUtc, e.VisibleIndividually, e.Published, e.Deleted })
                    .HasName("IX_Product_VisibleIndividually_Published_Deleted_Extended");

                entity.Property(e => e.AdditionalShippingCharge).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.AllowedQuantities).HasMaxLength(1000);

                entity.Property(e => e.BasepriceAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.BasepriceBaseAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Gtin).HasMaxLength(400);

                entity.Property(e => e.Height).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.IsCovid19Product)
                    .IsRequired()
                    .HasDefaultValueSql("('false')");

                entity.Property(e => e.Length).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ManufacturerPartNumber).HasMaxLength(400);

                entity.Property(e => e.MaximumCustomerEnteredPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.MetaKeywords).HasMaxLength(400);

                entity.Property(e => e.MetaTitle).HasMaxLength(400);

                entity.Property(e => e.MinimumCustomerEnteredPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Msku).HasColumnName("MSku");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(800);

                entity.Property(e => e.OldPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OverriddenGiftCardAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ProductCost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.RequiredProductIds).HasMaxLength(1000);

                entity.Property(e => e.ShowCovid19OnHomepage)
                    .IsRequired()
                    .HasDefaultValueSql("('false')");

                entity.Property(e => e.Sku).HasMaxLength(400);

                entity.Property(e => e.Weight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Width).HasColumnType("decimal(18, 4)");
            });

            modelBuilder.Entity<ProductAttribute>(entity =>
            {
                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<ProductAttributeCombination>(entity =>
            {
                entity.HasIndex(e => e.ProductId);

                entity.Property(e => e.Gtin).HasMaxLength(400);

                entity.Property(e => e.ManufacturerPartNumber).HasMaxLength(400);

                entity.Property(e => e.OverriddenPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Sku).HasMaxLength(400);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductAttributeCombination)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_ProductAttributeCombination_ProductId_Product_Id");
            });

            modelBuilder.Entity<ProductAttributeValue>(entity =>
            {
                entity.HasIndex(e => e.ProductAttributeMappingId);

                entity.HasIndex(e => new { e.ProductAttributeMappingId, e.DisplayOrder });

                entity.Property(e => e.ColorSquaresRgb).HasMaxLength(100);

                entity.Property(e => e.Cost).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Msku).HasColumnName("MSku");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.PriceAdjustment).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.WeightAdjustment).HasColumnType("decimal(18, 4)");

                entity.HasOne(d => d.ProductAttributeMapping)
                    .WithMany(p => p.ProductAttributeValue)
                    .HasForeignKey(d => d.ProductAttributeMappingId)
                    .HasConstraintName("FK_ProductAttributeValue_ProductAttributeMappingId_Product_ProductAttribute_Mapping_Id");
            });

            modelBuilder.Entity<ProductAvailabilityRange>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<ProductCategoryMapping>(entity =>
            {
                entity.ToTable("Product_Category_Mapping");

                entity.HasIndex(e => e.CategoryId);

                entity.HasIndex(e => e.IsFeaturedProduct);

                entity.HasIndex(e => e.ProductId);

                entity.HasIndex(e => new { e.CategoryId, e.ProductId })
                    .HasName("IX_PCM_Product_and_Category");

                entity.HasIndex(e => new { e.CategoryId, e.ProductId, e.IsFeaturedProduct })
                    .HasName("IX_PCM_ProductId_Extended");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.ProductCategoryMapping)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK_Product_Category_Mapping_CategoryId_Category_Id");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductCategoryMapping)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_Product_Category_Mapping_ProductId_Product_Id");
            });

            modelBuilder.Entity<ProductManufacturerMapping>(entity =>
            {
                entity.ToTable("Product_Manufacturer_Mapping");

                entity.HasIndex(e => e.IsFeaturedProduct);

                entity.HasIndex(e => e.ManufacturerId);

                entity.HasIndex(e => e.ProductId);

                entity.HasIndex(e => new { e.ManufacturerId, e.ProductId })
                    .HasName("IX_PMM_Product_and_Manufacturer");

                entity.HasIndex(e => new { e.ManufacturerId, e.ProductId, e.IsFeaturedProduct })
                    .HasName("IX_PMM_ProductId_Extended");

                entity.HasOne(d => d.Manufacturer)
                    .WithMany(p => p.ProductManufacturerMapping)
                    .HasForeignKey(d => d.ManufacturerId)
                    .HasConstraintName("FK_Product_Manufacturer_Mapping_ManufacturerId_Manufacturer_Id");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductManufacturerMapping)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_Product_Manufacturer_Mapping_ProductId_Product_Id");
            });

            modelBuilder.Entity<ProductPictureMapping>(entity =>
            {
                entity.ToTable("Product_Picture_Mapping");

                entity.HasIndex(e => e.PictureId);

                entity.HasIndex(e => e.ProductId);

                entity.HasOne(d => d.Picture)
                    .WithMany(p => p.ProductPictureMapping)
                    .HasForeignKey(d => d.PictureId)
                    .HasConstraintName("FK_Product_Picture_Mapping_PictureId_Picture_Id");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductPictureMapping)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_Product_Picture_Mapping_ProductId_Product_Id");
            });

            modelBuilder.Entity<ProductProductAttributeMapping>(entity =>
            {
                entity.ToTable("Product_ProductAttribute_Mapping");

                entity.HasIndex(e => e.ProductAttributeId);

                entity.HasIndex(e => e.ProductId);

                entity.HasIndex(e => new { e.ProductId, e.DisplayOrder });

                entity.HasOne(d => d.ProductAttribute)
                    .WithMany(p => p.ProductProductAttributeMapping)
                    .HasForeignKey(d => d.ProductAttributeId)
                    .HasConstraintName("FK_Product_ProductAttribute_Mapping_ProductAttributeId_ProductAttribute_Id");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductProductAttributeMapping)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_Product_ProductAttribute_Mapping_ProductId_Product_Id");
            });

            modelBuilder.Entity<ProductProductTagMapping>(entity =>
            {
                entity.HasKey(e => new { e.ProductId, e.ProductTagId });

                entity.ToTable("Product_ProductTag_Mapping");

                entity.HasIndex(e => e.ProductId);

                entity.HasIndex(e => e.ProductTagId);

                entity.Property(e => e.ProductId).HasColumnName("Product_Id");

                entity.Property(e => e.ProductTagId).HasColumnName("ProductTag_Id");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductProductTagMapping)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_Product_ProductTag_Mapping_Product_Id_Product_Id");

                entity.HasOne(d => d.ProductTag)
                    .WithMany(p => p.ProductProductTagMapping)
                    .HasForeignKey(d => d.ProductTagId)
                    .HasConstraintName("FK_Product_ProductTag_Mapping_ProductTag_Id_ProductTag_Id");
            });

            modelBuilder.Entity<ProductReview>(entity =>
            {
                entity.HasIndex(e => e.CustomerId);

                entity.HasIndex(e => e.ProductId);

                entity.HasIndex(e => e.StoreId);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.ProductReview)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_ProductReview_CustomerId_Customer_Id");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductReview)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_ProductReview_ProductId_Product_Id");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.ProductReview)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_ProductReview_StoreId_Store_Id");
            });

            modelBuilder.Entity<ProductReviewHelpfulness>(entity =>
            {
                entity.HasIndex(e => e.ProductReviewId);

                entity.HasOne(d => d.ProductReview)
                    .WithMany(p => p.ProductReviewHelpfulness)
                    .HasForeignKey(d => d.ProductReviewId)
                    .HasConstraintName("FK_ProductReviewHelpfulness_ProductReviewId_ProductReview_Id");
            });

            modelBuilder.Entity<ProductReviewReviewTypeMapping>(entity =>
            {
                entity.ToTable("ProductReview_ReviewType_Mapping");

                entity.HasIndex(e => e.ProductReviewId);

                entity.HasIndex(e => e.ReviewTypeId);

                entity.HasOne(d => d.ProductReview)
                    .WithMany(p => p.ProductReviewReviewTypeMapping)
                    .HasForeignKey(d => d.ProductReviewId)
                    .HasConstraintName("FK_ProductReview_ReviewType_Mapping_ProductReviewId_ProductReview_Id");

                entity.HasOne(d => d.ReviewType)
                    .WithMany(p => p.ProductReviewReviewTypeMapping)
                    .HasForeignKey(d => d.ReviewTypeId)
                    .HasConstraintName("FK_ProductReview_ReviewType_Mapping_ReviewTypeId_ReviewType_Id");
            });

            modelBuilder.Entity<ProductSpecificationAttributeMapping>(entity =>
            {
                entity.ToTable("Product_SpecificationAttribute_Mapping");

                entity.HasIndex(e => e.ProductId);

                entity.HasIndex(e => e.SpecificationAttributeOptionId);

                entity.HasIndex(e => new { e.ProductId, e.SpecificationAttributeOptionId, e.AllowFiltering })
                    .HasName("IX_PSAM_SpecificationAttributeOptionId_AllowFiltering");

                entity.Property(e => e.CustomValue).HasMaxLength(4000);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductSpecificationAttributeMapping)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_Product_SpecificationAttribute_Mapping_ProductId_Product_Id");

                entity.HasOne(d => d.SpecificationAttributeOption)
                    .WithMany(p => p.ProductSpecificationAttributeMapping)
                    .HasForeignKey(d => d.SpecificationAttributeOptionId)
                    .HasConstraintName("FK_Product_SpecificationAttribute_Mapping_SpecificationAttributeOptionId_SpecificationAttributeOption_Id");
            });

            modelBuilder.Entity<ProductTag>(entity =>
            {
                entity.HasIndex(e => e.Name);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<ProductTemplate>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.ViewPath)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<ProductWarehouseInventory>(entity =>
            {
                entity.HasIndex(e => e.ProductId);

                entity.HasIndex(e => e.WarehouseId);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductWarehouseInventory)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_ProductWarehouseInventory_ProductId_Product_Id");

                entity.HasOne(d => d.Warehouse)
                    .WithMany(p => p.ProductWarehouseInventory)
                    .HasForeignKey(d => d.WarehouseId)
                    .HasConstraintName("FK_ProductWarehouseInventory_WarehouseId_Warehouse_Id");
            });

            modelBuilder.Entity<QueuedEmail>(entity =>
            {
                entity.HasIndex(e => e.CreatedOnUtc);

                entity.HasIndex(e => e.EmailAccountId);

                entity.HasIndex(e => new { e.SentTries, e.SentOnUtc, e.DontSendBeforeDateUtc })
                    .HasName("IX_QueuedEmail_SentOnUtc_DontSendBeforeDateUtc_Extended");

                entity.Property(e => e.Bcc).HasMaxLength(500);

                entity.Property(e => e.Cc)
                    .HasColumnName("CC")
                    .HasMaxLength(500);

                entity.Property(e => e.From)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.FromName).HasMaxLength(500);

                entity.Property(e => e.ReplyTo).HasMaxLength(500);

                entity.Property(e => e.ReplyToName).HasMaxLength(500);

                entity.Property(e => e.Subject).HasMaxLength(1000);

                entity.Property(e => e.To)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.ToName).HasMaxLength(500);

                entity.HasOne(d => d.EmailAccount)
                    .WithMany(p => p.QueuedEmail)
                    .HasForeignKey(d => d.EmailAccountId)
                    .HasConstraintName("FK_QueuedEmail_EmailAccountId_EmailAccount_Id");
            });

            modelBuilder.Entity<RecurringPayment>(entity =>
            {
                entity.HasIndex(e => e.InitialOrderId);

                entity.HasOne(d => d.InitialOrder)
                    .WithMany(p => p.RecurringPayment)
                    .HasForeignKey(d => d.InitialOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RecurringPayment_InitialOrderId_Order_Id");
            });

            modelBuilder.Entity<RecurringPaymentHistory>(entity =>
            {
                entity.HasIndex(e => e.RecurringPaymentId);

                entity.HasOne(d => d.RecurringPayment)
                    .WithMany(p => p.RecurringPaymentHistory)
                    .HasForeignKey(d => d.RecurringPaymentId)
                    .HasConstraintName("FK_RecurringPaymentHistory_RecurringPaymentId_RecurringPayment_Id");
            });

            modelBuilder.Entity<RelatedProduct>(entity =>
            {
                entity.HasIndex(e => e.ProductId1);
            });

            modelBuilder.Entity<ReturnRequest>(entity =>
            {
                entity.HasIndex(e => e.CustomerId);

                entity.Property(e => e.ReasonForReturn).IsRequired();

                entity.Property(e => e.RequestedAction).IsRequired();

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.ReturnRequest)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_ReturnRequest_CustomerId_Customer_Id");
            });

            modelBuilder.Entity<ReturnRequestAction>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<ReturnRequestReason>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<ReviewType>(entity =>
            {
                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<RewardPointsHistory>(entity =>
            {
                entity.HasIndex(e => e.CustomerId);

                entity.Property(e => e.UsedAmount).HasColumnType("decimal(18, 4)");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.RewardPointsHistory)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_RewardPointsHistory_CustomerId_Customer_Id");
            });

            modelBuilder.Entity<ScheduleTask>(entity =>
            {
                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.Type).IsRequired();
            });

            modelBuilder.Entity<ScrappingLastRecord>(entity =>
            {
                entity.ToTable("Scrapping_LastRecord");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.LastUrl)
                    .HasColumnName("LastURL")
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Setting>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Value).IsRequired();
            });

            modelBuilder.Entity<Shipment>(entity =>
            {
                entity.HasIndex(e => e.OrderId);

                entity.Property(e => e.AmountPaid).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalWeight).HasColumnType("decimal(18, 4)");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Shipment)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_Shipment_OrderId_Order_Id");
            });

            modelBuilder.Entity<ShipmentItem>(entity =>
            {
                entity.HasIndex(e => e.ShipmentId);

                entity.HasOne(d => d.Shipment)
                    .WithMany(p => p.ShipmentItem)
                    .HasForeignKey(d => d.ShipmentId)
                    .HasConstraintName("FK_ShipmentItem_ShipmentId_Shipment_Id");
            });

            modelBuilder.Entity<ShippingMethod>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<ShippingMethodRestrictions>(entity =>
            {
                entity.HasKey(e => new { e.ShippingMethodId, e.CountryId });

                entity.HasIndex(e => e.CountryId);

                entity.HasIndex(e => e.ShippingMethodId);

                entity.Property(e => e.ShippingMethodId).HasColumnName("ShippingMethod_Id");

                entity.Property(e => e.CountryId).HasColumnName("Country_Id");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.ShippingMethodRestrictions)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("FK_ShippingMethodRestrictions_Country_Id_Country_Id");

                entity.HasOne(d => d.ShippingMethod)
                    .WithMany(p => p.ShippingMethodRestrictions)
                    .HasForeignKey(d => d.ShippingMethodId)
                    .HasConstraintName("FK_ShippingMethodRestrictions_ShippingMethod_Id_ShippingMethod_Id");
            });

            modelBuilder.Entity<ShoppingCartItem>(entity =>
            {
                entity.HasIndex(e => e.CustomerId);

                entity.HasIndex(e => e.ProductId);

                entity.HasIndex(e => new { e.ShoppingCartTypeId, e.CustomerId });

                entity.Property(e => e.CustomerEnteredPrice).HasColumnType("decimal(18, 4)");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.ShoppingCartItem)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_ShoppingCartItem_CustomerId_Customer_Id");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ShoppingCartItem)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_ShoppingCartItem_ProductId_Product_Id");
            });

            modelBuilder.Entity<SpecificationAttribute>(entity =>
            {
                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<SpecificationAttributeOption>(entity =>
            {
                entity.HasIndex(e => e.SpecificationAttributeId);

                entity.Property(e => e.ColorSquaresRgb).HasMaxLength(100);

                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(d => d.SpecificationAttribute)
                    .WithMany(p => p.SpecificationAttributeOption)
                    .HasForeignKey(d => d.SpecificationAttributeId)
                    .HasConstraintName("FK_SpecificationAttributeOption_SpecificationAttributeId_SpecificationAttribute_Id");
            });

            modelBuilder.Entity<StateProvince>(entity =>
            {
                entity.HasIndex(e => e.CountryId);

                entity.Property(e => e.Abbreviation).HasMaxLength(100);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.StateProvince)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("FK_StateProvince_CountryId_Country_Id");
            });

            modelBuilder.Entity<StockQuantityHistory>(entity =>
            {
                entity.HasIndex(e => e.ProductId);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.StockQuantityHistory)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_StockQuantityHistory_ProductId_Product_Id");
            });

            modelBuilder.Entity<Store>(entity =>
            {
                entity.Property(e => e.CompanyAddress).HasMaxLength(1000);

                entity.Property(e => e.CompanyName).HasMaxLength(1000);

                entity.Property(e => e.CompanyPhoneNumber).HasMaxLength(1000);

                entity.Property(e => e.CompanyVat).HasMaxLength(1000);

                entity.Property(e => e.Hosts).HasMaxLength(1000);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<StoreMapping>(entity =>
            {
                entity.HasIndex(e => e.StoreId);

                entity.HasIndex(e => new { e.EntityId, e.EntityName });

                entity.Property(e => e.EntityName)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.StoreMapping)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_StoreMapping_StoreId_Store_Id");
            });

            modelBuilder.Entity<SuspendedCart>(entity =>
            {
                entity.Property(e => e.Comment).HasMaxLength(500);

                entity.Property(e => e.EmailAddress).HasMaxLength(50);

                entity.Property(e => e.Poname).HasColumnName("POName");
            });

            modelBuilder.Entity<TaxCategory>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<TierPrice>(entity =>
            {
                entity.HasIndex(e => e.CustomerRoleId);

                entity.HasIndex(e => e.ProductId);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");

                entity.HasOne(d => d.CustomerRole)
                    .WithMany(p => p.TierPrice)
                    .HasForeignKey(d => d.CustomerRoleId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_TierPrice_CustomerRoleId_CustomerRole_Id");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.TierPrice)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_TierPrice_ProductId_Product_Id");
            });

            modelBuilder.Entity<TopicTemplate>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.ViewPath)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<UrlRecord>(entity =>
            {
                entity.HasIndex(e => e.Slug);

                entity.HasIndex(e => new { e.EntityId, e.EntityName, e.LanguageId, e.IsActive })
                    .HasName("IX_UrlRecord_Custom_1");

                entity.Property(e => e.EntityName)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasMaxLength(800);
            });

            modelBuilder.Entity<Vendor>(entity =>
            {
                entity.Property(e => e.Email).HasMaxLength(400);

                entity.Property(e => e.MetaKeywords).HasMaxLength(400);

                entity.Property(e => e.MetaTitle).HasMaxLength(400);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.PageSizeOptions).HasMaxLength(200);
            });

            modelBuilder.Entity<VendorAttribute>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<VendorAttributeValue>(entity =>
            {
                entity.HasIndex(e => e.VendorAttributeId);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.HasOne(d => d.VendorAttribute)
                    .WithMany(p => p.VendorAttributeValue)
                    .HasForeignKey(d => d.VendorAttributeId)
                    .HasConstraintName("FK_VendorAttributeValue_VendorAttributeId_VendorAttribute_Id");
            });

            modelBuilder.Entity<VendorNote>(entity =>
            {
                entity.HasIndex(e => e.VendorId);

                entity.Property(e => e.Note).IsRequired();

                entity.HasOne(d => d.Vendor)
                    .WithMany(p => p.VendorNote)
                    .HasForeignKey(d => d.VendorId)
                    .HasConstraintName("FK_VendorNote_VendorId_Vendor_Id");
            });

            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<WishList>(entity =>
            {
                entity.Property(e => e.ListName)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
