using CustomHttpServer.Attributes;

namespace CustomHttpServer.Controllers;

[Controller("auth")]
public class AuthController
{
    // GET: /auth/login
    [Get("login")]
    public void Login()
    {
        // отдавть Login html
    }
    
    // POST: /auth/login
    [Post("login")]
    public void Login(string username, string password)
    {
        Console.WriteLine($"Username: {username}, Password: {password}");
    }
    
}