using Npgsql;

namespace Шилов.Components.Classes
{
    public class Shedule
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public int GroupId { get; set; }
        public int number_room { get; set; }
        public int[] peoples { get; set; } = { };

        public Shedule() { }

        public static async Task SaveShedule(NpgsqlConnection dbConnection, Shedule currentClass)
        {
            try
            {
                await dbConnection.OpenAsync();
                using var cmd = new NpgsqlCommand(
                    currentClass.Id == 0
                    ? "INSERT INTO shedule (date, start_time, end_time, group_id, number_room) VALUES (@date, @startTime, @endTime, @groupId, @number_room)"
                    : "UPDATE shedule SET date = @date, start_time = @startTime, end_time = @endTime, group_id = @groupId, number_room = @number_room WHERE shed_id = @id",
                    dbConnection
                );

                if (currentClass.Id != 0)
                    cmd.Parameters.AddWithValue("@id", currentClass.Id);

                cmd.Parameters.AddWithValue("@date", currentClass.Date);
                cmd.Parameters.AddWithValue("@startTime", currentClass.StartTime);
                cmd.Parameters.AddWithValue("@endTime", currentClass.EndTime);
                cmd.Parameters.AddWithValue("@groupId", currentClass.GroupId);
                cmd.Parameters.AddWithValue("@number_room", currentClass.number_room);
                await cmd.ExecuteNonQueryAsync();
                await dbConnection.CloseAsync();
                string message;
                if(currentClass.Id == 0)
                {
                    message = $"Здравствуйте занятие у вас будет {currentClass.Date.ToString("yyyy-MM-dd")}. Начало занятия в {currentClass.StartTime / 1000 / 60 / 60}:00. Номер зала: {currentClass.number_room}";
                }
                else
                {
                    message = $"Здравствуйте изменение в расписании занятие будет {currentClass.Date.ToString("yyyy-MM-dd")}. Начало занятия в {currentClass.StartTime / 1000 / 60 / 60}:00. Номер зала: {currentClass.number_room}";
                }
                Group sendMsg_group = await Group.GetGroup(currentClass.GroupId, dbConnection);
                foreach(int n in sendMsg_group.client_id_list)
                {
                    Client client = await Client.GetClient(n, dbConnection);
                    Sub_Client sub = await Sub_Client.GetAbonementCl(client.client_abonemnt, dbConnection);
                    if (sub.status.Equals("Активный"))
                    {
                        await Message.SendMessage(n, message, dbConnection);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving class: {ex.Message}");
            }
            finally
            {
                await dbConnection.CloseAsync();
            }
        }
        public static async Task DeleteShedule(NpgsqlConnection dbConnection, Shedule currentClass)
        {
            try
            {
                await dbConnection.OpenAsync();
                using var cmd = new NpgsqlCommand("DELETE FROM shedule WHERE shed_id = @id", dbConnection);
                cmd.Parameters.AddWithValue("@id", currentClass.Id);
                await cmd.ExecuteNonQueryAsync();

                await dbConnection.CloseAsync();

                string message = $"Здравствуйте изменение в расписании, занятия {currentClass.Date.ToString("yyyy-MM-dd")} в {currentClass.StartTime / 1000 / 60 / 60}:00 не будет";
                Group sendMsg_group = await Group.GetGroup(currentClass.GroupId, dbConnection);
                foreach (int n in sendMsg_group.client_id_list)
                {
                    await Message.SendMessage(n, message, dbConnection);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting class: {ex.Message}");
            }
            finally
            {
                await dbConnection.CloseAsync();
            }
        }
        public static async Task<List<Shedule>> GetWeekShedule(NpgsqlConnection dbConnection, DateTime currentWeekStart)
        {
            List<Shedule> list = new();
            try
            {
                await dbConnection.OpenAsync();
                var endDate = currentWeekStart.AddDays(7);
                using var cmd = new NpgsqlCommand("SELECT * FROM shedule WHERE date >= @start AND date < @end", dbConnection);
                cmd.Parameters.AddWithValue("@start", currentWeekStart);
                cmd.Parameters.AddWithValue("@end", endDate);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new Shedule
                    {
                        Id = reader.GetInt32(0),
                        Date = reader.GetDateTime(1),
                        GroupId = reader.GetInt32(2),
                        StartTime = reader.GetInt32(3),
                        EndTime = reader.GetInt32(4),
                        number_room = reader.GetInt32(5)
                    });
                }
            }
            finally
            {
                await dbConnection.CloseAsync();
            }

            return list;
        }
        public static async Task<List<Shedule>> GetPos(NpgsqlConnection dbConnection, int group_id)
        {
            List<Shedule> list = new();
            try
            {
                await dbConnection.OpenAsync();
                using var cmd = new NpgsqlCommand("SELECT * FROM shedule WHERE group_id = @group_id AND peoples IS NOT NULL AND cardinality(peoples) > 0;", dbConnection);
                cmd.Parameters.AddWithValue("@group_id", group_id);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Shedule shedule = new Shedule
                    {
                        Id = reader.GetInt32(0),
                        Date = reader.GetDateTime(1),
                        GroupId = reader.GetInt32(2),
                        StartTime = reader.GetInt32(3),
                        EndTime = reader.GetInt32(4),
                        number_room = reader.GetInt32(5),
                        peoples = (int[])reader.GetValue(6)
                    };
                    list.Add(shedule);
                }
            }
            finally
            {
                await dbConnection.CloseAsync();
            }

            return list;
        }
        public static async Task<List<Shedule>> GetSheduleForTimeSlot(NpgsqlConnection dbConnection, DateTime date, int startTime, int endTime)
        {
            List<Shedule> list = new();
            try
            {
                await dbConnection.OpenAsync();
                using var cmd = new NpgsqlCommand(
                    "SELECT * FROM shedule WHERE date = @date AND " +
                    "((start_time >= @start AND start_time < @end) OR " +
                    "(end_time > @start AND end_time <= @end))",
                    dbConnection);

                cmd.Parameters.AddWithValue("@date", date);
                cmd.Parameters.AddWithValue("@start", startTime);
                cmd.Parameters.AddWithValue("@end", endTime);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new Shedule
                    {
                        Id = reader.GetInt32(0),
                        Date = reader.GetDateTime(1),
                        GroupId = reader.GetInt32(2),
                        StartTime = reader.GetInt32(3),
                        EndTime = reader.GetInt32(4),
                        number_room = reader.GetInt32(5)
                    });
                }
            }
            finally
            {
                await dbConnection.CloseAsync();
            }
            return list;
        }
        public static async Task<List<Shedule>> GetSheduleForOtchet(NpgsqlConnection dbConnection)
        {
            List<Shedule> list = new();
            try
            {
                await dbConnection.OpenAsync();
                using var cmd = new NpgsqlCommand("SELECT date, group_id, peoples FROM shedule", dbConnection);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new Shedule
                    {
                        Date = reader.GetDateTime(0),
                        GroupId = reader.GetInt32(1),
                        peoples = reader.IsDBNull(2) ? Array.Empty<int>() : reader.GetFieldValue<int[]>(2),
                    });
                }
            }
            finally
            {
                await dbConnection.CloseAsync();
            }

            return list;
        }
        public static async Task<List<Shedule>> GetWeekSheduleForGroup(NpgsqlConnection dbConnection, DateTime currentWeekStart, int group_id)
        {
            List<Shedule> list = new();
            try
            {
                await dbConnection.OpenAsync();
                var endDate = currentWeekStart.AddDays(7);
                using var cmd = new NpgsqlCommand("SELECT * FROM shedule WHERE date >= @start AND date < @end and group_id = @group_id", dbConnection);
                cmd.Parameters.AddWithValue("@start", currentWeekStart);
                cmd.Parameters.AddWithValue("@end", endDate);
                cmd.Parameters.AddWithValue("@group_id", group_id);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new Shedule
                    {
                        Id = reader.GetInt32(0),
                        Date = reader.GetDateTime(1),
                        GroupId = reader.GetInt32(2),
                        StartTime = reader.GetInt32(3),
                        EndTime = reader.GetInt32(4),
                        number_room = reader.GetInt32(5)
                    });
                }
            }
            finally
            {
                await dbConnection.CloseAsync();
            }

            return list;
        }
    }
}
