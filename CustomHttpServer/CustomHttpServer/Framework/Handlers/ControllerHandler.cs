using System.Net;
using System.Reflection;

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
            string[] strParams = context.Request.Url 
                .Segments 
                .Skip(2) 
                .Select(s => s.Replace("/", "")) 
                .ToArray(); 
 
            var assembly = Assembly.GetExecutingAssembly(); 
 
            var controller = assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(HttpController))).FirstOrDefault(c => c.Name.ToLower() == controllerName.ToLower()); 
 
            if (controller == null) return false; 
 
            var test = typeof(HttpController).Name; 
            var method = controller.GetMethods() 
                .FirstOrDefault(t => t.GetCustomAttributes(true) 
                    .Any(attr => attr.GetType().Name == $"Http{context.Request.HttpMethod}")); 
 
            if (method == null) return false; 
 
            object[] queryParams = method.GetParameters() 
                .Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType)) 
                .ToArray(); 
 
            var ret = method.Invoke(Activator.CreateInstance(controller), queryParams);
        }
        else if (Successor != null) 
        {
            Successor.HandleRequest(context);
        }
    }
}