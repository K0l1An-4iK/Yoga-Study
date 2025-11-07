using Microsoft.AspNetCore.Mvc.ModelBinding;
using Npgsql;
using System.Data;

namespace Шилов.Components.Classes
{
    public class Report
    {
        public int Id { get; set; }
        public int sum { get; set; }
        public string Descript { get; set; } = string.Empty;
        public DateTime date { get; set; }
        public string category { get; set; } = string.Empty;

        public static async Task SaveIncome(int sum, string desc, DateTime date, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("insert into income (income_sum, desc_income, date_income) values (@income_sum, @desc_income, @date_income)", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@income_sum", sum);
                    cmd.Parameters.AddWithValue("@desc_income", desc);
                    cmd.Parameters.AddWithValue("@date_income", date);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                if (dbConnection.State == System.Data.ConnectionState.Open)
                    await dbConnection.CloseAsync();
            }
        }
        public static async Task SaveExpenses(int sum, string desc, string category, DateTime date, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("insert into expenses (expenses_sum, desc_expenses, date_expenses, category) values (@expenses_sum, @desc_expenses, @date_expenses, @category)", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@expenses_sum", sum);
                    cmd.Parameters.AddWithValue("@desc_expenses", desc);
                    cmd.Parameters.AddWithValue("@date_expenses", date);
                    cmd.Parameters.AddWithValue("@category", category);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                if (dbConnection.State == System.Data.ConnectionState.Open)
                    await dbConnection.CloseAsync();
            }
        }
        public static async Task<List<Report>> GetListIncomes(NpgsqlConnection dbConnection)
        {
            string incomeQuery = "SELECT * FROM income";
            List<Report> reports = new List<Report>();
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand(incomeQuery, dbConnection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Report getreport = new Report
                            {
                                Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                sum = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                Descript = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                date = reader.GetDateTime(3)
                            };
                            reports.Add(getreport);
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                await dbConnection.CloseAsync();
            }
            return reports;
        }
        public static async Task<List<Report>> GetListExpenses(NpgsqlConnection dbConnection)
        {
            string expenseQuery = "SELECT * FROM expenses";
            List<Report> reports = new List<Report>();
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand(expenseQuery, dbConnection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Report getreport = new Report
                            {
                                Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                sum = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                Descript = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                date = reader.GetDateTime(3),
                                category = reader.GetString(4)
                            };
                            reports.Add(getreport);
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                await dbConnection.CloseAsync();
            }
            return reports;
        }
        public static async Task DeleteExp(int Id, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("delete from expenses where expenses_id = @Id;", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@Id", Id);
                    await cmd.ExecuteNonQueryAsync();
                }
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
    }
}
