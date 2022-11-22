using Inovatiqa.Database.Models;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Orders
{
    public partial class PlaceOrderResult
    {
        public PlaceOrderResult()
        {
            Errors = new List<string>();
        }

        public bool Success => !Errors.Any();

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public IList<string> Errors { get; set; }

        public Order PlacedOrder { get; set; }
    }
}