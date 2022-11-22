using Inovatiqa.Database.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inovatiqa.Services.Tasks
{
    public class UpdateRootCategoryIdsForProductsTaskService:BackgroundTaskService
    {
        #region Fields
        private readonly IServiceScopeFactory _serviceScopeFactory;
        #endregion

        #region Ctor
        public UpdateRootCategoryIdsForProductsTaskService(
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
                    var products = contextService.Product.Where(x=>x.RootCategoryId == null).ToList();
                    var rootCategoryIds = contextService.Category.Where(x=>x.ParentCategoryId == 0).Select(y=>y.Id).ToList();
                    UpdateRootCategoryIds(products, contextService, rootCategoryIds);
                }
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken); // Time to pause this background logic
            }
        }
        #endregion

        #region Methods
        private void UpdateRootCategoryIds(IList<Product> products, Database.DbContexts.InovatiqaContext contextService, IList<int> rootCategoryIds)
        {
            var productCategoryMappings = contextService.ProductCategoryMapping.Where(x => rootCategoryIds.Contains(x.CategoryId)).ToList();
            foreach (var product in products)
            {
                product.RootCategoryId = productCategoryMappings.Where(x => x.ProductId == product.Id).Select(y => y.CategoryId).FirstOrDefault();
            }

            contextService.SaveChanges();
        }
        #endregion
    }
}
