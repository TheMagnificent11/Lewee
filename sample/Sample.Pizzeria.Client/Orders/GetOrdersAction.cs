﻿using Lewee.Blazor.Fluxor.Actions;

namespace Sample.Pizzeria.Client.Orders;

public record GetOrdersAction : IRequestAction
{
    public GetOrdersAction()
    {
        this.CorrelationId = Guid.NewGuid();
    }

    public Guid CorrelationId { get; }
}