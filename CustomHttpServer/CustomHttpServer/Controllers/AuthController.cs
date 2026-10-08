using CustomHttpServer.Attributes;

namespace CustomHttpServer.Controllers;

[Controller("auth")]
public class AuthController
{
    // GET: /auth/login
    [Get("login")]
    public string Login()
    {
        return "Hello from AuthController!";
    }
    
    // POST: /auth/login
    [Post("login")]
    public string Login(string username, string password)
    {
        Console.WriteLine($"Username: {username}, Password: {password}");
        return "Ok";
    }
    
}