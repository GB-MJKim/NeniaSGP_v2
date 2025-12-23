using Microsoft.Data.Sqlite;
using Nenia.SGP.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nenia.SGP.Services;

public sealed class ProductService
{
    private readonly DatabaseService _db;

    public ProductService(DatabaseService db) => _db = db;

    public Task<int> GetTotalCountAsync()
        => ScalarIntAsync("SELECT COUNT(*) FROM Products;");

    public Task<int> GetPlacedCountAsync()
        => ScalarIntAsync("SELECT COUNT(*) FROM Products WHERE PageNumber IS NOT NULL;");

    public async Task<List<Product>> GetAllProductsAsync()
    {
        const string sql = "SELECT * FROM Products ORDER BY CreatedDate DESC;";
        return await QueryProductsAsync(sql, null);
    }

    public async Task<List<Product>> SearchProductsAsync(string keyword)
    {
        const string sql = @"
SELECT * FROM Products
WHERE ProductName LIKE $kw
ORDER BY ProductName ASC;";
        var p = new Dictionary<string, object?>
        {
            ["$kw"] = $"%{keyword}%"
        };
        return await QueryProductsAsync(sql, p);
    }

    private async Task<int> ScalarIntAsync(string sql)
    {
        await using var conn = _db.CreateConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var value = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(value);
    }

