﻿using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.AWS.Experimental;


namespace Aspire.Hosting;

public static class DynamoDBLocalResourceBuilderExtensions
{
    /// <summary>
    /// Add an instance of DynamoDB local. This is a container pulled from Amazon ECR public gallery.
    /// Projects that use DynamoDB local can get a reference to the instance using the WithAWSDynamoDBLocalReference method.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="image">Optional: default value is "public.ecr.aws/aws-dynamodb-local/aws-dynamodb-local"</param>
    /// <param name="tag">Optional: default value is "latest"</param>
    /// <param name="disableDynamoDBLocalTelemetry">Optional: default to false. It controls setting the DynamoDB local DDB_LOCAL_TELEMETRY environment variable.</param>
    /// <returns></returns>
    /// <exception cref="DistributedApplicationException"></exception>
    public static IResourceBuilder<IDynamoDBLocalResource> AddAWSDynamoDBLocal(this IDistributedApplicationBuilder builder,
        string name, string image = "public.ecr.aws/aws-dynamodb-local/aws-dynamodb-local", string tag = "latest", bool disableDynamoDBLocalTelemetry = false)
    {
        var container = new DynamoDBLocalResource(name, disableDynamoDBLocalTelemetry);
        var containerBuilder = builder.AddResource(container)
                  .ExcludeFromManifest()
                  .WithEndpoint(targetPort:8000, scheme:"http")
                  .WithAnnotation(new ContainerImageAnnotation { Image = image, Tag = tag });

        // Repurpose the WithEnvironment to invoke the users callback to seed DynamoDB local.
        // This needs a better mechanism to have a callback once a container has been started
        // so a hosting component can do initial configuration or seeding in the container.
        containerBuilder.WithEnvironment(context =>
        {
            if (context.ExecutionContext.IsPublishMode)
            {
                return;
            }

            if (container.DisableDynamoDBLocalTelemetry)
            {
                // Info on DynamoDB Local telemetry
                // https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocalTelemetry.html
                context.EnvironmentVariables.Add("DDB_LOCAL_TELEMETRY", "0");
            }
        });

        return containerBuilder;
    }

    /// <summary>
    /// Add a reference to the DynamoDB local to the project. This is done by setting the AWS_ENDPOINT_URL_DYNAMODB environment
    /// variable for the project to the http endpoint of the DynamoDB local container. Any DynamoDB service clients
    /// created in the project relying on endpoint resolution will pick up this environment variable and use it.
    /// </summary>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="builder"></param>
    /// <param name="dynamoDBLocalResourceBuilder"></param>
    /// <returns></returns>
    /// <exception cref="DistributedApplicationException"></exception>
    public static IResourceBuilder<TDestination> WithReference<TDestination>(this IResourceBuilder<TDestination> builder, IResourceBuilder<IDynamoDBLocalResource> dynamoDBLocalResourceBuilder)
        where TDestination : IResourceWithEnvironment
    {
        builder.WithEnvironment(context =>
        {
            if (context.ExecutionContext.IsPublishMode)
            {
                return;
            }

            var endpoint = dynamoDBLocalResourceBuilder.Resource.GetEndpoints().First();
            context.EnvironmentVariables.Add("AWS_ENDPOINT_URL_DYNAMODB", endpoint.Url);
        });
        return builder;
    }
}