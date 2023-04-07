using Server.Models;
using System.Net;
using System.Text.Json;
using Server.Contexts;

Dictionary<string, int> keyValuePairs = new();

var client = new HttpClient();
var listener = new HttpListener();

listener.Prefixes.Add("http://localhost:45678/");

listener.Start();

while (true)
{
    HttpListenerContext context = await listener.GetContextAsync();
    HttpListenerRequest request = context.Request;

    if (request == null) continue;

    switch (request.HttpMethod)
    {
        case "GET":
            {
                var response = context.Response;
                var key = request.QueryString["key"];
                ArgumentNullException.ThrowIfNull(key, nameof(key));

                if (!keyValuePairs.ContainsKey(key)) keyValuePairs[key] = 0;
                keyValuePairs[key]++;

                var dbContext = new KVDbContext();

                if (keyValuePairs[key] >= 3)
                {
                    var clientResponse = await client.GetAsync($"http://localhost:45678/?key={key}");
                    if (clientResponse.StatusCode == HttpStatusCode.OK)
                    {
                        response.ContentType = "application/json";
                        response.StatusCode = (int)HttpStatusCode.OK;

                        var jsonStr = await clientResponse.Content.ReadAsStringAsync();
                        var writer = new StreamWriter(response.OutputStream);
                        await writer.WriteAsync(jsonStr);
                        writer.Flush();
                    }
                    else
                    {
                        var kv = dbContext.Find<KeyValue>(key);

                        if (kv is not null)
                        {
                            response.ContentType = "application/json";
                            response.StatusCode = (int)HttpStatusCode.OK;

                            var keyValue = kv;
                            var jsonStr = JsonSerializer.Serialize(kv);
                            var writer = new StreamWriter(response.OutputStream);
                            await writer.WriteAsync(jsonStr);
                            writer.Flush();
                            var content = new StringContent(jsonStr);
                            await client.PostAsync("http://localhost:45678/", content);
                        }
                        else
                            response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                }
                else
                {
                    var temp = dbContext.Find<KeyValue>(key);

                    if (temp is not null)
                    {
                        response.ContentType = "application/json";
                        response.StatusCode = (int)HttpStatusCode.OK;

                        var keyValue = temp;
                        var jsonStr = JsonSerializer.Serialize(keyValue);
                        var writer = new StreamWriter(response.OutputStream);
                        await writer.WriteAsync(jsonStr);
                        writer.Flush();
                    }
                    else
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                }

                response.Close();
                break;
            }
        case "POST":
            {
                var stream = request.InputStream;
                var reader = new StreamReader(stream);
                var jsonStr = reader.ReadToEnd();
                var keyValue = JsonSerializer.Deserialize<KeyValue>(jsonStr);

                if (keyValue is not null)
                {
                    var response = context.Response;
                    var dbCon = new KVDbContext();
                    var key = keyValue.Key;

                    if (dbCon.Find<KeyValue>(key) == null)
                    {
                        dbCon.Add(keyValue);
                        dbCon.SaveChanges();
                        response.StatusCode = (int)HttpStatusCode.OK;
                    }
                    else
                        response.StatusCode = (int)HttpStatusCode.Found;

                    response.Close();
                }

                break;
            }
        case "PUT":
            {
                var stream = request.InputStream;
                var reader = new StreamReader(stream);
                var jsonStr = reader.ReadToEnd();

                Console.WriteLine(jsonStr);

                var keyValue = JsonSerializer.Deserialize<KeyValue>(jsonStr);
                var response = context.Response;
                var dbCon = new KVDbContext();
                var temp = dbCon.Find<KeyValue>(keyValue.Key);

                if (temp != null)
                {
                    temp.Value = keyValue.Value;
                    dbCon.SaveChanges();
                    response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                    response.StatusCode = (int)HttpStatusCode.NotFound;

                response.Close();
                break;
            }
    }
}