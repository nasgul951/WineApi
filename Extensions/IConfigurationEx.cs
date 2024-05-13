namespace WineApi.Extensions
{
    public static class IConfigurationEx {
        public static DbOptions GetDbOptions(this IConfiguration config) {
            var dbOptions = new DbOptions();
            config.GetSection(nameof(DbOptions)).Bind(dbOptions);
            return dbOptions;
        }
    }
}