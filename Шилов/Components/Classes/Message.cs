using Npgsql;

namespace Шилов.Components.Classes
{
    public class Message
    {
        public int messageId {  get; set; }
        public int user_id {  get; set; }
        public string message_text {  get; set; } = string.Empty;

        public Message(int user_id, string message_text) 
        {
            this.user_id = user_id;
            this.message_text = message_text;
        }
        public Message(){}
        public static async Task SendMessage(int user_id, string text, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("insert into message (user_id, message_text) values (@user_id, @message_text)", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@user_id", user_id);
                    cmd.Parameters.AddWithValue("@message_text", text);
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
        public static async Task<List<Message>> GetListMessage(int id, NpgsqlConnection dbConnection)
        {
            List<Message> Messages = new List<Message>();
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM public.message where user_id = @user_id", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@user_id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Message Message = new Message
                            {
                                messageId = reader.GetInt32(0),
                                user_id = reader.GetInt32(1),
                                message_text = reader.GetString(2)
                            };
                            Messages.Add(Message);
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
            Sort(Messages);
            return Messages;
        }
        private static void Sort(List<Message> Messages)
        {
            for (int i = 0; i < Messages.Count; i++)
            {
                for (int j = i + 1; j < Messages.Count; j++)
                {
                    if (Messages[i].messageId > Messages[j].messageId)
                    {
                        var temp = Messages[i];
                        Messages[i] = Messages[j];
                        Messages[j] = temp;
                    }
                }
            }
        }
        public static async Task DeleteMessage(int message_Id, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("Delete from public.message where message_id = @message_id;", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@message_id", message_Id);
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
    }
}