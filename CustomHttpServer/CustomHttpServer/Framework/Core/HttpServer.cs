using System.Net;
using CustomHttpServer.Framework.Handlers;
using CustomHttpServer.Helpers;

public class HttpServer
{
    private readonly ConfigurationManager _settings;
    private readonly HttpListener _listener;

    public HttpServer(ConfigurationManager settings)
    {
        _settings = settings;
        _listener = new HttpListener();
        string prefixes = $"http://{_settings.Server.Host}:{_settings.Server.Port}{_settings.Server.Path}";
        _listener.Prefixes.Add(prefixes);
    }

    public Task StartAsync()
    {
        _listener.Start();
        return ListenAsync();
    }

    public void Stop()
    {
        _listener.Stop();
    }

    private async Task ListenAsync()
    {
        try
        {
            while (_listener.IsListening)
            {
                HttpListenerContext context = await _listener.GetContextAsync();
                await ProcessRequestAsync(context);
            }
        }
        catch (HttpListenerException)
        {
            Console.WriteLine("Сервер остановлен");
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        Handler h1 = new StaticFilesHandler();
        //Handler h2 = ();
        //h1.Successor = h2;
        h1.HandleRequest(context);
        Console.WriteLine($"Обработан запрос: {context.Request.Url}");
    }
}