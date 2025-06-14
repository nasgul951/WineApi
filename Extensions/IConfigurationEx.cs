namespace WineApi.Extensions
{
    public static class IConfigurationEx
    {
        public static DbOptions GetDbOptions(this IConfiguration config)
        {
            var dbOptions = new DbOptions();
            config.GetSection(nameof(DbOptions)).Bind(dbOptions);
            return dbOptions;
        }
        
        public static List<string> GetCorsOrigins(this IConfiguration config, params string[] defaultOrigins)
        {
            var corsOrigins = new List<string>();
            config.GetSection("Cors:AllowedOrigins").Bind(corsOrigins);
            return corsOrigins.Count > 0 
                ? corsOrigins 
                : [.. defaultOrigins];
        }
    }
}