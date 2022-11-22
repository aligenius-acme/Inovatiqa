using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Messages.Interfaces
{
    public partial interface IMessageTokenProviderService
    {
        void AddCustomerTokens(IList<Token> tokens, int customerId);

        void AddCustomerTokens(IList<Token> tokens, Customer customer);

        void AddStoreTokens(IList<Token> tokens, EmailAccount emailAccount);

        void AddProductReviewTokens(IList<Token> tokens, ProductReview productReview);

        void AddOrderTokens(IList<Token> tokens, Order order, int languageId, int vendorId = 0);

        void AddReturnRequestTokens(IList<Token> tokens, ReturnRequest returnRequest, OrderItem orderItem);

        void AddOrderNoteTokens(IList<Token> tokens, OrderNote orderNote);

        void AddOrderRefundedTokens(IList<Token> tokens, Order order, decimal refundedAmount);

        void AddShipmentTokens(IList<Token> tokens, Shipment shipment, int languageId);
    }
}