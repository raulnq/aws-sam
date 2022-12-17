namespace FunctionLibrary;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;

public class TasksRepository
{
    private AmazonDynamoDBClient _amazonDynamoDB;
    private string _tableName;

    public TasksRepository(AmazonDynamoDBClient amazonDynamoDB, string tableName)
    {
        _amazonDynamoDB = amazonDynamoDB;
        _tableName = tableName;
    }

    public System.Threading.Tasks.Task Save(Task task)
    {
        var putItemRequest = new PutItemRequest
        {
            TableName = _tableName,
            Item = new Dictionary<string, AttributeValue> {
                    {
                        "id",
                        new AttributeValue {
                        S = task.Id.ToString(),
                    }
                    },
                    {
                        "description",
                        new AttributeValue {
                        S = task.Description
                        }
                    },
                    {
                        "title",
                        new AttributeValue {
                        S = task.Title
                        }
                    }
                }
        };

        return _amazonDynamoDB.PutItemAsync(putItemRequest);
    }

    public async Task<Task?> Get(Guid id)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>() { { "id", new AttributeValue { S = id.ToString() } } },
        };

        var response = await _amazonDynamoDB.GetItemAsync(request);

        if(response.HttpStatusCode!= System.Net.HttpStatusCode.OK)
        {
            return null;
        }

        var task = new Task()
        {
            Id = Guid.Parse(response.Item["id"].S),
            Description = response.Item["description"].S,
            Title = response.Item["title"].S
        };

        return task;
    }

    public async Task<Task[]> List()
    {
        var request = new ScanRequest()
        {
            TableName = _tableName
        };

        var response = await _amazonDynamoDB.ScanAsync(request);

        return response.Items.Select(item => new Task()
        {
            Id = Guid.Parse(item["id"].S),
            Description = item["description"].S,
            Title = item["title"].S
        }).ToArray();
    }
}