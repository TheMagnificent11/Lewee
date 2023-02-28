﻿using System.Reflection;
using System.Text.Json;
using Lewee.Shared;
using Microsoft.Extensions.Logging;

namespace Lewee.Blazor.Events;

internal class MessageDeserializer
{
    private const string CouldNotDeserializeError = "Could not deserialize message '{Message}'";
    private const string CouldNotFindContractError =
        "Could not find JSON contract type for message (Message: {Message}, Contract Assembly: {ContractAssembly}, Contract Class: {ContractClass})";

    private readonly ILogger<MessageDeserializer> logger;

    public MessageDeserializer(ILogger<MessageDeserializer> logger)
    {
        this.logger = logger;
    }

    public object? Deserialize(string message)
    {
        try
        {
            var clientMessage = JsonSerializer.Deserialize<ClientMessge>(message);
            if (clientMessage == null)
            {
                this.logger.LogError(CouldNotDeserializeError, message);
                return null;
            }

            var assembly = Assembly.Load(clientMessage.ContractAssemblyName);
            var targetType = assembly.GetType(clientMessage.ContractFullClassName);
            if (targetType == null)
            {
                this.logger.LogError(
                    CouldNotFindContractError,
                    message,
                    clientMessage.ContractAssemblyName,
                    clientMessage.ContractFullClassName);

                return null;
            }

            var obj = JsonSerializer.Deserialize(clientMessage.MessageJson, targetType);
            if (obj == null)
            {
                this.logger.LogError(
                    CouldNotFindContractError,
                    message,
                    clientMessage.ContractAssemblyName,
                    clientMessage.ContractFullClassName);
            }

            return obj;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, CouldNotDeserializeError, message);
            return null;
        }
    }
}
