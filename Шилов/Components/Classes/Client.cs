using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace Шилов.Components.Classes
{
    public class Client : User
    {
        public int client_Id { get; set; }
        public int user_id { get; set; }
        public int group_id { get; set; }
        public int anket_id { get; set; }
        public int client_abonemnt { get; set; }
        public Client(User user, int group_id, int anket_id)
        {
            this.client_Id = user.Id;
            this.user_id = user.Id;
            this.group_id = group_id;
            this.anket_id = anket_id;
        }
        public Client() { }
        public static async Task AddClient(Client client, Anket newAnket, NpgsqlConnection dbConnection)
        {
            await User.AddUser(client, dbConnection);
            User user = await User.GetUser(client.login, client.email, client.password, dbConnection);
            newAnket.Id = user.Id;
            await Anket.AddAnket(newAnket, dbConnection);
            client.client_Id = user.Id;
            client.user_id = user.Id;
            client.anket_id = newAnket.Id;
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("insert into client (client_id, user_id, group_id, anket_id) values (@client_id, @user_id, @group_id, @anket_id)", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@client_id", client.client_Id);
                    cmd.Parameters.AddWithValue("@user_id", client.user_id);
                    cmd.Parameters.AddWithValue("@group_id", client.group_id);
                    cmd.Parameters.AddWithValue("@anket_id", client.anket_id);
                    cmd.ExecuteNonQuery();
                }
                await dbConnection.CloseAsync();
                await Group.AddClientInGroup(client, dbConnection);
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
        public static async Task<List<Client>> GetListClient(NpgsqlConnection dbConnection)
        {
            List<Client> Clients = new List<Client>();
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM Client", dbConnection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Client Client = new Client
                            {
                                client_Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                user_id = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                group_id = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                anket_id = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                client_abonemnt = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                            };
                            Clients.Add(Client);
                        }
                    }
                }
                await dbConnection.CloseAsync();
                foreach (Client client in Clients)
                {
                    User user = await User.GetUser(client.user_id, dbConnection);
                    client.email = user.email;
                    client.login = user.login;
                    client.password = user.password;
                    client.FirstName = user.FirstName;
                    client.LastName = user.LastName;
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
            Sort(Clients);
            return Clients;
        }
        public static async Task EditClient(Client client, Anket anket, NpgsqlConnection dbConnection)
        {
            if (client.role.Equals(string.Empty))
            {
                client.role = "Клиент";
            }
            try
            {
                await User.EditAccount(client, client.user_id, dbConnection);
                await Anket.EditAnket(anket, dbConnection);
                await EditClientIngroup(client, dbConnection);
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
        public static async Task RemoveClient(Client client, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();
                string sql = "UPDATE group_table SET client_id = array_remove(client_id, @client_id) WHERE group_id = @group_id;";
                using (var cmd = new NpgsqlCommand(sql, dbConnection))
                {
                    cmd.Parameters.AddWithValue("@client_id", client.client_Id);
                    cmd.Parameters.AddWithValue("@group_id", client.group_id);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new NpgsqlCommand(
                    "DELETE FROM client WHERE client_id = @client_id", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@client_id", client.client_Id);
                    await cmd.ExecuteNonQueryAsync();
                }
                await dbConnection.CloseAsync();
                await Anket.RemoveAnket(client.anket_id, dbConnection);
                await User.RemoveUser(client.user_id, dbConnection);
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
        public static async Task EditClientIngroup(Client client, NpgsqlConnection dbConnection)
        {
            //SELECT client_id FROM public.group_table where group_id = @oldgroup_id
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("update group_table set client_id = array_remove(client_id, @client_id);", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@client_id", client.client_Id);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new NpgsqlCommand("update client set group_id = @group_id where client_id = @client_id;", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@group_id", client.group_id);
                    cmd.Parameters.AddWithValue("@client_id", client.client_Id);
                    cmd.ExecuteNonQuery();
                }
                await dbConnection.CloseAsync();
                await Group.AddClientInGroup(client, dbConnection);
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка при добавлении клиента в группу: {ex.Message}");
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                    await dbConnection.CloseAsync();
            }
        }
        private static void Sort(List<Client> clients)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                for (int j = i + 1; j < clients.Count; j++)
                {
                    if (clients[i].client_Id > clients[j].client_Id)
                    {
                        var temp = clients[i];
                        clients[i] = clients[j];
                        clients[j] = temp;
                    }
                }
            }
        }
        public static async Task<Client> GetClient(int id, NpgsqlConnection dbConnection)
        {
            Client Client = new Client();
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM Client where client_id = @id", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Client = new Client
                            {
                                client_Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                user_id = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                group_id = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                anket_id = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                client_abonemnt = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                            };
                        }
                    }
                }
                await dbConnection.CloseAsync();
                User user = await User.GetUser(id, dbConnection);
                Client.email = user.email;
                Client.login = user.login;
                Client.password = user.password;
                Client.FirstName = user.FirstName;
                Client.LastName = user.LastName;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                await dbConnection.CloseAsync();
            }
            return Client;
        }
        public static async Task AddNewClient(string login, string email, string password, NpgsqlConnection dbConnection)
        {
            Anket newAnket = new Anket();
            Client client = new Client();
            await User.RegisterUser(login, email, password, dbConnection);
            User user = await User.GetUser(login, email, password, dbConnection);
            newAnket.Id = user.Id;
            await Anket.AddAnket(newAnket, dbConnection);
            client.client_Id = user.Id;
            client.user_id = user.Id;
            client.anket_id = newAnket.Id;
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("insert into client (client_id, user_id, anket_id) values (@client_id, @user_id, @anket_id)", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@client_id", client.client_Id);
                    cmd.Parameters.AddWithValue("@user_id", client.user_id);
                    cmd.Parameters.AddWithValue("@anket_id", client.anket_id);
                    cmd.ExecuteNonQuery();
                }
                await dbConnection.CloseAsync();
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
        public static async Task AddNewClientForClient(Client client, Anket newAnket, NpgsqlConnection dbConnection)
        {
            await Anket.EditAnket(newAnket, dbConnection);
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("UPDATE user_table SET firstname = @firstname, lastname = @lastname WHERE user_id = @user_id;", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@firstname", client.FirstName);
                    cmd.Parameters.AddWithValue("@lastname", client.LastName);
                    cmd.Parameters.AddWithValue("@user_id", client.user_id);
                    await cmd.ExecuteNonQueryAsync();
                }
                await dbConnection.CloseAsync();
                await Message.SendMessage(3, $"Добавлен новый клиент {client.LastName} {client.FirstName} добавьте его в группу!", dbConnection);
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
