using Npgsql;
using System.Data;

namespace Шилов.Components.Classes
{
    public class Anket
    {
        public int Id { get; set; } = -1;
        public int age { get; set; } = -1;
        public string lv_training {  get; set; } = string.Empty;
        public string goal {  get; set; } = string.Empty;
        public string protivipokazania {  get; set; } = string.Empty;

        public Anket(){ }
        public Anket(int id, int age, string lv_training, string goal, string protivipokazania)
        {
            Id = id;
            this.age = age;
            this.lv_training = lv_training;
            this.goal = goal;
            this.protivipokazania = protivipokazania;
        }
        public static async Task AddAnket(Anket anket, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("insert into anket (anket_id, age, lv_traning, goal, protivopokazania) values (@anket_id, @age, @lv_training, @goal, @protivipokazania)", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@anket_id", anket.Id);
                    cmd.Parameters.AddWithValue("@age", anket.age);
                    cmd.Parameters.AddWithValue("@lv_training", anket.lv_training);
                    cmd.Parameters.AddWithValue("@goal", anket.goal);
                    cmd.Parameters.AddWithValue("@protivipokazania", anket.protivipokazania);
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
        public static async Task<Anket> GetAnket(int id, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM anket where anket_id=@anket_id", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@anket_id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Anket anket = new Anket
                            {
                                Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                age = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                lv_training = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                goal = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                protivipokazania = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                            };
                            return anket;
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
            return new Anket();
        }
        public static async Task EditAnket(Anket anket, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("UPDATE anket SET age = @age, lv_traning = @lvt, goal = @goal, protivopokazania = @prot WHERE anket_id = @anket_id;", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@age", anket.age);
                    cmd.Parameters.AddWithValue("@lvt", anket.lv_training);
                    cmd.Parameters.AddWithValue("@goal", anket.goal);
                    cmd.Parameters.AddWithValue("@prot", anket.protivipokazania);
                    cmd.Parameters.AddWithValue("@anket_id", anket.Id);
                    await cmd.ExecuteNonQueryAsync();
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
        public static async Task RemoveAnket(int anket_id, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("DELETE FROM anket WHERE anket_id = @anket_id", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@anket_id", anket_id);
                    await cmd.ExecuteNonQueryAsync();
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
    }
}
