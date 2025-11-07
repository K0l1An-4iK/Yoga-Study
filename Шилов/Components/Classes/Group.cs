using Npgsql;
using NpgsqlTypes;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Шилов.Components.Classes
{
    public class Group
    {
        public int Id { get; set; } = -1;
        public int instructor_id { get; set; } = -1;

        public int[] client_id_list = { };
        public string goal { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;

        public Group() { }
        public static async Task<List<Group>> GetListGroup(NpgsqlConnection dbConnection)
        {
            List<Group> Groups = new List<Group>();
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM group_table", dbConnection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Group group = new Group
                            {
                                Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                instructor_id = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                client_id_list = reader.IsDBNull(2) ? Array.Empty<int>() : reader.GetFieldValue<int[]>(2),
                                goal = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                name = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                            };
                            Groups.Add(group);
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
            Sort(Groups);
            return Groups;
        }
        public static async Task<Group> GetGroup(int id, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM group_table where group_id=@group_id", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@group_id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Group group = new Group
                            {
                                Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                instructor_id = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                client_id_list = reader.IsDBNull(2) ? Array.Empty<int>() : reader.GetFieldValue<int[]>(2),
                                goal = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                name = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                            };
                            return group;
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
            return new Group();
        }
        public static async Task AddClientInGroup(Client client, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();
                string sql = "UPDATE group_table SET client_id = client_id || ARRAY[@client_id] WHERE array_position(client_id, @client_id) IS NULL AND group_id = @group_id;";
                using (var cmd = new NpgsqlCommand(sql, dbConnection))
                {
                    cmd.Parameters.AddWithValue("@group_id", client.group_id);
                    cmd.Parameters.AddWithValue("@client_id", client.client_Id);

                    int affectedRows = await cmd.ExecuteNonQueryAsync();

                    if (affectedRows == 0)
                    {
                        Console.WriteLine($"Клиент {client.client_Id} уже находится в группе {client.group_id}");
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка при добавлении клиента в группу: {ex.Message}");
                throw;
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                    await dbConnection.CloseAsync();
            }
        }
        public static async Task AddGroup(Group group, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("insert into group_table (goal, name) values (@goal, @name)", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@goal", group.goal);
                    cmd.Parameters.AddWithValue("@name", group.name);
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
        public static async Task UpdateGroup(Group currentGroup, int oldgroup, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("update group_table set instructor_id = null where group_id = @oldgroup", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@oldgroup", oldgroup);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new NpgsqlCommand("update group_table set instructor_id = @instructor_id, goal = @goal, name = @name where group_id = @group_id", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@instructor_id", currentGroup.instructor_id);
                    cmd.Parameters.AddWithValue("@goal", currentGroup.goal);
                    cmd.Parameters.AddWithValue("@name", currentGroup.name);
                    cmd.Parameters.AddWithValue("@group_id", currentGroup.Id);
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
        public static async Task DeleteGroup(int group_id, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("UPDATE client SET group_id = NULL WHERE group_id = @group_id;", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@group_id", group_id);
                    await cmd.ExecuteNonQueryAsync();
                }
                using (var cmd = new NpgsqlCommand("UPDATE shedule SET group_id = NULL WHERE group_id = @group_id;", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@group_id", group_id);
                    await cmd.ExecuteNonQueryAsync();
                }
                using (var cmd = new NpgsqlCommand("delete from group_table where group_id = @group_id;", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@group_id", group_id);
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
        private static void Sort(List<Group> groups)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                for (int j = i + 1; j < groups.Count; j++)
                {
                    if (groups[i].Id > groups[j].Id)
                    {
                        var temp = groups[i];
                        groups[i] = groups[j];
                        groups[j] = temp;
                    }
                }
            }
        }
    }
}
