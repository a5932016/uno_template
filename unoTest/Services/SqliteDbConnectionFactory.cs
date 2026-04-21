using Microsoft.Data.Sqlite;

namespace unoTest.Services;

public interface ISqliteDbConnectionFactory
{
    SqliteConnection CreateOpenConnection();
}

/// <summary>
/// 集中管理 SQLite 連線建立與 DB 檔案位置，避免在業務層硬編碼。
/// </summary>
public sealed class SqliteDbConnectionFactory : ISqliteDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteDbConnectionFactory()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(appDataPath))
        {
            appDataPath = AppContext.BaseDirectory;
        }

        Directory.CreateDirectory(appDataPath);

        var dbPath = Path.Combine(appDataPath, "unoTest.template.db3");
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        }.ToString();
    }

    public SqliteConnection CreateOpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
