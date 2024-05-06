
// Configure what DynamoDB table to use
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using ZipCode.Model;

//var zipCodeTableName = Environment.GetEnvironmentVariable("ZIP_CODE_TABLE");
//if (string.IsNullOrEmpty(zipCodeTableName))
//{
//    zipCodeTableName = "ZipCodes";
//}



var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add AWS Services to DI
builder.Services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();



var app = builder.Build();

var zipCodeTableName = app.Configuration["AWS:Resources:ZipCodesTable"];
var zipCodeStateIndexName = "State-index";
var zipCodeCityIndexName = "City-index";

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

// Simple HealthCheck. Ideally should check dependency resources
app.MapGet("/api/healthcheck", () => true);

// Get by Zip Code
app.MapGet("/api/lookup/code/{code}", async ([FromServices] IAmazonDynamoDB ddbClient, string code) =>
{
    try
    {
        var response = await ddbClient.GetItemAsync(new GetItemRequest
        {
            TableName = zipCodeTableName,
            Key = new Dictionary<string, AttributeValue>
                    {
                        {"Code", new AttributeValue{S = code} }
                    }
        });

        if (!response.Item.Any())
        {
            return Results.NotFound();
        }

        return Results.Ok(ConvertItemToDTO(response.Item));
    }
    catch (Amazon.DynamoDBv2.Model.ResourceNotFoundException)
    {
        return Results.NotFound();
    }
});

// Get zip codes for a state
app.MapGet("/api/lookup/state/{state}", async ([FromServices] IAmazonDynamoDB ddbClient, string state) =>
{
    var response = await ddbClient.QueryAsync(new QueryRequest
    {
        TableName = zipCodeTableName,
        IndexName = zipCodeStateIndexName,
        KeyConditionExpression = "#S = :s",
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":s", new AttributeValue{S=state} }
            },
        ExpressionAttributeNames = new Dictionary<string, string>
            {
                {"#S", "State"}
            }
    });

    return response.Items.Select(x => ConvertItemToDTO(x));
});

// Get zip codes for a city
app.MapGet("/api/lookup/city/{city}", async ([FromServices] IAmazonDynamoDB ddbClient, string city) =>
{
    var response = await ddbClient.QueryAsync(new QueryRequest
    {
        TableName = zipCodeTableName,
        IndexName = zipCodeCityIndexName,
        KeyConditionExpression = "#S = :s",
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":s", new AttributeValue{S=city} }
            },
        ExpressionAttributeNames = new Dictionary<string, string>
            {
                {"#S", "City"}
            }
    });

    return response.Items.Select(x => ConvertItemToDTO(x));
});

app.MapGet("/", () => "US ZIP code lookup API");


app.Run();



app.Run();

ZipCodeEntry ConvertItemToDTO(IDictionary<string, AttributeValue> item)
{
    return new ZipCodeEntry
    {
        Code = item["Code"].S,
        City = item["City"].S,
        State = item["State"].S,
        Latitude = double.Parse(item["Latitude"].N),
        Longitude = double.Parse(item["Longitude"].N),
    };
}