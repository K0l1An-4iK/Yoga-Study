using Npgsql;
using System.Data;

namespace Шилов.Components.Classes
{
    public class Sub_Client
    {
        public int sub_cl_id { get; set; }
        public DateTime start_time { get; set; }
        public DateTime end_time { get; set; }
        public string status { get; set; } = string.Empty;
        public int sub_id { get; set; }
        public int days { get; set; }

        public Sub_Client() { }
        public static async Task RemoveSubClient(int id, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("DELETE FROM sub_client WHERE sub_cl_id = @sub_cl_id", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@sub_cl_id", id);
                    cmd.ExecuteNonQuery();
                }
                await dbConnection.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении: {ex.Message}");
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                    await dbConnection.CloseAsync();
            }
        }
        public static async Task<Sub_Client> GetAbonementCl(int id, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sub_client where sub_cl_id = @id", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Sub_Client sub_cl = new Sub_Client
                            {
                                sub_cl_id = reader.GetInt32(0),
                                start_time = reader.GetDateTime(1),
                                end_time = reader.GetDateTime(2),
                                status = reader.GetString(3),
                                sub_id = reader.GetInt32(4),
                                days = reader.GetInt32(5),
                            };
                            var daysDate = sub_cl.end_time - DateTime.Now;
                            sub_cl.days = daysDate.Days;
                            return sub_cl;
                        }
                    }
                }
                await dbConnection.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении: {ex.Message}");
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                    await dbConnection.CloseAsync();
            }
            return new Sub_Client();
        }
        public static async Task<List<Sub_Client>> GetListAbonementCl(NpgsqlConnection dbConnection)
        {
            List<Sub_Client> sub_Clients = new List<Sub_Client>();
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sub_client", dbConnection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Sub_Client sub_cl = new Sub_Client
                            {
                                sub_cl_id = reader.GetInt32(0),
                                start_time = reader.GetDateTime(1),
                                end_time = reader.GetDateTime(2),
                                status = reader.GetString(3),
                                sub_id = reader.GetInt32(4),
                                days = reader.GetInt32(5)
                            };
                            sub_Clients.Add(sub_cl);
                        }
                    }
                }
                foreach (var sub_cl in sub_Clients)
                {
                    if (sub_cl.status.Equals(sub_cl.days <= 0))
                    {
                        sub_cl.status = "Закончился";
                        using (var cmd = new NpgsqlCommand("Update sub_client set status = @status where sub_cl_id = @id", dbConnection))
                        {
                            cmd.Parameters.AddWithValue("@id", sub_cl.sub_cl_id);
                            cmd.Parameters.AddWithValue("@status", "Закончился");
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                foreach (var sub_cl in sub_Clients)
                {
                    if (sub_cl.status.Equals("Активный"))
                    {
                        var daysDate = sub_cl.end_time - DateTime.Now;
                        sub_cl.days = daysDate.Days;
                        using (var cmd = new NpgsqlCommand("Update sub_client set days = @days where sub_cl_id = @id", dbConnection))
                        {
                            cmd.Parameters.AddWithValue("@id", sub_cl.sub_cl_id);
                            cmd.Parameters.AddWithValue("@days", sub_cl.days);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                await dbConnection.CloseAsync();
                return sub_Clients;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении: {ex.Message}");
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                    await dbConnection.CloseAsync();
            }
            return new List<Sub_Client>();
        }
        public static async Task Freeze(int id, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("Update sub_client set status = @status where sub_cl_id = @id", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@status", "Заморожен");
                    cmd.ExecuteNonQuery();
                }

                await dbConnection.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении: {ex.Message}");
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                    await dbConnection.CloseAsync();
            }
        }
        public static async Task NotFreeze(int id, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("Update sub_client set status = @status where sub_cl_id = @id", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@status", "Активный");
                    cmd.ExecuteNonQuery();
                }

                await dbConnection.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении: {ex.Message}");
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                    await dbConnection.CloseAsync();
            }
        }
        public static async Task AddSub_CL(Subscription sub, DateTime startTime, DateTime endTime, int client_id, NpgsqlConnection db)
        {
            try
            {
                int id;
                await db.OpenAsync();
                using (var cmd = new NpgsqlCommand(
                "INSERT INTO sub_client (start_time, end_time, status, sub_id, days) " +
                "VALUES (@start_time, @end_time, @status, @sub_id, @days) RETURNING sub_cl_id", db))
                {
                    cmd.Parameters.AddWithValue("@start_time", startTime);
                    cmd.Parameters.AddWithValue("@end_time", endTime);
                    cmd.Parameters.AddWithValue("@status", "Активный");
                    cmd.Parameters.AddWithValue("@sub_id", sub.sub_id);
                    cmd.Parameters.AddWithValue("@days", sub.duration_days);
                    id = (int)await cmd.ExecuteScalarAsync();
                }
                using (var cmd = new NpgsqlCommand(
                "UPDATE client SET sub_cl_id = @sub_cl_id WHERE client_id = @client_id;", db))
                {
                    cmd.Parameters.AddWithValue("@sub_cl_id", id);
                    cmd.Parameters.AddWithValue("@client_id", client_id);
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
        public static async Task ExtendSubClient(DateTime endTime, int days, int id, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("Update sub_client set end_time = @endTime, days = @days WHERE sub_cl_id = @sub_cl_id", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@endTime", endTime);
                    cmd.Parameters.AddWithValue("@days", days);
                    cmd.Parameters.AddWithValue("@sub_cl_id", id);
                    cmd.ExecuteNonQuery();
                }
                await dbConnection.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении: {ex.Message}");
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                    await dbConnection.CloseAsync();
            }
        }
        public static async Task DeleteSubCLient(Client client, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();
                string sql = "UPDATE client SET sub_cl_id = null WHERE client_id = @client_id;";
                using (var cmd = new NpgsqlCommand(sql, dbConnection))
                {
                    cmd.Parameters.AddWithValue("@client_id", client.client_Id);
                    cmd.ExecuteNonQuery();
                }
                await dbConnection.CloseAsync();
                await Sub_Client.RemoveSubClient(client.client_abonemnt, dbConnection);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении: {ex.Message}");
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                    await dbConnection.CloseAsync();
            }
        }
        public static async Task<List<Sub_Client>> GetListAbonementCl(NpgsqlConnection dbConnection, DateTime start, DateTime end)
        {
            List<Sub_Client> sub_Clients = new List<Sub_Client>();
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sub_client WHERE start_time BETWEEN @start AND @end ORDER BY start_time;", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@start", start);
                    cmd.Parameters.AddWithValue("@end", end);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Sub_Client sub_cl = new Sub_Client
                            {
                                sub_cl_id = reader.GetInt32(0),
                                start_time = reader.GetDateTime(1),
                                end_time = reader.GetDateTime(2),
                                status = reader.GetString(3),
                                sub_id = reader.GetInt32(4),
                                days = reader.GetInt32(5)
                            };
                            sub_Clients.Add(sub_cl);
                        }
                    }
                }
                await dbConnection.CloseAsync();
                return sub_Clients;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении: {ex.Message}");
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                    await dbConnection.CloseAsync();
            }
            return new List<Sub_Client>();
        }
    }
}
