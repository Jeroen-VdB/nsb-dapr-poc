using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using NServiceBus;
using NServiceBus.Configuration.AdvancedExtensibility;
using NServiceBus.MessageInterfaces;
using NServiceBus.Serialization;
using NServiceBus.Settings;

class CloudEventsSerializerDefinition : SerializationDefinition
{
    public const string Key = "CloudEventsSerializer.Settings";

    public override Func<IMessageMapper, IMessageSerializer> Configure(IReadOnlySettings settings)
    {
        return mapper =>
        {
            var value = settings.GetOrDefault<string>(Key);
            return new CloudEventsSerializer(value);
        };
    }
}

class CloudEventsSerializer :
    IMessageSerializer
{
    private readonly CloudEventFormatter _formatter;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private const string MessageTypeAttribute = "messagetype";

    public CloudEventsSerializer(string settingsValue)
    {
        _formatter = new JsonEventFormatter();
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        // Add code initializing serializer on the basis of settingsValue
    }

    public string ContentType { get; set; } = ContentTypes.Json;

    public void Serialize(object message, Stream stream)
    {
        // Add code to serialize message
        throw new NotImplementedException();
    }

    public object[] Deserialize(ReadOnlyMemory<byte> body, IList<Type> messageTypes = null)
    {
        if (body.IsEmpty)
        {
            return Array.Empty<object>();
        }

        var cloudEvent = _formatter.DecodeStructuredModeMessage(body, null, null);

        if (cloudEvent == null)
        {
            throw new Exception("Failed to deserialize CloudEvent");
        }

        Type messageType = messageTypes != null && messageTypes.Any()
            ? messageTypes.First()
            : GetTypeFromAttribute(cloudEvent, messageTypes);

        // Deserialize the data
        var message = cloudEvent.Data;

        // CloudEvents SDK might return the data as a JsonElement or string depending on the formatter
        // You might need to handle the conversion here based on your needs
        if (message is JsonElement jsonElement)
        {
            var jsonString = jsonElement.GetRawText();
            message = JsonSerializer.Deserialize(jsonString, messageType, _jsonSerializerOptions);
        }
        else if (message is string jsonString)
        {
            message = JsonSerializer.Deserialize(jsonString, messageType, _jsonSerializerOptions);
        }

        return [message];
    }

    private Type GetTypeFromAttribute(CloudEvent cloudEvent, IList<Type> messageTypes)
    {
        // Try to get the message type from extension attributes first, then fall back to Type attribute
        var messageTypeName = cloudEvent.GetAttribute(MessageTypeAttribute)?.ToString()
                              ?? cloudEvent.Type;

        Type messageType = null;

        // Try to find the type from the provided types first
        if (messageTypes != null && messageTypes.Any())
        {
            messageType = messageTypes.FirstOrDefault(t =>
                t.FullName == cloudEvent.Type ||
                t.AssemblyQualifiedName == messageTypeName);
        }

        // If not found, try to resolve it
        if (messageType == null && !string.IsNullOrEmpty(messageTypeName))
        {
            messageType = Type.GetType(messageTypeName);
        }

        if (messageType == null)
        {
            throw new Exception($"Could not determine message type from CloudEvent. Type: {cloudEvent.Type}");
        }

        return messageType;
    }
}

static class CloudEventsSerializerConfigurationExtensions
    {
        public static void Settings(this SerializationExtensions<CloudEventsSerializerDefinition> config, string value)
        {
            var settingsHolder = config.GetSettings();
            settingsHolder.Set(CloudEventsSerializerDefinition.Key, value);
        }
    }