﻿using Lewee.Domain;

namespace Sample.Restaurant.Domain;

public class Order : Entity
{
    private readonly List<OrderItem> items = new();

    private Order(Table table)
        : base(Guid.Empty)
    {
        this.TableId = table.Id;
        this.Table = table;
        this.OrderStatusId = Domain.OrderStatus.Ordering;
    }

    // EF Constructor
    private Order()
    {
        this.TableId = Guid.Empty;
        this.Table = new Table(this.TableId, 0);
    }

    public Guid TableId { get; protected set; }
    public Table Table { get; protected set; }
    public OrderStatus OrderStatusId { get; protected set; }
    public EnumEntity<OrderStatus>? OrderStatus { get; protected set; } // Not really nullable, but this is difficult to set for EF
    public IReadOnlyCollection<OrderItem> Items => this.items;
    public decimal Total => this.items.Sum(x => x.ItemTotal);

    internal static Order EmptyOrder => new(Table.EmptyTable);

    public static Order StartNewOrder(Table table)
    {
        return new Order(table);
    }

    internal void AddItem(MenuItem menuItem)
    {
        var existingMenuItem = this.items.FirstOrDefault(x => x.MenuItemId == menuItem.Id);

        if (existingMenuItem == null)
        {
            this.items.Add(OrderItem.AddToOrder(this, menuItem));
            return;
        }

        existingMenuItem.IncreaseQuantity();
    }

    internal void RemoveItem(MenuItem menuItem)
    {
        var existingMenuItem = this.items.FirstOrDefault(x => x.MenuItemId == menuItem.Id);

        if (existingMenuItem == null)
        {
            return;
        }

        existingMenuItem.ReduceQuantity();

        if (existingMenuItem.Quantity > 0)
        {
            return;
        }

        existingMenuItem.Delete();
    }
}
