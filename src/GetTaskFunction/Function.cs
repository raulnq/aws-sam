using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using FunctionLibrary;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GetTaskFunction;

public class Function
{
    private static TasksRepository _tasksRepository;

    static Function()
    {
        var tableName = Environment.GetEnvironmentVariable("TABLE_NAME");
        _tasksRepository = new TasksRepository(new AmazonDynamoDBClient(), tableName);
    }

    private static async System.Threading.Tasks.Task Main()
    {
        Func<APIGatewayHttpApiV2ProxyRequest, ILambdaContext, Task<APIGatewayHttpApiV2ProxyResponse>> handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<LambdaFunctionJsonSerializerContext>())
            .Build()
            .RunAsync();
    }

    public static async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest input, ILambdaContext context)
    {
        var id = input.PathParameters["id"];

        var task = await _tasksRepository.Get(Guid.Parse(id));

        if (task == null)
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 404
            };
        }

        var body = JsonSerializer.Serialize(task, LambdaFunctionJsonSerializerContext.Default.Task);

        return new APIGatewayHttpApiV2ProxyResponse
        {
            Body = body,
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}

[JsonSerializable(typeof(FunctionLibrary.Task))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
}