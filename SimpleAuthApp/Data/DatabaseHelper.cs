using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace SimpleAuthApp.Data
{
    public static class DatabaseHelper
    {
        // Default connection string for LocalDB
        private static string _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=SimpleAuthDB;Trusted_Connection=True;";

        public static void SetConnectionString(string connString)
        {
            _connectionString = connString;
        }

        public static bool RegisterUser(string username, string password, string fullName)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Check if user exists
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                using (var checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Username", username);
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0) return false;
                }

                string insertQuery = "INSERT INTO Users (Username, PasswordHash, FullName) VALUES (@Username, @PasswordHash, @FullName)";
                using (var cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(password));
                    cmd.Parameters.AddWithValue("@FullName", fullName ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        public static bool ValidateUser(string username, string password)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT PasswordHash FROM Users WHERE Username = @Username";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        string storedHash = result.ToString();
                        return storedHash == HashPassword(password);
                    }
                }
            }
            return false;
        }

        public static string GetUserImage(string username)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT ProfileImageBase64 FROM Users WHERE Username = @Username";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    var result = cmd.ExecuteScalar();
                    return result as string;
                }
            }
        }
        
        public static string GetUserFullName(string username)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT FullName FROM Users WHERE Username = @Username";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    var result = cmd.ExecuteScalar();
                    return result as string;
                }
            }
        }

        public static void UpdateUserImage(string username, string base64Image)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "UPDATE Users SET ProfileImageBase64 = @Image WHERE Username = @Username";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Image", base64Image ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static DataTable GetAllUsers(string currentUsername)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT Username, FullName, ProfileImageBase64 FROM Users WHERE Username != @Username";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", currentUsername);
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public static void CreatePost(string username, string content)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Get UserId
                string idQuery = "SELECT Id FROM Users WHERE Username = @Username";
                int userId;
                using(var idCmd = new SqlCommand(idQuery, conn))
                {
                    idCmd.Parameters.AddWithValue("@Username", username);
                    object result = idCmd.ExecuteScalar();
                    if(result == null) return;
                    userId = (int)result;
                }

                string insertQuery = "INSERT INTO Posts (UserId, Content) VALUES (@UserId, @Content)";
                using (var cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Content", content);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static DataTable GetUserPosts(string username)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT p.Content, p.CreatedAt 
                    FROM Posts p 
                    JOIN Users u ON p.UserId = u.Id 
                    WHERE u.Username = @Username 
                    ORDER BY p.CreatedAt DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
