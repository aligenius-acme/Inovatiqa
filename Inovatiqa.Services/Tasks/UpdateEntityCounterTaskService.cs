using Inovatiqa.Database.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inovatiqa.Services.Tasks
{
    public class UpdateEntityCounterTaskService : BackgroundTaskService
    {
        #region Fields
        private readonly IServiceScopeFactory _serviceScopeFactory;
        #endregion

        #region Ctor
        public UpdateEntityCounterTaskService(
            IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        #endregion

        #region Utilities
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var contextService = scope.ServiceProvider.GetRequiredService<Inovatiqa.Database.DbContexts.InovatiqaContext>();
                    var categories = contextService.Category.ToList();
                    var productCategoryMappings = contextService.ProductCategoryMapping.ToList();
                    UpdateCategoryCount(categories, productCategoryMappings, contextService);

                    var manufacturers = contextService.Manufacturer.ToList();
                    var productManufacturerMappings = contextService.ProductManufacturerMapping.ToList();
                    UpdateManufacturerCount(manufacturers, productManufacturerMappings, contextService);
                }
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken); // Time to pause this background logic
            }
        }
        #endregion

        #region Methods
        private void UpdateCategoryCount(IList<Category> categories, IList<ProductCategoryMapping> productCategoryMapping, Database.DbContexts.InovatiqaContext contextService)
        {
            foreach(var category in categories)
            {
                var currentProductCount = productCategoryMapping.Where(x => x.CategoryId == category.Id).Count();
                if (category.ProductCount != currentProductCount)
                {
                    category.ProductCount = currentProductCount;
                }
            }

            contextService.SaveChanges();
        }

        private void UpdateManufacturerCount(IList<Manufacturer> manufacturers, IList<ProductManufacturerMapping> productManufacturerMappings, Database.DbContexts.InovatiqaContext contextService)
        {
            foreach (var manufacturer in manufacturers)
            {
                var currentProductCount = productManufacturerMappings.Where(x => x.ManufacturerId == manufacturer.Id).Count();
                if (manufacturer.ProductCount != currentProductCount)
                {
                    manufacturer.ProductCount = currentProductCount;
                }
            }

            contextService.SaveChanges();
        }
        #endregion
    }
}
