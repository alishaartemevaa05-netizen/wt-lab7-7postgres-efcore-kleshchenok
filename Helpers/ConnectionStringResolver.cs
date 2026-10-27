namespace wt_lab7_7postgres_efcore_kleshchenok.Helpers;

/// <summary>
/// Определяет строку подключения к PostgreSQL:
///  1) переменная окружения DATABASE_URL (Render) — формат postgresql://user:password@host:port/database;
///  2) иначе ConnectionStrings:DefaultConnection (локальная разработка).
/// </summary>
public static class ConnectionStringResolver
{
    public static string Resolve(IConfiguration configuration)
    {
        var databaseUrl = configuration["DATABASE_URL"];
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':', 2);

             var port = uri.Port > 0 ? uri.Port : 5432;

                       
            var external = uri.Host.EndsWith(".render.com", StringComparison.OrdinalIgnoreCase);
            var ssl = external ? "SSL Mode=Require;Trust Server Certificate=true" : "SSL Mode=Disable";

            return $"Host={uri.Host};Port={port};Database={uri.AbsolutePath.TrimStart('/')};" +
                   $"Username={Uri.UnescapeDataString(userInfo[0])};" +
                   $"Password={Uri.UnescapeDataString(userInfo.Length > 1 ? userInfo[1] : "")};" + ssl;
        }

        return configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("Не задана строка подключения (DATABASE_URL или DefaultConnection).");
    }
}
