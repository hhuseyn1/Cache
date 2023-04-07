using Cache.Models;
using System.Net;
using System.Text.Json;

HttpListener listener = new HttpListener();
listener.Prefixes.Add("http://localhost:45678");
listener.Start();

List<KeyValue> keyValues = new();

while (true)
{
    HttpListenerContext context = await listener.GetContextAsync();
    HttpListenerRequest request = context.Request;

    if (request is not null)
    {
        switch (request.HttpMethod)
        {
            case "GET":
                {
                    HttpListenerResponse response = context.Response;

                    var key = request.QueryString["key"];

                    KeyValue? kv = keyValues.FirstOrDefault(kv => kv.Key == key);

                    if (kv is not null)
                    {
                        response.ContentType = "application/json";
                        response.StatusCode = (int)HttpStatusCode.OK;

                        KeyValue? keyValue = kv;

                        var jsonStr = JsonSerializer.Serialize(keyValue);
                        var writer = new StreamWriter(response.OutputStream);
                        await writer.WriteAsync(jsonStr);
                        writer.Flush();
                    }
                    else
                        response.StatusCode = (int)HttpStatusCode.NotFound;

                    response.Close();
                    break;
                }
            case "POST":
                {
                    HttpListenerResponse response = context.Response;

                    var stream = request.InputStream;
                    var reader = new StreamReader(stream);
                    var jsonStr = reader.ReadToEnd();

                    KeyValue? keyValue = JsonSerializer.Deserialize<KeyValue>(jsonStr);

                    if (keyValue is not null)
                    {
                        keyValues.Add(keyValue);
                        response.StatusCode = (int)HttpStatusCode.OK;
                    }
                    else
                        response.StatusCode = (int)HttpStatusCode.BadRequest;

                    response.Close();
                    break;
                }
            case "PUT":
                {
                    HttpListenerResponse response = context.Response;

                    var stream = request.InputStream;
                    var reader = new StreamReader(stream);
                    var jsonStr = reader.ReadToEnd();

                    KeyValue? temp = JsonSerializer.Deserialize<KeyValue>(jsonStr);
                    KeyValue? keyValue = keyValues.Find(kv => kv.Key == temp?.Key);

                    keyValue.Value = temp.Value;
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.Close();
                    break;
                }
        }
    }
}