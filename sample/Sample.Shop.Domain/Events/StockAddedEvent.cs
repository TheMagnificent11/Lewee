﻿using Saji.Domain;

namespace Sample.Shop.Domain.Events;

public class StockAddedEvent : IDomainEvent
{
    public StockAddedEvent(Guid correlationId, Guid productId, int quantity, int newStockLevel)
    {
        this.CorrelationId = correlationId;
        this.ProductId = productId;
        this.Quantity = quantity;
        this.NewStockLevel = newStockLevel;
    }

    public Guid CorrelationId { get; }
    public Guid ProductId { get; }
    public int Quantity { get; }
    public int NewStockLevel { get; }
}
