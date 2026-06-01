namespace WaszEscapeRoom;
using System.Threading;
using MySqlConnector;
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

        // migracja
        try
        {
            using var addUserId = new MySqlCommand("ALTER TABLE Progress ADD COLUMN user_id INT NULL", connection);
            addUserId.ExecuteNonQuery();
        }
        catch (MySqlException)
        {
            // Kolumna już istnieje
        }

        using (var backfillUserId = new MySqlCommand("UPDATE Progress SET user_id = id WHERE user_id IS NULL", connection))
        {
            backfillUserId.ExecuteNonQuery();
        }

        try
        {
            using var addUnique = new MySqlCommand("ALTER TABLE Progress ADD UNIQUE KEY uq_progress_user_id (user_id)", connection);
            addUnique.ExecuteNonQuery();
        }
        catch (MySqlException)
        {
            // Indeks już istnieje
        }
        connection.Close();
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
}