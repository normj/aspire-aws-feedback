using ZipCodes.DataLoader;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();
builder.Services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();

var host = builder.Build();
host.Run();
