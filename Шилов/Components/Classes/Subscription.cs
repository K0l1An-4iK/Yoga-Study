using Npgsql;
using System.Data;
using System.Data.Common;

namespace Шилов.Components.Classes;
public class Subscription
{
    public int sub_id { get; set; }
    public string sub_name { get; set; } = string.Empty;
    public double price { get; set; }
    public int duration_days { get; set; }

    public static async Task<List<Subscription>> GetList(NpgsqlConnection db)
    {
        var list = new List<Subscription>();
        await db.OpenAsync();
        var cmd = new NpgsqlCommand("SELECT * FROM subscription ORDER BY sub_id", db);
        var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new Subscription
            {
                sub_id = reader.GetInt32(0),
                sub_name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                price = reader.GetDouble(2),
                duration_days = reader.GetInt32(3)
            });
        }
        await db.CloseAsync();
        Sort(list);
        return list;
    }
    public static async Task<List<Subscription>> GetSubscr(int id, NpgsqlConnection db)
    {
        var list = new List<Subscription>();
        await db.OpenAsync();
        var cmd = new NpgsqlCommand("SELECT * FROM subscription where sub_id = @id", db);
        var reader = await cmd.ExecuteReaderAsync();
        cmd.Parameters.AddWithValue("@id", id);
        while (await reader.ReadAsync())
        {
            list.Add(new Subscription
            {
                sub_id = reader.GetInt32(0),
                sub_name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                price = reader.GetDouble(2),
                duration_days = reader.GetInt32(3)
            });
        }
        await db.CloseAsync();
        return list;
    }
    public static async Task Add(Subscription sub, NpgsqlConnection db)
    {
        try
        {
            await db.OpenAsync();
            using (var cmd = new NpgsqlCommand(
            "INSERT INTO subscription (sub_name, price, duration_days) " +
            "VALUES (@sub_name, @price, @days) RETURNING sub_id", db))
            {

                cmd.Parameters.AddWithValue("@sub_name", sub.sub_name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@price", sub.price);
                cmd.Parameters.AddWithValue("@days", sub.duration_days);

                sub.sub_id = (int)await cmd.ExecuteScalarAsync();
                await db.CloseAsync();
            }
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        finally
        {
            await db.CloseAsync();
        }
    }
    public static async Task Update(Subscription sub, NpgsqlConnection db)
    {
        try
        {
            await db.OpenAsync();
            using (var cmd = new NpgsqlCommand(
            "UPDATE subscription SET sub_name = @sub_name, price = @price, duration_days = @days WHERE sub_id = @sub_id;", db))
            {
                cmd.Parameters.AddWithValue("@sub_name", sub.sub_name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@price", sub.price);
                cmd.Parameters.AddWithValue("@days", sub.duration_days);
                cmd.Parameters.AddWithValue("@sub_id", sub.sub_id);
                cmd.ExecuteNonQuery();
            }
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        finally
        {
            await db.CloseAsync();
        }
    }
    public static async Task Delete(int sub_id, NpgsqlConnection db)
    {
        try
        {
            await db.OpenAsync();
            using (var cmd = new NpgsqlCommand(
            "DELETE FROM subscription WHERE sub_id = @sub_id", db))
            {
                cmd.Parameters.AddWithValue("@sub_id", sub_id);
                cmd.ExecuteNonQuery();
            }
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        finally
        {
            await db.CloseAsync();
        }
    }
    private static void Sort(List<Subscription> subscriptions)
    {
        for (int i = 0; i < subscriptions.Count; i++)
        {
            for (int j = i + 1; j < subscriptions.Count; j++)
            {
                if (subscriptions[i].sub_id > subscriptions[j].sub_id)
                {
                    var temp = subscriptions[i];
                    subscriptions[i] = subscriptions[j];
                    subscriptions[j] = temp;
                }
            }
        }
    }
}