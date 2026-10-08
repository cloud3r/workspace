using CustomHttpServer.Attributes;

namespace CustomHttpServer.Controllers;

[Controller("test")]
public class TestController
{
    [Get("hello")]
    public string Hello()
    {
        return "Hello from TestController!";
    }
}