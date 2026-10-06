using System.Net;
using CustomHttpServer.Helpers;

namespace CustomHttpServer.Framework.Handlers;

public class StaticFilesHandler : Handler
{
    public override async Task HandleRequest(HttpListenerContext context)
    {
        HttpListenerResponse response = context.Response;
        HttpListenerRequest request = context.Request;
        string path = request.Url.LocalPath;
        string filePath = Directory.GetCurrentDirectory() + $"/static{path}";
        
        if (Directory.Exists(filePath))
        {
            filePath = Path.Combine(filePath, "index.html");
            WriteResponseAsync(response, filePath);
        }
        
        bool isFile = path.Contains(".");
        
        if (isFile)
        {
            Console.WriteLine($"REQUEST: {context.Request.HttpMethod} {context.Request.Url}");

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                {
                    response.StatusCode = 404;
                    filePath = Directory.GetCurrentDirectory() + $"/static/404.html";
                }

                string contentType = MimeHandler.GetMimeType(filePath);
                byte[] buffer = await File.ReadAllBytesAsync(filePath);

                response.ContentLength64 = buffer.Length;
                response.ContentType = contentType;

                using Stream output = response.OutputStream;
                await output.WriteAsync(buffer, 0, buffer.Length);
                await output.FlushAsync();
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                await WriteResponseAsync(response, ex.Message);
            }
        }
        
        // передача запроса дальше по цепи при наличии в ней обработчиков
        else if (Successor != null)
        {
            Successor.HandleRequest(context);
        }
    }

    private static async Task WriteResponseAsync(HttpListenerResponse response, string content)
    {
        byte[] buffer = await File.ReadAllBytesAsync(content);
        response.ContentLength64 = buffer.Length;
        response.ContentType = MimeHandler.GetMimeType(content);
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.OutputStream.Flush();
    }
}