    private async Task<List<Product>> QueryProductsAsync(string sql, Dictionary<string, object?>? parameters)
    {
        var list = new List<Product>();

        await using var conn = _db.CreateConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        if (parameters != null)
        {
            foreach (var kv in parameters)
                cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
        }

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapProduct(reader));
        }

        return list;
    }

    private static Product MapProduct(SqliteDataReader r)
    {
        return new Product
        {
            ProductID = r.GetInt32(r.GetOrdinal("ProductID")),
            ProductName = r.GetString(r.GetOrdinal("ProductName")),
            Capacity = r.IsDBNull(r.GetOrdinal("Capacity")) ? null : r.GetString(r.GetOrdinal("Capacity")),
            MainImagePath = r.IsDBNull(r.GetOrdinal("MainImagePath")) ? null : r.GetString(r.GetOrdinal("MainImagePath")),
            ThumbnailImagePath = r.IsDBNull(r.GetOrdinal("ThumbnailImagePath")) ? null : r.GetString(r.GetOrdinal("ThumbnailImagePath")),
            PageNumber = r.IsDBNull(r.GetOrdinal("PageNumber")) ? null : r.GetInt32(r.GetOrdinal("PageNumber")),
            OrderInPage = r.IsDBNull(r.GetOrdinal("OrderInPage")) ? null : r.GetInt32(r.GetOrdinal("OrderInPage")),
            StandardPrice = r.IsDBNull(r.GetOrdinal("StandardPrice")) ? 0 : r.GetDouble(r.GetOrdinal("StandardPrice")),
            PricePerUnit = r.IsDBNull(r.GetOrdinal("PricePerUnit")) ? null : r.GetDouble(r.GetOrdinal("PricePerUnit")),
            StorageType = r.IsDBNull(r.GetOrdinal("StorageType")) ? null : r.GetString(r.GetOrdinal("StorageType")),
            Certification = r.IsDBNull(r.GetOrdinal("Certification")) ? null : r.GetString(r.GetOrdinal("Certification")),
            Tags = r.IsDBNull(r.GetOrdinal("Tags")) ? null : r.GetString(r.GetOrdinal("Tags")),
            Ingredients = r.IsDBNull(r.GetOrdinal("Ingredients")) ? null : r.GetString(r.GetOrdinal("Ingredients")),
            AllergyInfo = r.IsDBNull(r.GetOrdinal("AllergyInfo")) ? null : r.GetString(r.GetOrdinal("AllergyInfo")),
            Description = r.IsDBNull(r.GetOrdinal("Description")) ? null : r.GetString(r.GetOrdinal("Description")),
            CreatedDate = r.IsDBNull(r.GetOrdinal("CreatedDate")) ? DateTime.MinValue : r.GetDateTime(r.GetOrdinal("CreatedDate")),
            ModifiedDate = r.IsDBNull(r.GetOrdinal("ModifiedDate")) ? DateTime.MinValue : r.GetDateTime(r.GetOrdinal("ModifiedDate")),
            OriginalImagePath = r.IsDBNull(r.GetOrdinal("OriginalImagePath")) ? null : r.GetString(r.GetOrdinal("OriginalImagePath")),

        };
    }
    public async Task<int> AddProductAsync(Product p)
    {
        const string sql = @"
INSERT INTO Products
(ProductName, Capacity, MainImagePath, ThumbnailImagePath, PageNumber, OrderInPage,
 StandardPrice, PricePerUnit, StorageType, Certification, Tags, Ingredients, AllergyInfo, Description,
 CreatedDate, ModifiedDate)
VALUES
($ProductName, $Capacity, $MainImagePath, $ThumbnailImagePath, $PageNumber, $OrderInPage,
 $StandardPrice, $PricePerUnit, $StorageType, $Certification, $Tags, $Ingredients, $AllergyInfo, $Description,
 CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);";

        await using var conn = _db.CreateConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.AddWithValue("$ProductName", p.ProductName);
        cmd.Parameters.AddWithValue("$Capacity", (object?)p.Capacity ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$MainImagePath", (object?)p.MainImagePath ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$ThumbnailImagePath", (object?)p.ThumbnailImagePath ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$PageNumber", (object?)p.PageNumber ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$OrderInPage", (object?)p.OrderInPage ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$StandardPrice", p.StandardPrice);
        cmd.Parameters.AddWithValue("$PricePerUnit", (object?)p.PricePerUnit ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$StorageType", (object?)p.StorageType ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$Certification", (object?)p.Certification ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$Tags", (object?)p.Tags ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$Ingredients", (object?)p.Ingredients ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$AllergyInfo", (object?)p.AllergyInfo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$Description", (object?)p.Description ?? DBNull.Value);
        // INSERT 구문에 OriginalImagePath 추가
        cmd.Parameters.AddWithValue("$OriginalImagePath", (object?)p.OriginalImagePath ?? DBNull.Value);


        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> UpdateProductAsync(Product p)
    {
        const string sql = @"
UPDATE Products SET
 ProductName = $ProductName,
 Capacity = $Capacity,
 MainImagePath = $MainImagePath,
 ThumbnailImagePath = $ThumbnailImagePath,
 PageNumber = $PageNumber,
 OrderInPage = $OrderInPage,
 StandardPrice = $StandardPrice,
 PricePerUnit = $PricePerUnit,
 StorageType = $StorageType,
 Certification = $Certification,
 Tags = $Tags,
 Ingredients = $Ingredients,
 AllergyInfo = $AllergyInfo,
 Description = $Description,
 ModifiedDate = CURRENT_TIMESTAMP
WHERE ProductID = $ProductID;";

        await using var conn = _db.CreateConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.AddWithValue("$ProductID", p.ProductID);
        cmd.Parameters.AddWithValue("$ProductName", p.ProductName);
        cmd.Parameters.AddWithValue("$Capacity", (object?)p.Capacity ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$MainImagePath", (object?)p.MainImagePath ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$ThumbnailImagePath", (object?)p.ThumbnailImagePath ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$PageNumber", (object?)p.PageNumber ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$OrderInPage", (object?)p.OrderInPage ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$StandardPrice", p.StandardPrice);
        cmd.Parameters.AddWithValue("$PricePerUnit", (object?)p.PricePerUnit ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$StorageType", (object?)p.StorageType ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$Certification", (object?)p.Certification ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$Tags", (object?)p.Tags ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$Ingredients", (object?)p.Ingredients ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$AllergyInfo", (object?)p.AllergyInfo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$Description", (object?)p.Description ?? DBNull.Value);
        // UPDATE 구문에 OriginalImagePath 추가
        cmd.Parameters.AddWithValue("$OriginalImagePath", (object?)p.OriginalImagePath ?? DBNull.Value);


        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> DeleteProductAsync(int productId)
    {
        // 지침서처럼 가격 테이블부터 지우고 상품 삭제(외래키/무결성 고려) [file:1]
        await using var conn = _db.CreateConnection();

        await using (var cmd1 = conn.CreateCommand())
        {
            cmd1.CommandText = "DELETE FROM RegionalPrices WHERE ProductID = $ProductID;";
            cmd1.Parameters.AddWithValue("$ProductID", productId);
            await cmd1.ExecuteNonQueryAsync();
        }

        await using (var cmd2 = conn.CreateCommand())
        {
            cmd2.CommandText = "DELETE FROM Products WHERE ProductID = $ProductID;";
            cmd2.Parameters.AddWithValue("$ProductID", productId);
            return await cmd2.ExecuteNonQueryAsync();
        }
    }

}
