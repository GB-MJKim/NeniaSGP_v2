using Microsoft.Data.Sqlite;
using Nenia.SGP.Utilities;
using System;
using System.Threading.Tasks;

namespace Nenia.SGP.Services
{
    public sealed class DatabaseService
    {
        private readonly string _cs;

        public DatabaseService()
        {
            _cs = new SqliteConnectionStringBuilder
            {
                DataSource = PathHelper.GetDbPath(),
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();
        }

        public SqliteConnection CreateConnection()
        {
            var conn = new SqliteConnection(_cs);
            conn.Open();

            // 무결성/안정성 설정
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
PRAGMA foreign_keys = ON;
PRAGMA busy_timeout = 5000;
PRAGMA journal_mode = WAL;";
                cmd.ExecuteNonQuery();
            }

            return conn;
        }

        public async Task InitializeAsync()
        {
            await using var conn = new SqliteConnection(_cs);
            await conn.OpenAsync();

            await using (var pragma = conn.CreateCommand())
            {
                pragma.CommandText = @"
PRAGMA foreign_keys = ON;
PRAGMA busy_timeout = 5000;
PRAGMA journal_mode = WAL;";
                await pragma.ExecuteNonQueryAsync();
            }

            var sql = @"
CREATE TABLE IF NOT EXISTS Products (
  ProductID INTEGER PRIMARY KEY AUTOINCREMENT,
  ProductName TEXT NOT NULL,
  Capacity TEXT,
  MainImagePath TEXT,
  ThumbnailImagePath TEXT,
  PageNumber INTEGER,
  OrderInPage INTEGER,
  StandardPrice REAL,
  PricePerUnit REAL,
  StorageType TEXT,
  Certification TEXT,
  Tags TEXT,
  Ingredients TEXT,
  AllergyInfo TEXT,
  Description TEXT,
  CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
  ModifiedDate DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS Regions (
  RegionID INTEGER PRIMARY KEY AUTOINCREMENT,
  RegionName TEXT UNIQUE NOT NULL,
  DefaultMainType TEXT,
  CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS RegionalPrices (
  PriceID INTEGER PRIMARY KEY AUTOINCREMENT,
  ProductID INTEGER NOT NULL,
  RegionID INTEGER NOT NULL,
  RegionalPrice REAL,
  DiscountRate REAL DEFAULT 0,
  CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
  ModifiedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY(ProductID) REFERENCES Products(ProductID),
  FOREIGN KEY(RegionID) REFERENCES Regions(RegionID),
  UNIQUE(ProductID, RegionID)
);

CREATE TABLE IF NOT EXISTS PageLayoutLogs (
  LogID INTEGER PRIMARY KEY AUTOINCREMENT,
  ProductID INTEGER,
  PreviousPage INTEGER,
  PreviousOrder INTEGER,
  NewPage INTEGER,
  NewOrder INTEGER,
  ChangeDate DATETIME DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY(ProductID) REFERENCES Products(ProductID)
);

CREATE TABLE IF NOT EXISTS Settings (
  SettingKey TEXT PRIMARY KEY,
  SettingValue TEXT
);

CREATE INDEX IF NOT EXISTS idx_productname ON Products(ProductName);
CREATE INDEX IF NOT EXISTS idx_pagenumber ON Products(PageNumber);
CREATE INDEX IF NOT EXISTS idx_regionname ON Regions(RegionName);
CREATE INDEX IF NOT EXISTS idx_prices_product ON RegionalPrices(ProductID);
CREATE INDEX IF NOT EXISTS idx_prices_region ON RegionalPrices(RegionID);
";
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            await EnsureColumnExistsAsync(conn, "Products", "OriginalImagePath", "TEXT");
        }

        private static async Task EnsureColumnExistsAsync(SqliteConnection conn, string table, string column, string type)
        {
            // pragma_table_info('table')에서 name 컬럼을 조회해 존재 여부 확인 [web:259]
            await using (var check = conn.CreateCommand())
            {
                check.CommandText = $"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name = $name;";
                check.Parameters.AddWithValue("$name", column);

                var count = Convert.ToInt32(await check.ExecuteScalarAsync());
                if (count > 0) return;
            }

            await using (var alter = conn.CreateCommand())
            {
                alter.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {type};";
                await alter.ExecuteNonQueryAsync();
            }
        }
    }
}
