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
        string path = request.Url.LocalPath;
        bool isFile = path.Contains(".");

        if (isFile)
        {
            // Логика выбора метода контроллера
            // controllerName - получаем от клиента(/controller/method(/auth/login))
            // Auth это класс условно. А login  метод в этом классе
            // Иметь возможность добавить новый контроллер в папку Controllers и они должны автоматом подключиться с помощью рефлексии и кода ниже(что-то лишнее возмжожно)
            // Забираем string params если они есть и делаем как параметр дальше создаем рефлексию говорим что у нас есть решение и мы хотим работать забираем все его типы
            // ищем в типах типы у которы аттрибут имеет httpcontroller attribute дальше ищем первого у которого имя совпадает с controllername to lower(имя самого аттрибута который в нем зашит
            // controller name надо где-то взять где-то должно быть переменной(из пути) если нет контроллера то должны что-то вернуть(false) если нашли контроллер то мы должны вызвать метод
            // получаем от контроллера все его методы ищем по custom аттрибут и ищем по типу имени у аттрибута http attribute(http method(get,post)) забирем метод если не пустой мы
            // получаем его параметры(если есть какие-то файлы мы их передаем), queryparams мы преобразуем параметры в объект(массив ключ значение) и вызываем сам метод Instance создаем instance этого контролера
            // те параметры которые в логине приходили, contenttype приходящего request form
            string[] segments = context.Request.Url 
                .Segments 
                .Select(s => s.Replace("/", "")) 
                .ToArray();
            string controllerRoute = segments[0];
            string methodRoute = segments[1];
            string attributeName = $"{context.Request.HttpMethod[0]}{context.Request.HttpMethod[1..].ToLower()}Attribute";
            var assembly = Assembly.GetExecutingAssembly(); 
 
            var controller = assembly.GetTypes()
                .Where(t =>
                    t.GetCustomAttribute<ControllerAttribute>() != null)
                .FirstOrDefault(c =>
                    c.GetCustomAttribute<ControllerAttribute>()!.Route == controllerRoute);

            var test = typeof(ControllerAttribute).Name; 
            var method = controller.GetMethods()
                .Where(t => t.GetCustomAttributes(true)
                    .Any(attr => attr.GetType().Name == attributeName))
                .FirstOrDefault(c=>c.GetCustomAttribute<GetAttribute>()!.Route==methodRoute);
                
            
 
            object[] queryParams = method.GetParameters() 
                .Select((p, i) => Convert.ChangeType(segments[i], p.ParameterType)) 
                .ToArray(); 
 
            var ret = method.Invoke(Activator.CreateInstance(controller), queryParams);
        }
        else if (Successor != null) 
        {
            Successor.HandleRequest(context);
        }
    }
}