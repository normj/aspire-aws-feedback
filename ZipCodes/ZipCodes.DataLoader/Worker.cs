using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace ZipCodes.DataLoader;

public class Worker(ILogger<Worker> logger, IAmazonDynamoDB ddbClient, IConfiguration configuration) : BackgroundService
{
    const int MAX_BATCH_WRITE_SIZE = 25;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DDB Endpoint: {endpoint}", ddbClient.DetermineServiceOperationEndpoint(new ListTablesRequest()).URL);

        var zipCodeTableName = configuration["AWS:Resources:ZipCodesTable"];

        if (string.IsNullOrEmpty(zipCodeTableName))
        {
            throw new Exception("Table name is not configured for loading data");
        }

        await EnsureTableExistsAsync(zipCodeTableName, stoppingToken);

        var batchCalls = 0;
        var putCount = 0;
        var fileContents = File.ReadAllLines("zip_codes_states-all.csv").Skip(1);

        var writeRequests = new List<WriteRequest>();
        var batchWriteRequest = new BatchWriteItemRequest
        {
            RequestItems = new Dictionary<string, List<WriteRequest>>
            {
                {zipCodeTableName, writeRequests }
            }
        };

        foreach (var line in fileContents)
        {
            var tokens = line.Split(',').Select(x => x.Replace("\"", "")).ToArray();
            if (tokens.Length == 6)
            {
                if (string.IsNullOrEmpty(tokens[1]) || string.IsNullOrEmpty(tokens[2]))
                    continue;

                writeRequests.Add(new WriteRequest
                {
                    PutRequest = new PutRequest
                    {
                        Item = new Dictionary<string, AttributeValue>
                        {
                            {"Code", new AttributeValue{S = tokens[0] } },
                            {"Latitude", new AttributeValue{N = tokens[1] } },
                            {"Longitude", new AttributeValue{N = tokens[2] } },
                            {"City", new AttributeValue{S = tokens[3] } },
                            {"State", new AttributeValue{S = tokens[4] } },
                            {"Country", new AttributeValue{S = tokens[5] } }
                        }
                    }
                });

                if (writeRequests.Count == MAX_BATCH_WRITE_SIZE)
                {
                    await ddbClient.BatchWriteItemAsync(batchWriteRequest);
                    batchCalls++;
                    putCount += writeRequests.Count;
                    writeRequests.Clear();

                    if (batchCalls % 5 == 0)
                    {
                        logger.LogInformation("... Loaded {count} items", putCount);
                    }
                }
            }
        }

        if (writeRequests.Count > 0)
        {
            await ddbClient.BatchWriteItemAsync(batchWriteRequest);
            putCount += writeRequests.Count;
        }

        logger.LogInformation("Data loader complete with {count} items", putCount);
    }

    private async Task EnsureTableExistsAsync(string tableName, CancellationToken token)
    {
        if ((await ddbClient.ListTablesAsync()).TableNames.Contains(tableName))
        {
            return;
        }

        await ddbClient.CreateTableAsync(new CreateTableRequest
        {
            TableName = tableName,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement{AttributeName = "Code", KeyType = KeyType.HASH}
            },
            GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
            {
                new GlobalSecondaryIndex
                {
                    IndexName = "City-index",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement{AttributeName = "City", KeyType = KeyType.HASH}
                    },
                    Projection = new Projection{ProjectionType = ProjectionType.ALL},
                },
                new GlobalSecondaryIndex
                {
                    IndexName = "State-index",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement{AttributeName = "State", KeyType = KeyType.HASH}
                    },
                    Projection = new Projection{ProjectionType = ProjectionType.ALL},
                }
            },
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition{AttributeName = "Code", AttributeType = ScalarAttributeType.S },
                new AttributeDefinition{AttributeName = "City", AttributeType = ScalarAttributeType.S },
                new AttributeDefinition{AttributeName = "State", AttributeType = ScalarAttributeType.S },
            }
        }, token);

        DescribeTableResponse describeResponse;
        do
        {
            await Task.Delay(1000);
            describeResponse = await ddbClient.DescribeTableAsync(tableName);
        } while (describeResponse.Table.TableStatus != TableStatus.ACTIVE);
    }

}
