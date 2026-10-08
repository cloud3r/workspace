using System.Net;
using System.Reflection;
using CustomHttpServer.Attributes;

namespace CustomHttpServer.Framework.Handlers;

public class ControllerHandler : Handler
{
    public async override Task HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        Console.WriteLine($"Method: {request.HttpMethod}");
        Console.WriteLine($"ContentType: {request.ContentType}");
        Console.WriteLine($"ContentLength: {request.ContentLength64}");

        try
        {
            bool isFile = request.Url!.LocalPath.Contains('.');

            if (isFile)
            {
                if (Successor != null) await Successor.HandleRequest(context);
                else await Send(response, 404, "Not found");
                return;
            }

            string[] segments = request.Url.Segments
                .Select(s => s.Trim('/'))
                .Where(s => s.Length > 0)
                .ToArray();

            if (segments.Length < 2)
            {
                await Send(response, 404, "Not found");
                return;
            }

            string controllerRoute = segments[0];
            string methodRoute = segments[1];
            string attributeName =
                $"{request.HttpMethod[0]}{request.HttpMethod[1..].ToLower()}Attribute";

            var controller = Assembly.GetExecutingAssembly().GetTypes()
                .FirstOrDefault(t => t.GetCustomAttribute<ControllerAttribute>()?.Route == controllerRoute);

            if (controller == null)
            {
                await Send(response, 404, "Controller not found");
                return;
            }

            var method = controller.GetMethods()
                .FirstOrDefault(m => m.GetCustomAttributes(true)
                    .Any(a => a.GetType().Name == attributeName
                              && (a as dynamic).Route == methodRoute));

            if (method == null)
            {
                await Send(response, 404, "Method not found");
                return;
            }

            var parameters = method.GetParameters();
            var args = segments.Skip(2).ToArray();

            if (args.Length != parameters.Length)
            {
                await Send(response, 400, "Bad request");
                return;
            }

            object?[] queryParams = parameters
                .Select((p, i) => Convert.ChangeType(args[i], p.ParameterType))
                .ToArray();

            var ret = method.Invoke(Activator.CreateInstance(controller), queryParams);

            if (ret is Task t)
            {
                await t;
                ret = t.GetType().IsGenericType ? ((dynamic)t).Result : null;
            }

            await Send(response, 200, ret?.ToString() ?? "");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            try
            {
                await Send(response, 500, "Internal server error");
            }
            catch
            {
            }
        }

        static async Task Send(HttpListenerResponse response, int status, string body)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(body);
            response.StatusCode = status;
            response.ContentType = "text/plain; charset=utf-8";
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes);
            response.Close();
        }
    }
}
