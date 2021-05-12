using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Devices;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace C2D_Console
{
    class Program
    {                
        static ServiceClient serviceClient;
        static string deviceId;
        static string moduleId;

        static async Task Main(string[] args)
        {
            try
            {
                // Read app configuration
                IConfigurationRoot appSettings = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();             

                // Configure the IoT Hub Service client
                serviceClient = ServiceClient.CreateFromConnectionString(appSettings["IoThubConnectionString"]);
                deviceId = appSettings["deviceId"];
                moduleId = appSettings["moduleId"];                
    
                // Read operations json and deserialize it in to a dynamic object
                string operationsJson = File.ReadAllText("operations.json");
                dynamic operationsObject = JsonConvert.DeserializeObject(operationsJson);

                // Read  API version property
                JProperty apiVersionProperty = new JProperty("@apiVersion", operationsObject.apiVersion);                            

                // Loop through the operations
                foreach(var op in operationsObject.operations)
                {                 
                    string operationName = op.opName;
                    PrintMessage("\n--------------------------------------------------------------------------\n", ConsoleColor.Cyan);
                    Console.WriteLine("Executing operation " + operationName);

                    var operationParams = op.opParams;

                    switch(operationName)
                    {
                        case "pipelineTopologySet" :      
                            await ExecutePipelineTopologySetOperationAsync(operationParams, apiVersionProperty);
                            break;

                        case "pipelineTopologyList" :
                        case "livePipelineList" :
                        case "livePipelineActivate" :
                        case "livePipelineDeactivate" :
                        case "livePipelineDelete" :
                        case "livePipelineGet" :
                        case "livePipelineSet" :
                        case "pipelineTopologyDelete" :
                        case "pipelineTopologyGet" :
                            await ExecuteLivePipelineOperationAsync(operationParams, operationName, apiVersionProperty);
                            break;

                        case "WaitForInput" :                            
                            PrintMessage(operationParams.message.ToString(), ConsoleColor.Yellow);
                            Console.ReadLine();
                            break;

                        default :
                            PrintMessage("Unknown operation " + op.name, ConsoleColor.Red);                            
                            Console.WriteLine("Press Enter to continue");
                            Console.ReadLine();
                            break;
                    }

                }
            }
            catch(Exception ex)
            {
                PrintMessage(ex.ToString(), ConsoleColor.Red);
            }
        }             

        static void PrintMessage(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static async Task ExecutePipelineTopologySetOperationAsync(JObject operationParams, JProperty apiVersionProperty)
        {
            try
            {
                if (operationParams == null)
                {
                    PrintMessage("opParams object is missing", ConsoleColor.Red);
                    PrintMessage("Press Enter to continue", ConsoleColor.Yellow);
                    Console.ReadLine();
                }
                else
                {
                    if (operationParams["pipelineTopologyUrl"] != null)
                    {
                        // Download the pipelineTopology JSON and invoke pipelineTopologySet
                        string pipelineTopologyJson = await DownloadFromUrlAsync((string)operationParams["pipelineTopologyUrl"]);                        
                        await InvokeMethodWithPayloadAsync("pipelineTopologySet", pipelineTopologyJson);
                    }
                    else if (operationParams["pipelineTopologyFile"] != null)
                    {
                        // Read the pipelineTopology JSON from the file and invoke pipelineTopologySet
                        string pipelineTopologyJson = File.ReadAllText((string)operationParams["pipelineTopologyFile"]);                        
                        await InvokeMethodWithPayloadAsync("pipelineTopologySet", pipelineTopologyJson);
                    }
                    else
                    {
                        PrintMessage("Neither pipelineTopologyUrl nor pipelineTopologyFile specified", ConsoleColor.Red);
                        PrintMessage("Press Enter to continue", ConsoleColor.Yellow);
                        Console.ReadLine();
                    }
                }
            }
            catch(Exception ex)
            {
                PrintMessage(ex.ToString(), ConsoleColor.Red);
            }
        }

        static async Task ExecuteLivePipelineOperationAsync(JObject operationParams, string operationName, JProperty apiVersionProperty)
        {
            try
            {
                if (operationParams == null)
                {
                    PrintMessage("opParams object is missing", ConsoleColor.Red);
                    PrintMessage("Press Enter to continue", ConsoleColor.Yellow);
                    Console.ReadLine();
                }
                else
                {                
                    JObject avaObjectRequest = operationParams;
                    avaObjectRequest.AddFirst(apiVersionProperty);                                
                    await InvokeMethodWithPayloadAsync(operationName, avaObjectRequest.ToString());
                }
            }
            catch(Exception ex)
            {
                PrintMessage(ex.ToString(), ConsoleColor.Red);
            }
        }


        static async Task<string> DownloadFromUrlAsync(string url)   
        {
            string fileText = null;
            using (var httpClient = new HttpClient())
            {
                // Download the file
                using (var result = await httpClient.GetAsync(url))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        byte[] bytesArray = await result.Content.ReadAsByteArrayAsync();
                        fileText = Encoding.UTF8.GetString(bytesArray);                        
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Could not download from " + url);
                        Console.ResetColor();
                    }

                }
            }                

            return fileText;
        }

        static async Task InvokeMethodWithPayloadAsync(string methodName, string payload)
        {
            // Create a direct method call
            var methodInvocation = new CloudToDeviceMethod(methodName)
            { 
                ResponseTimeout = TimeSpan.FromSeconds(30)
            }
            .SetPayloadJson(payload);

            // Invoke the direct method asynchronously and get the response from the device.
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n-----------------------  Request: {methodName}  --------------------------------------------------\n");
            Console.ResetColor();
            Console.WriteLine(JToken.Parse(payload).ToString());            

            var response = await serviceClient.InvokeDeviceMethodAsync(deviceId, moduleId, methodInvocation);
            var responseString = response.GetPayloadAsJson();

            if (response.Status >= 400)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            
            Console.WriteLine($"\n---------------  Response: {methodName} - Status: {response.Status}  ---------------\n");
            Console.ResetColor();

            if (responseString != null)
            {
                Console.WriteLine(JToken.Parse(responseString).ToString());
            }
            else
            {
                Console.WriteLine(responseString);
            }
        }

    }
}
