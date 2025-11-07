using Microsoft.JSInterop;
using Npgsql;
using System.Data.Common;
using System.Data;

namespace Шилов.Components.Classes
{
    public class Instructor : User
    {
        public int instructor_id { get; set; }
        public int experience { get; set; }
        public string description { get; set; } = string.Empty;
        public int user_id { get; set; }

        public Instructor(User user, int experience, string description)
        {
            this.instructor_id = user.Id;
            this.user_id = user.Id;
            this.experience = experience;
            this.description = description;
        }
        public Instructor() { }
        public static async Task RegisterInstructor(Instructor instructor, int id, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("insert into instructor (instructor_id, experience, description, user_id) values (@instructor_id, @experience, @description, @user_id)", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@instructor_id", instructor.instructor_id);
                    cmd.Parameters.AddWithValue("@experience", instructor.experience);
                    cmd.Parameters.AddWithValue("@description", instructor.description);
                    cmd.Parameters.AddWithValue("@user_id", instructor.user_id);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new NpgsqlCommand("update group_table set instructor_id = @instructor_id where group_id = @groupr_id", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@instructor_id", instructor.instructor_id);
                    cmd.Parameters.AddWithValue("@groupr_id", id);
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
        public static async Task RegisterInstructor(Instructor instructor, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("insert into instructor (instructor_id, experience, description, user_id) values (@instructor_id, @experience, @description, @user_id)", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@instructor_id", instructor.instructor_id);
                    cmd.Parameters.AddWithValue("@experience", instructor.experience);
                    cmd.Parameters.AddWithValue("@description", instructor.description);
                    cmd.Parameters.AddWithValue("@user_id", instructor.user_id);
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
        public static async Task<List<Instructor>> GetListInstructor(NpgsqlConnection dbConnection)
        {
            List<Instructor> Instructors = new List<Instructor>();
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM instructor", dbConnection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Instructor Instructor = new Instructor
                            {
                                instructor_id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                experience = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                user_id = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                            };
                            Instructors.Add(Instructor);
                        }
                    }
                }
                foreach (Instructor instr in Instructors)
                {
                    await dbConnection.CloseAsync();
                    User user = await User.GetUser(instr.user_id, dbConnection);
                    instr.email = user.email;
                    instr.login = user.login;
                    instr.password = user.password;
                    instr.FirstName = user.FirstName;
                    instr.LastName = user.LastName;
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
            Sort(Instructors);
            return Instructors;
        }
        public static async Task EditInstructor(Instructor instructor, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();

                using (var cmd = new NpgsqlCommand("UPDATE instructor SET experience = @experience, description = @description WHERE instructor_id = @instructor_id;", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@experience", instructor.experience);
                    cmd.Parameters.AddWithValue("@description", instructor.description);
                    cmd.Parameters.AddWithValue("@instructor_id", instructor.instructor_id);
                    cmd.ExecuteNonQuery();
                }
                await dbConnection.CloseAsync();
                await User.EditAccount(instructor, instructor.instructor_id, dbConnection);
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
        public static async Task RemoveInstructor(int instructorId, int id_group, NpgsqlConnection dbConnection)
        {
            try
            {
                await dbConnection.OpenAsync();


                using (var cmd = new NpgsqlCommand("UPDATE group_table SET instructor_id = null WHERE group_id = @id_group;", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@id_group", id_group);
                    cmd.ExecuteNonQuery();
                }
            
                using (var cmd = new NpgsqlCommand(
                    "DELETE FROM instructor WHERE instructor_id = @instructor_id", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@instructor_id", instructorId);
                    await cmd.ExecuteNonQueryAsync();
                }
                await dbConnection.CloseAsync();
                await User.RemoveUser(instructorId, dbConnection);
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
        public static async Task<Instructor> GetInstructor(int id, NpgsqlConnection dbConnection)
        {
            Instructor instr = new Instructor();
            try
            {
                await dbConnection.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM instructor where instructor_id = @instructor_id", dbConnection))
                {
                    cmd.Parameters.AddWithValue("@instructor_id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            instr = new Instructor
                            {
                                instructor_id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                experience = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                user_id = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                            };
                        }
                    }
                }
                await dbConnection.CloseAsync();
                User user = await User.GetUser(instr.user_id, dbConnection);
                instr.email = user.email;
                instr.login = user.login;
                instr.password = user.password;
                instr.FirstName = user.FirstName;
                instr.LastName = user.LastName;

            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                await dbConnection.CloseAsync();
            }
            return instr;
        }
        private static void Sort(List<Instructor> Instructors)
        {
            for (int i = 0; i < Instructors.Count; i++)
            {
                for (int j = i + 1; j < Instructors.Count; j++)
                {
                    if (Instructors[i].instructor_id > Instructors[j].instructor_id)
                    {
                        var temp = Instructors[i];
                        Instructors[i] = Instructors[j];
                        Instructors[j] = temp;
                    }
                }
            }
        }
    }
}
