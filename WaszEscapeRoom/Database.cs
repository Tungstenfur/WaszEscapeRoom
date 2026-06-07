namespace WaszEscapeRoom;
using MySqlConnector;
using System.Collections.Generic;

public class Database
{
    static readonly string _login = "Server=localhost;Database=game;User ID=game;Password=123;";

    public static void InitDB()
    {
        using var connection = new MySqlConnection(_login);
        connection.Open();
        string query = "CREATE TABLE IF NOT EXISTS users (" +
                       "id INT AUTO_INCREMENT PRIMARY KEY," +
                       "username VARCHAR(255) NOT NULL UNIQUE," +
                       "password VARCHAR(255) NOT NULL)";
        using var command = new MySqlCommand(query, connection);
        command.ExecuteNonQuery();
        query = "CREATE TABLE IF NOT EXISTS Progress (" +
                       "user_id INT PRIMARY KEY," +
                       "current_level INT NOT NULL DEFAULT 0," +
                       "CONSTRAINT fk_progress_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE" +
                    ")";
        using var command2 = new MySqlCommand(query, connection);
        command2.ExecuteNonQuery();
        query = "CREATE TABLE IF NOT EXISTS level_times (" +
                "id INT AUTO_INCREMENT PRIMARY KEY," +
                "user_id INT NOT NULL," +
                "level INT NOT NULL," +
                "completed_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP," +
                "time_seconds INT NOT NULL," +
                "CONSTRAINT fk_level_times_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE," +
                "UNIQUE KEY uq_user_level (user_id, level)" +
                ")";
        using var command3 = new MySqlCommand(query, connection);
        command3.ExecuteNonQuery();
        connection.Close();
    }

    public static void LogLevelCompletion(int userId, int level, int timeSeconds)
    {
        using var connection = new MySqlConnection(_login);
        connection.Open();
        var query = "SELECT time_seconds FROM level_times WHERE user_id = @userId AND level = @level";
        using var checkCommand = new MySqlCommand(query, connection);
        checkCommand.Parameters.AddWithValue("@userId", userId);
        checkCommand.Parameters.AddWithValue("@level", level);
        var existingTime = checkCommand.ExecuteScalar();
        if (existingTime != null && existingTime != DBNull.Value)
        {
            int existingSeconds = Convert.ToInt32(existingTime);
            if (timeSeconds >= existingSeconds)
            {
                return;
            }
        }

        query = "INSERT INTO level_times (user_id, level, time_seconds) " +
                "VALUES (@userId, @level, @timeSeconds) " +
                "ON DUPLICATE KEY UPDATE time_seconds = @timeSeconds, completed_at = CURRENT_TIMESTAMP";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@level", level);
        command.Parameters.AddWithValue("@timeSeconds", timeSeconds);
        command.ExecuteNonQuery();
    }

    public static List<(int level, int timeSeconds)> GetLevelTimes(int userId)
    {
        var result = new List<(int level, int timeSeconds)>();
        using var connection = new MySqlConnection(_login);
        connection.Open();
        var query = "SELECT level, time_seconds FROM level_times WHERE user_id = @userId ORDER BY level";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@userId", userId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add((reader.GetInt32("level"), reader.GetInt32("time_seconds")));
        }
        return result;
    }

    public static int GetUserId(string username)
    {
        using var connection = new MySqlConnection(_login);
        connection.Open();
        var query = "SELECT id FROM users WHERE username = @username";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@username", username);
        var result = command.ExecuteScalar();
        if (result is null || result is DBNull)
        {
            return -1;
        }
        return Convert.ToInt32(result);
    }

    public static LoginResult verifyLogin(string username, string password)
    {
        using var connection = new MySqlConnection(_login);
        connection.Open();
        string query = "SELECT * FROM users WHERE username = @username";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@username", username);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            string storedHash = reader.GetString("password");
            if(Auth.VerifyPassword(password, storedHash))return LoginResult.Success;
            else return LoginResult.InvalidPassword;
        }
        connection.Close();
        return LoginResult.UserNotFound;
    }

    public static RegisterResult registerUser(string username, string password)
    {
        var passwordHash = Auth.HashPassword(password);
        using var connection = new MySqlConnection(_login);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var query = "INSERT INTO users (username, password) VALUES (@username, @password)";
        using var command = new MySqlCommand(query, connection, transaction);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password", passwordHash);
        try
        {
            command.ExecuteNonQuery();
            var userId = command.LastInsertedId;
            var progressQuery = "INSERT INTO Progress (user_id, current_level) VALUES (@userId, 0)";
            using var progressCommand = new MySqlCommand(progressQuery, connection, transaction);
            progressCommand.Parameters.AddWithValue("@userId", userId);
            progressCommand.ExecuteNonQuery();
            transaction.Commit();
            return RegisterResult.Success;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            if (ex.Message.Contains("Duplicate entry"))
            {
                return RegisterResult.UserAlreadyExists;
            }
            throw;
        }
    }

    public static int GetCurrentLevel(string username)
    {
        using var connection = new MySqlConnection(_login);
        connection.Open();
        var query = "SELECT p.current_level FROM users u LEFT JOIN Progress p ON p.user_id = u.id WHERE u.username = @username";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@username", username);
        var result = command.ExecuteScalar();
        if (result is null || result is DBNull)
        {
            return 0;
        }
        return Convert.ToInt32(result);
    }

    public static void SetCurrentLevel(string username, int level)
    {
        using var connection = new MySqlConnection(_login);
        connection.Open();
        var query = "INSERT INTO Progress (user_id, current_level) " +
                    "SELECT id, @level FROM users WHERE username = @username " +
                    "ON DUPLICATE KEY UPDATE current_level = @level";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@level", level);
        command.ExecuteNonQuery();
    }

    public static  List<(string username, int timeSeconds)> GetLeaderboardForLevel(int level, int limit = 10)
    {
        var result = new List<(string username, int timeSeconds)>();
        using var connection = new MySqlConnection(_login);
        connection.Open();
        var query = """
                    SELECT u.username, lt.time_seconds
                    FROM level_times lt
                    JOIN users u ON lt.user_id = u.id
                    WHERE lt.level = @level
                    ORDER BY lt.time_seconds ASC
                    LIMIT @limit;
                    """;
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@level", level);
        command.Parameters.AddWithValue("@limit", limit);
        using var reader = command.ExecuteReaderAsync().Result;
        while (reader.Read())
        {
            result.Add((reader.GetString("username"), reader.GetInt32("time_seconds")));
        }
        return result;
    }

    public static async Task deleteUserProgress(string username)
    {
        using var connection = new MySqlConnection(_login);
        await connection.OpenAsync();
        var query = "DELETE FROM Progress WHERE user_id = (SELECT id FROM users WHERE username = @username)";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@username", username);
        await command.ExecuteNonQueryAsync();
    }
}