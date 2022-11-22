using Inovatiqa.Database.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inovatiqa.Services.Tasks
{
    public class UpdateRolesForEachCategoryTaskService : BackgroundTaskService
    {
        #region Fields
        private readonly IServiceScopeFactory _serviceScopeFactory;
        #endregion

        #region Ctor
        public UpdateRolesForEachCategoryTaskService(
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
                    var rootCategories = contextService.Category.Where(x=>x.ParentCategoryId == 0).ToList();
                    var roles = contextService.CustomerRole.ToList();
                    UpdateRolesForEachCategory(contextService, rootCategories, roles);
                }
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken); // Time to pause this background logic
            }
        }
        #endregion

        #region Methods
        private void UpdateRolesForEachCategory(Database.DbContexts.InovatiqaContext contextService, IList<Category> rootCategories, IList<CustomerRole> roles)
        {
            List<string> defaultRoles = new List<string>();
            defaultRoles.Add("Retail");
            defaultRoles.Add("Bronze");
            defaultRoles.Add("Bronze Premier");
            defaultRoles.Add("Gold");
            defaultRoles.Add("Gold Premier");
            defaultRoles.Add("Onyx");
            defaultRoles.Add("Onyx Premier");
            defaultRoles.Add("Diamond");
            defaultRoles.Add("Diamond Premier");
            defaultRoles.Add("Distributor");
            defaultRoles.Add("Distributor Premier");
            foreach (var rootCategory in rootCategories)
            {
                foreach (var defaultRole in defaultRoles)
                {
                    var categoryRole = roles.Where(x => x.SystemName == rootCategory.Name + "_" + defaultRole + "_" + rootCategory.Id).FirstOrDefault();
                    if (categoryRole == null)
                    {
                        CustomerRole role = new CustomerRole
                        {
                            Name = rootCategory.Name + "_" + defaultRole + "_" + rootCategory.Id,
                            SystemName = rootCategory.Name + "_" + defaultRole + "_" + rootCategory.Id,
                            FreeShipping = false,
                            TaxExempt = false,
                            Active = true,
                            IsSystemRole = true,
                            EnablePasswordLifetime = false,
                            OverrideTaxDisplayType = false,
                            DefaultTaxDisplayTypeId = 0,
                            PurchasedWithProductId = 0
                        };
                        contextService.CustomerRole.Add(role);
                    }
                }
            }
            contextService.SaveChanges();
        }
        #endregion
    }
}
