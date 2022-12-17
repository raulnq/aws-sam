using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using FunctionLibrary;
using System.Text.Json;
using System.Text.Json.Serialization;
using Task = FunctionLibrary.Task;

namespace PostTaskFunction;

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
        var request = JsonSerializer.Deserialize(input.Body, LambdaFunctionJsonSerializerContext.Default.RegisterTaskRequest)!;

        var task = new Task() { Id = Guid.NewGuid(), Description = request.Description, Title = request.Title };

        await _tasksRepository.Save(task);

        var body = JsonSerializer.Serialize(new RegisterTaskResponse(task.Id), LambdaFunctionJsonSerializerContext.Default.RegisterTaskResponse);

        return new APIGatewayHttpApiV2ProxyResponse
        {
            Body = body,
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}

public record RegisterTaskRequest(string Description, string Title);

public record RegisterTaskResponse(Guid Id);

[JsonSerializable(typeof(RegisterTaskResponse))]
[JsonSerializable(typeof(RegisterTaskRequest))]
[JsonSerializable(typeof(Task))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
}