using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Services.Orders
{
    public class UpdateOrderParameters
    {
        #region Ctor

        public UpdateOrderParameters(Order updatedOrder, OrderItem updatedOrderItem)
        {
            if (updatedOrder is null)
                throw new ArgumentNullException(nameof(updatedOrder));

            if (updatedOrderItem is null)
                throw new ArgumentNullException(nameof(updatedOrderItem));

            UpdatedOrder = updatedOrder;
            UpdatedOrderItem = updatedOrderItem;
        }

        #endregion

        public Order UpdatedOrder { get; protected set; }

        public OrderItem UpdatedOrderItem { get; protected set; }

        public decimal PriceInclTax { get; set; }

        public decimal PriceExclTax { get; set; }

        public int Quantity { get; set; }

        public decimal DiscountAmountInclTax { get; set; }

        public decimal DiscountAmountExclTax { get; set; }

        public decimal SubTotalInclTax { get; set; }

        public decimal SubTotalExclTax { get; set; }

        public List<string> Warnings { get; } = new List<string>();

        public List<Discount> AppliedDiscounts { get; } = new List<Discount>();

        public PickupPoint PickupPoint { get; set; }
    }
}