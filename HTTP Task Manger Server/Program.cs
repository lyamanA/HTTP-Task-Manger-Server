using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;

var listener = new HttpListener();
listener.Prefixes.Add(@"http://localhost:27001/");
listener.Start();

Console.WriteLine("Server started. Listening for connections...");

while (true)
{
    HttpListenerContext context = listener.GetContext();
    HttpListenerRequest request = context.Request;
    HttpListenerResponse response = context.Response;

    try
    {
        switch (request.HttpMethod)
        {
            case "GET":
                var processes = Process.GetProcesses().Select(p => p.ProcessName).ToList();
                string responseJson = JsonSerializer.Serialize(processes);

                byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
                response.ContentLength64 = buffer.Length;
                response.ContentType = "application/json";

                using (var output = response.OutputStream)
                {
                    output.Write(buffer, 0, buffer.Length);
                }
                Console.WriteLine("Process list sent to client.");
                break;

            case "POST":
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string taskName = await reader.ReadToEndAsync();
                    Process.Start(taskName);
                    Console.WriteLine($"Task '{taskName}' started.");
                }
                response.StatusCode = (int)HttpStatusCode.OK;
                break;

            case "DELETE":

                try
                {
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        var requestBody = reader.ReadToEnd();
                        var processName = JsonSerializer.Deserialize<string>(requestBody);
                        var processList = Process.GetProcessesByName(processName);

                        if (processList.Length == 0)
                        {
                            throw new Exception($"Process {processName} was not found.");
                        }

                        foreach (var process in processList)
                        {
                            try
                            {
                                process.Kill();
                                process.WaitForExit();
                                Console.WriteLine($"Process {processName} has ended");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error ending process {processName}: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error when trying to terminate the process: {ex.Message}");
                }
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        response.StatusCode = (int)HttpStatusCode.InternalServerError;
    }
    finally
    {
        response.Close();
    }
}


