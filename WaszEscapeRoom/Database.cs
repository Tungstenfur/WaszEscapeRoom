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
                       "id int PRIMARY KEY AUTO_INCREMENT,"+
                       "current_level INT NOT NULL"
                          +")";
        using var command2 = new MySqlCommand(query, connection);
        command2.ExecuteNonQuery();
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
        var passwordHash=Auth.HashPassword(password);
        using var connection = new MySqlConnection(_login);
        connection.Open();
        string query = "INSERT INTO users (username, password) VALUES (@username, @password)";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password", passwordHash);
        try
        {
            command.ExecuteNonQuery();
            return RegisterResult.Success;
        }
        catch(Exception ex)
        {
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
        string query = "SELECT current_level FROM Progress WHERE id = (SELECT id FROM users WHERE username = @username)";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@username", username);
        var result = command.ExecuteScalar();
        if (result is not null)
        { 
            return Convert.ToInt32(result);
        }
        return 0;
    }
}