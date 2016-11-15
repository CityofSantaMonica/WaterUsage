using CSM.WaterUsage.Customers.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSM.WaterUsage.Customers
{
    public interface ICustomersSerivce
    {
        IAccount GetAccount(IUsageRecord usage);
        IAccountService GetAccountService(IUsageRecord usage);
        IUsageCategory GetUsageCategory(IAccountService service);

        IEnumerable<IUsageRecord> GetUsageRecords();
        IEnumerable<IUsageRecord> GetUsageRecords(DateTime afterDate);
    }

    public class CustomersService : ICustomersSerivce
    {
        private readonly CustomerEntities entities;

        private IDictionary<string, IAccount> accounts;
        private IDictionary<string, IAccountService> services;
        private IDictionary<string, IUsageCategory> categories;

        public CustomersService()
        {
            entities = new CustomerEntities();
            entities.Database.CommandTimeout = 300;

            accounts = entities.Accounts.ToDictionary(a => KeyMaker.ForAccount(a), a => a as IAccount);
            services = entities.AccountsServices.ToDictionary(s => KeyMaker.ForService(s), s => s as IAccountService);
            categories = entities.UsageCategories.ToDictionary(c => KeyMaker.ForCategory(c), c => c as IUsageCategory);
        }

        public IAccount GetAccount(IUsageRecord usage)
        {
            return accounts.ContainsKey(KeyMaker.ForAccount(usage)) ? accounts[KeyMaker.ForAccount(usage)] : new Account();
        }

        public IAccountService GetAccountService(IUsageRecord usage)
        {
            return services.ContainsKey(KeyMaker.ForService(usage)) ? services[KeyMaker.ForService(usage)] : new AccountService();
        }

        public IUsageCategory GetUsageCategory(IAccountService service)
        {
            var category = categories.ContainsKey(KeyMaker.ForCategory(service)) ? categories[KeyMaker.ForCategory(service)] : new UsageCategory();

            category.code = KeyMaker.ForCategory(service);

            return category;
        }

        public IEnumerable<IUsageRecord> GetUsageRecords()
        {
            return entities.UsageRecords.OrderByDescending(w => w.prorate_to);
        }

        public IEnumerable<IUsageRecord> GetUsageRecords(DateTime afterDate)
        {
            return GetUsageRecords().Where(w => w.prorate_to >= afterDate);
        }
    }
}
