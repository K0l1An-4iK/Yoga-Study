using Microsoft.AspNetCore.Components;
using Npgsql;
using System.Data;
using System.Data.Common;

namespace Шилов.Components.Classes
{
    public class User
    {
        public int Id { get; set; } = -1;
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;

        public string login = string.Empty;
        public User() { }
        public static async Task<User> GetUser(string login, string email, string password, NpgsqlConnection dbConnection)
        {
            var user = new User(); // Создаем новый экземпляр

            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("SELECT * FROM user_table", dbConnection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if(reader.GetString(1).Equals(email) && reader.GetString(2).Equals(password) && reader.GetString(6).Equals(login))
                            {
                                user.Id = reader.GetInt32(0);
                                user.email = reader.GetString(1);
                                user.password = reader.GetString(2);
                                if (!reader.IsDBNull(3))
                                    user.FirstName = reader.GetString(3);

                                if (!reader.IsDBNull(4))
                                    user.LastName = reader.GetString(4);

                                if (!reader.IsDBNull(5))
                                    user.role = reader.GetString(5);
                                if (!reader.IsDBNull(6))
                                    user.login = reader.GetString(6);
                            }
                        }
                    }
                }
                return user;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                user.Id = -1; // Помечаем как не загруженный
                return user;
            }
            finally
            {
                if (dbConnection.State == System.Data.ConnectionState.Open)
                    await dbConnection.CloseAsync();
            }
        }
        public static async Task<User> GetUser(int id, NpgsqlConnection dbConnection)
        {
            var user = new User(); // Создаем новый экземпляр

            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("SELECT * FROM user_table where user_id=@user_id", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@user_id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            user.Id = reader.GetInt32(0);
                            user.email = reader.GetString(1);
                            user.password = reader.GetString(2);
                            if (!reader.IsDBNull(3))
                                user.FirstName = reader.GetString(3);
                            if (!reader.IsDBNull(4))
                                user.LastName = reader.GetString(4);
                            if (!reader.IsDBNull(5))
                                user.role = reader.GetString(5);
                            if (!reader.IsDBNull(6))
                                user.login = reader.GetString(6);
                        }
                    }
                }
                return user;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                user.Id = -1; // Помечаем как не загруженный
                return user;
            }
            finally
            {
                if (dbConnection.State == System.Data.ConnectionState.Open)
                    await dbConnection.CloseAsync();
            }
        }
        public static async Task<List<User>> GetListUsers(NpgsqlConnection dbConnection)
        {
            List<User> users = new List<User>();
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM user_table", dbConnection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            User user = new User
                            {
                                Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                email = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                password = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                FirstName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                LastName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                role = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                login = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                            };
                            if(user.role.Equals("Инструктор") || user.role.Equals("Клиент"))
                            {
                                users.Add(user);
                            }
                        }
                    }
                }
                await dbConnection.CloseAsync();
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                await dbConnection.CloseAsync();
            }
            Sort(users);
            return users;
        }
        public static async Task AddUser(User user, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("insert into user_table (login, email, pass, firstname, lastname, role) values (@login, @email, @pass, @firstName, @lastName, @role)", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@login", user.login);
                    cmd.Parameters.AddWithValue("@email", user.email);
                    cmd.Parameters.AddWithValue("@pass", user.password);
                    cmd.Parameters.AddWithValue("@firstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", user.LastName);
                    cmd.Parameters.AddWithValue("@role", user.role);
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
        public static async Task RemoveUser(int id, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand(
                    "DELETE FROM user_table WHERE user_id = @user_id", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@user_id", id);
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
        public static async Task<bool> Authorize(string login, string email, string password, NpgsqlConnection dbConnection)
        {
            User user = await User.GetUser(login, email, password, dbConnection);

            if (user.Id == -1)
            {
                return false;
            }
            return true;
        }
        public static async Task<bool> RegisterUser(string login, string email, string password, NpgsqlConnection dbConnection)
        {
            User user = await User.GetUser(login, email, password, dbConnection);

            if (user.Id == -1)
            {
                user.email = email;
                user.password = password;
                user.login = login;
                user.role = "Клиент";
                await User.AddUser(user, dbConnection);
                return true;
            }
            return false;
        }
        public static async Task EditAccount(User user, int id, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("UPDATE user_table SET email = @email, pass = @password, firstname = @firstname, lastname = @lastname, role = @role, login = @login WHERE user_id = @user_id;", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@email", user.email);
                    cmd.Parameters.AddWithValue("@password", user.password);
                    cmd.Parameters.AddWithValue("@firstname", user.FirstName);
                    cmd.Parameters.AddWithValue("@lastname", user.LastName);
                    cmd.Parameters.AddWithValue("@role", user.role);
                    cmd.Parameters.AddWithValue("@login", user.login);
                    cmd.Parameters.AddWithValue("@user_id", id);
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
        private static void Sort(List<User> users)
        {
            for (int i = 0; i < users.Count; i++)
            {
                for (int j = i + 1; j < users.Count; j++)
                {
                    if (users[i].Id > users[j].Id)
                    {
                        var temp = users[i];
                        users[i] = users[j];
                        users[j] = temp;
                    }
                }
            }
        }
    }

}
