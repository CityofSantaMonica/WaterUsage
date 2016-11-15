using System;

namespace CSM.WaterUsage.Customers
{
    class KeyMaker
    {
        public static string ForAccount(IAccount account)
        {
            if (account == null)
                return String.Empty;

            return forAccount(account.account_number, account.occupant_code);
        }

        public static string ForAccount(IUsageRecord usage)
        {
            if (usage == null)
                return String.Empty;

            return forAccount(usage.account_number, usage.occupant_code);
        }

        public static string ForCategory(IUsageCategory category)
        {
            if (category == null)
                return String.Empty;

            return forCategory(category.code);
        }

        public static string ForCategory(IAccountService service)
        {
            if (service == null)
                return String.Empty;

            return forCategory(service.category_code);
        }

        public static string ForService(IAccountService service)
        {
            if (service == null)
                return String.Empty;

            return forAccount(service.account_number, service.occupant_code);
        }

        public static string ForService(IUsageRecord usage)
        {
            if (usage == null)
                return String.Empty;

            return forAccount(usage.account_number, usage.occupant_code);
        }

        static string forAccount(int account, short occupant)
        {
            return String.Concat(account, ":", occupant);
        }

        static string forCategory(string code)
        {
            if (String.IsNullOrEmpty(code))
                return String.Empty;

            return code.Trim().ToUpper();
        }
    }
}
