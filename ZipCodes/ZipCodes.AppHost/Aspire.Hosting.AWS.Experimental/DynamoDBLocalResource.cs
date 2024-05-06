using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.AWS.Experimental;

/// <summary>
/// Represents a DynamoDB local resource. This is a dev only resources and will not be written to the project's manifest.
/// </summary>
internal sealed class DynamoDBLocalResource(string name, bool disableDynamoDBLocalTelemetry) : ContainerResource(name), IDynamoDBLocalResource
{
    internal bool DisableDynamoDBLocalTelemetry { get; } = disableDynamoDBLocalTelemetry;
}