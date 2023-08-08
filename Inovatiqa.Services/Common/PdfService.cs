using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Services.Media.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Payments.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.Vendors.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Inovatiqa.Services.Common
{
    public partial class PdfService : IPdfService
    {
        #region Fields

        private readonly IAddressAttributeFormatterService _addressAttributeFormatterService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IDateTimeHelperService _dateTimeHelperService;
        private readonly IMeasureService _measureService;
        private readonly IInovatiqaFileProvider _fileProvider;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IShipmentService _shipmentService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContextService _workContextService;

        #endregion

        #region Ctor

        public PdfService(IAddressAttributeFormatterService addressAttributeFormatterService,
            IAddressService addressService,
            ICountryService countryService,
            IDateTimeHelperService dateTimeHelperService,
            IMeasureService measureService,
            IInovatiqaFileProvider fileProvider,
            IOrderService orderService,
            IPaymentService paymentService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IShipmentService shipmentService,
            IStateProvinceService stateProvinceService,
            IVendorService vendorService,
            IWorkContextService workContextService)
        {
            _countryService = countryService;
            _addressAttributeFormatterService = addressAttributeFormatterService;
            _addressService = addressService;
            _dateTimeHelperService = dateTimeHelperService;
            _measureService = measureService;
            _fileProvider = fileProvider;
            _orderService = orderService;
            _paymentService = paymentService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _shipmentService = shipmentService;
            _stateProvinceService = stateProvinceService;
            _vendorService = vendorService;
            _workContextService = workContextService;
        }

        #endregion

        #region Utilities

        protected virtual Font GetFont()
        {
            return GetFont(InovatiqaDefaults.FontFileName);
        }

        protected virtual Font GetFont(string fontFileName)
        {
            if (fontFileName == null)
                throw new ArgumentNullException(nameof(fontFileName));

            var fontPath = _fileProvider.Combine(_fileProvider.MapPath("~/App_Data/Pdf/"), fontFileName);
            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new Font(baseFont, 10, Font.NORMAL);
            return font;
        }

        protected virtual int GetDirection(Language lang)
        {
            return lang.Rtl ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR;
        }

        protected virtual int GetAlignment(Language lang, bool isOpposite = false)
        {
            if (!isOpposite)
                return lang.Rtl ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT;

            return lang.Rtl ? Element.ALIGN_LEFT : Element.ALIGN_RIGHT;
        }

        protected virtual PdfPCell GetPdfCell(string resourceKey, Language lang, Font font)
        {
            return new PdfPCell(new Phrase(resourceKey, font));
        }

        protected virtual PdfPCell GetPdfCell(object text, Font font)
        {
            return new PdfPCell(new Phrase(text.ToString(), font));
        }

        protected virtual Paragraph GetParagraph(string resourceKey, Language lang, Font font, params object[] args)
        {
            return GetParagraph(resourceKey, string.Empty, lang, font, args);
        }

        protected virtual Paragraph GetParagraph(string resourceKey, string indent, Language lang, Font font, params object[] args)
        {
            var formatText = resourceKey;
            return new Paragraph(indent + (args.Any() ? string.Format(formatText, args) : formatText), font);
        }

        protected virtual void PrintOrderNotes(Order order, Language lang, Font titleFont, Document doc, Font font)
        {
            if (!InovatiqaDefaults.RenderOrderNotes)
                return;

            var orderNotes = _orderService.GetOrderNotesByOrderId(order.Id, true)
                .OrderByDescending(on => on.CreatedOnUtc)
                .ToList();

            if (!orderNotes.Any())
                return;

            var notesHeader = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var cellOrderNote = GetPdfCell("Order notes:", lang, titleFont);
            cellOrderNote.Border = Rectangle.NO_BORDER;
            notesHeader.AddCell(cellOrderNote);
            doc.Add(notesHeader);
            doc.Add(new Paragraph(" "));

            var notesTable = new PdfPTable(2)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            notesTable.SetWidths(lang.Rtl ? new[] { 70, 30 } : new[] { 30, 70 });

            cellOrderNote = GetPdfCell("Created on", lang, font);
            cellOrderNote.BackgroundColor = BaseColor.LightGray;
            cellOrderNote.HorizontalAlignment = Element.ALIGN_CENTER;
            notesTable.AddCell(cellOrderNote);

            cellOrderNote = GetPdfCell("Note", lang, font);
            cellOrderNote.BackgroundColor = BaseColor.LightGray;
            cellOrderNote.HorizontalAlignment = Element.ALIGN_CENTER;
            notesTable.AddCell(cellOrderNote);

            foreach (var orderNote in orderNotes)
            {
                cellOrderNote = GetPdfCell(_dateTimeHelperService.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc), font);
                cellOrderNote.HorizontalAlignment = Element.ALIGN_LEFT;
                notesTable.AddCell(cellOrderNote);

                cellOrderNote = GetPdfCell(HtmlHelper.ConvertHtmlToPlainText(_orderService.FormatOrderNoteText(orderNote), true, true), font);
                cellOrderNote.HorizontalAlignment = Element.ALIGN_LEFT;
                notesTable.AddCell(cellOrderNote);

            }

            doc.Add(notesTable);
        }

        protected virtual void PrintTotals(int vendorId, Language lang, Order order, Font font, Font titleFont, Document doc, int shipmentId = 0)
        {
            if (vendorId != 0)
                return;

            var totalsTable = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            totalsTable.DefaultCell.Border = Rectangle.NO_BORDER;

            var languageId = lang.Id;

            var orderTotalPartialInclTax = 0.0m;
            var orderTotalPartialExclTax = 0.0m;
            if (shipmentId != 0)
            {
                var orderItems = _orderService.GetOrderItems(order.Id, vendorId: vendorId);

                foreach (var orderItem in orderItems)
                {
                    var shipmentItems = _shipmentService.GetShipmentItemsByShipmentId(shipmentId);
                    if (shipmentItems != null)
                    {
                        var item = shipmentItems.Where(x => x.OrderItemId == orderItem.Id).FirstOrDefault();
                        if (item == null)
                            continue;
                        else
                        {
                            orderItem.Quantity = item.Quantity;
                            orderTotalPartialInclTax += orderItem.Quantity * orderItem.UnitPriceInclTax;
                            orderTotalPartialExclTax += orderItem.Quantity * orderItem.UnitPriceExclTax;
                        }
                    }
                }
            }

            if (shipmentId != 0)
            {
                order.OrderSubtotalInclTax = orderTotalPartialInclTax;
                order.OrderSubtotalExclTax = orderTotalPartialExclTax;
            }

            if (order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax &&
                !InovatiqaDefaults.ForceTaxExclusionFromOrderSubtotal)
            {
                var orderSubtotalInclTaxInCustomerCurrency = order.OrderSubtotalInclTax;
                var orderSubtotalInclTaxStr = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency);

                var p = GetPdfCell($"{"Sub-total:"} {orderSubtotalInclTaxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            if (order.ShippingStatusId != (int)ShippingStatus.ShippingNotRequired)
            {
                if (order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax)
                {
                    var orderShippingInclTaxInCustomerCurrency = order.OrderShippingInclTax;

                    var shipment = _shipmentService.GetShipmentById(shipmentId);
                    var shipmentItems = _shipmentService.GetShipmentItemsByShipmentId(shipmentId);

                    int? firstShipmentId = _shipmentService.GetShipmentsByOrderId(order.Id).OrderBy(s => s.Id).FirstOrDefault()?.Id;

                    var orderShippingInclTaxStr = _priceFormatter.FormatPrice(orderShippingInclTaxInCustomerCurrency);

                    if (shipmentId != 0)
                    {
                        order.OrderSubtotalInclTax += orderShippingInclTaxInCustomerCurrency;
                    }
                    
                    if(firstShipmentId == shipment.Id)
                    {
                        var p = GetPdfCell($"{"Shipping:"} {orderShippingInclTaxStr}", font);
                        p.HorizontalAlignment = Element.ALIGN_RIGHT;
                        p.Border = Rectangle.NO_BORDER;
                        totalsTable.AddCell(p);
                    }
                    else
                    {
                        var p = GetPdfCell($"{"Shipping:"} {_priceFormatter.FormatPrice(0)}", font);
                        p.HorizontalAlignment = Element.ALIGN_RIGHT;
                        p.Border = Rectangle.NO_BORDER;
                        totalsTable.AddCell(p);
                    }
                }
                else
                {
                    var orderShippingExclTaxInCustomerCurrency = order.OrderShippingExclTax;
                    var orderShippingExclTaxStr = _priceFormatter.FormatPrice(orderShippingExclTaxInCustomerCurrency);

                    if (shipmentId != 0)
                    {
                        order.OrderSubtotalExclTax += orderShippingExclTaxInCustomerCurrency;
                    }

                    var p = GetPdfCell($"{"Shipping:"} {orderShippingExclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
            {
                if (order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax)
                {
                    var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = order.PaymentMethodAdditionalFeeInclTax;

                    var paymentMethodAdditionalFeeInclTaxStr = _priceFormatter.FormatPrice(paymentMethodAdditionalFeeInclTaxInCustomerCurrency);

                    if (shipmentId != 0)
                    {
                        order.OrderSubtotalInclTax += paymentMethodAdditionalFeeInclTaxInCustomerCurrency;
                    }

                    var p = GetPdfCell($"{"Payment Method Additional Fee:"} {paymentMethodAdditionalFeeInclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
                else
                {
                    var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = order.PaymentMethodAdditionalFeeExclTax;
                    var paymentMethodAdditionalFeeExclTaxStr = _priceFormatter.FormatPrice(paymentMethodAdditionalFeeExclTaxInCustomerCurrency);

                    if (shipmentId != 0)
                    {
                        order.OrderSubtotalExclTax += paymentMethodAdditionalFeeExclTaxInCustomerCurrency;
                    }

                    var p = GetPdfCell($"{"Payment Method Additional Fee:"} {paymentMethodAdditionalFeeExclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            var taxStr = string.Empty;
            var taxRates = new SortedDictionary<decimal, decimal>();
            bool displayTax = false;
            var displayTaxRates = true;
            if (InovatiqaDefaults.HideTaxInOrderSummary && order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax)
            {
                displayTax = false;
            }
            else
            {
                if (order.OrderTax == 0 && InovatiqaDefaults.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
            }

            if (displayTax)
            {
                var p = GetPdfCell($"{"Tax:"} {taxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            if (displayTaxRates)
            {
                foreach (var item in taxRates)
                {
                    var taxRate = string.Format("Tax {0}%:",
                        _priceFormatter.FormatPrice(item.Key));
                    var taxValue = _priceFormatter.FormatPrice(item.Value);

                    var p = GetPdfCell($"{taxRate} {taxValue}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency = order.OrderDiscount;
                var orderDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency);

                if (shipmentId != 0)
                {
                    order.OrderSubtotalInclTax -= orderDiscountInCustomerCurrency;
                    order.OrderSubtotalExclTax -= orderDiscountInCustomerCurrency;
                }

                var p = GetPdfCell($"{"Discount:"} {orderDiscountInCustomerCurrencyStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            var orderTotalInCustomerCurrency = order.OrderTotal;

            if(shipmentId != 0)
            {
                if(_shipmentService.GetShipmentsByOrderId(order.Id).OrderBy(s => s.Id).FirstOrDefault()?.Id == shipmentId)
                {
                    orderTotalInCustomerCurrency = order.OrderSubtotalInclTax;
                }
                else
                {
                    orderTotalInCustomerCurrency = order.OrderSubtotalInclTax - order.OrderShippingInclTax;
                }
            }

            var orderTotalStr = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency);

            var pTotal = GetPdfCell($"{"Order total:"} {orderTotalStr}", titleFont);
            pTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
            pTotal.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(pTotal);

            doc.Add(totalsTable);
        }

        protected virtual void PrintCheckoutAttributes(int vendorId, Order order, Document doc, Language lang, Font font)
        {
            if (vendorId != 0 || string.IsNullOrEmpty(order.CheckoutAttributeDescription))
                return;

            doc.Add(new Paragraph(" "));
            var attribTable = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var cCheckoutAttributes = GetPdfCell(HtmlHelper.ConvertHtmlToPlainText(order.CheckoutAttributeDescription, true, true), font);
            cCheckoutAttributes.Border = Rectangle.NO_BORDER;
            cCheckoutAttributes.HorizontalAlignment = Element.ALIGN_RIGHT;
            attribTable.AddCell(cCheckoutAttributes);
            doc.Add(attribTable);
        }

        protected virtual void PrintProducts(int vendorId, Language lang, Font titleFont, Document doc, Order order, Font font, Font attributesFont, int shipmentId = 0)
        {
            var productsHeader = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            var cellProducts = GetPdfCell("Product(s)", lang, titleFont);
            cellProducts.Border = Rectangle.NO_BORDER;
            productsHeader.AddCell(cellProducts);
            doc.Add(productsHeader);
            doc.Add(new Paragraph(" "));

            var orderItems = _orderService.GetOrderItems(order.Id, vendorId: vendorId);

            var count = 4 + (InovatiqaDefaults.ShowSkuOnProductDetailsPage ? 1 : 0)
                        + (InovatiqaDefaults.ShowVendorOnOrderDetailsPage ? 1 : 0);

            var productsTable = new PdfPTable(count)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { 4, new[] { 50, 20, 10, 20 } },
                { 5, new[] { 45, 15, 15, 10, 15 } },
                { 6, new[] { 40, 13, 13, 12, 10, 12 } }
            };

            productsTable.SetWidths(lang.Rtl ? widths[count].Reverse().ToArray() : widths[count]);

            var cellProductItem = GetPdfCell("Name", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            if (InovatiqaDefaults.ShowSkuOnProductDetailsPage)
            {
                cellProductItem = GetPdfCell("SKU", lang, font);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
            }

            if (InovatiqaDefaults.ShowVendorOnOrderDetailsPage)
            {
                cellProductItem = GetPdfCell("Vendor name", lang, font);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
            }

            cellProductItem = GetPdfCell("Price", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            cellProductItem = GetPdfCell("Qty", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            cellProductItem = GetPdfCell("Total", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            var vendors = InovatiqaDefaults.ShowVendorOnOrderDetailsPage ? _vendorService.GetVendorsByProductIds(orderItems.Select(item => item.ProductId).ToArray()) : new List<Vendor>();

            foreach (var orderItem in orderItems)
            {
                var partialSubTotalInclTax = 0.0m;
                var partialSubTotalExclTax = 0.0m;
                if (shipmentId != 0)
                {
                    var shipmentItems = _shipmentService.GetShipmentItemsByShipmentId(shipmentId);
                    if (shipmentItems != null)
                    {
                        var item = shipmentItems.Where(x => x.OrderItemId == orderItem.Id).FirstOrDefault();
                        if (item == null)
                            continue;
                        else
                        {
                            orderItem.Quantity = item.Quantity;
                            partialSubTotalInclTax = orderItem.Quantity * orderItem.UnitPriceInclTax;
                            partialSubTotalExclTax = orderItem.Quantity * orderItem.UnitPriceExclTax;
                        }
                    }
                }


                var product = _productService.GetProductById(orderItem.ProductId);

                var pAttribTable = new PdfPTable(1) { RunDirection = GetDirection(lang) };
                pAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                var name = product.Name;
                pAttribTable.AddCell(new Paragraph(name, font));
                cellProductItem.AddElement(new Paragraph(name, font));
                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    var attributesParagraph =
                        new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true),
                            attributesFont);
                    pAttribTable.AddCell(attributesParagraph);
                }

                productsTable.AddCell(pAttribTable);

                if (InovatiqaDefaults.ShowSkuOnProductDetailsPage)
                {
                    var sku = _productService.FormatSku(product, orderItem.AttributesXml);
                    cellProductItem = GetPdfCell(sku ?? string.Empty, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cellProductItem);
                }

                if (InovatiqaDefaults.ShowVendorOnOrderDetailsPage)
                {
                    var vendorName = vendors.FirstOrDefault(v => v.Id == product.VendorId)?.Name ?? string.Empty;
                    cellProductItem = GetPdfCell(vendorName, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cellProductItem);
                }

                string unitPrice;
                if (order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax)
                {
                    var unitPriceInclTaxInCustomerCurrency = orderItem.UnitPriceInclTax;
                    unitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency);
                }
                else
                {
                    var unitPriceExclTaxInCustomerCurrency = orderItem.UnitPriceExclTax;
                    unitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency);
                }

                cellProductItem = GetPdfCell(unitPrice, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);

                cellProductItem = GetPdfCell(orderItem.Quantity, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);

                string subTotal;
                if (shipmentId == 0)
                {
                    if (order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax)
                    {
                        var priceInclTaxInCustomerCurrency = orderItem.PriceInclTax;
                        subTotal = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency);
                    }
                    else
                    {
                        var priceExclTaxInCustomerCurrency = orderItem.PriceExclTax;
                        subTotal = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency);
                    }
                }
                else
                {
                    if (order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax)
                    {
                        var priceInclTaxInCustomerCurrency = partialSubTotalInclTax;
                        subTotal = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency);
                    }
                    else
                    {
                        var priceExclTaxInCustomerCurrency = partialSubTotalExclTax;
                        subTotal = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency);
                    }
                }

                cellProductItem = GetPdfCell(subTotal, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);
            }

            doc.Add(productsTable);
        }

        protected virtual void PrintAddresses(int vendorId, Language lang, Font titleFont, Order order, Font font, Document doc)
        {
            var addressTable = new PdfPTable(2) { RunDirection = GetDirection(lang) };
            addressTable.DefaultCell.Border = Rectangle.NO_BORDER;
            addressTable.WidthPercentage = 100f;
            addressTable.SetWidths(new[] { 50, 50 });

            PrintBillingInfo(vendorId, lang, titleFont, order, font, addressTable);

            PrintShippingInfo(lang, order, titleFont, font, addressTable);

            doc.Add(addressTable);
            doc.Add(new Paragraph(" "));
        }

        protected virtual void PrintShippingInfo(Language lang, Order order, Font titleFont, Font font, PdfPTable addressTable)
        {
            var shippingAddressPdf = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang)
            };
            shippingAddressPdf.DefaultCell.Border = Rectangle.NO_BORDER;

            if (order.ShippingStatusId != (int)ShippingStatus.ShippingNotRequired)
            {
                const string indent = "   ";

                if (!order.PickupInStore)
                {
                    if (order.ShippingAddressId == null || !(_addressService.GetAddressById(order.ShippingAddressId.Value) is Address shippingAddress))
                        throw new InovatiqaException($"Shipping is required, but address is not available. Order ID = {order.Id}");

                    shippingAddressPdf.AddCell(GetParagraph("Shipping Information:", lang, titleFont));
                    if (!string.IsNullOrEmpty(shippingAddress.Company))
                        shippingAddressPdf.AddCell(GetParagraph("Company: {0}", indent, lang, font, shippingAddress.Company));
                    shippingAddressPdf.AddCell(GetParagraph("Name: {0}", indent, lang, font, shippingAddress.FirstName + " " + shippingAddress.LastName));
                    if (InovatiqaDefaults.PhoneEnabled)
                        shippingAddressPdf.AddCell(GetParagraph("Phone: {0}", indent, lang, font, shippingAddress.PhoneNumber));
                    if (InovatiqaDefaults.FaxEnabled && !string.IsNullOrEmpty(shippingAddress.FaxNumber))
                        shippingAddressPdf.AddCell(GetParagraph("Fax: {0}", indent, lang, font, shippingAddress.FaxNumber));
                    if (InovatiqaDefaults.StreetAddressEnabled)
                        shippingAddressPdf.AddCell(GetParagraph("Address: {0}", indent, lang, font, shippingAddress.Address1));
                    if (InovatiqaDefaults.StreetAddress2Enabled && !string.IsNullOrEmpty(shippingAddress.Address2))
                        shippingAddressPdf.AddCell(GetParagraph("Address 2: {0}", indent, lang, font, shippingAddress.Address2));
                    if (InovatiqaDefaults.CityEnabled || InovatiqaDefaults.StateProvinceEnabled ||
                        InovatiqaDefaults.CountyEnabled || InovatiqaDefaults.ZipPostalCodeEnabled)
                    {
                        var addressLine = $"{indent}{shippingAddress.City}, " +
                            $"{(!string.IsNullOrEmpty(shippingAddress.County) ? $"{shippingAddress.County}, " : string.Empty)}" +
                            $"{(_stateProvinceService.GetStateProvinceByAddress(shippingAddress) is StateProvince stateProvince ? stateProvince.Name : string.Empty)} " +
                            $"{shippingAddress.ZipPostalCode}";
                        shippingAddressPdf.AddCell(new Paragraph(addressLine, font));
                    }

                    if (InovatiqaDefaults.CountryEnabled && _countryService.GetCountryByAddress(shippingAddress) is Country country)
                    {
                        shippingAddressPdf.AddCell(
                            new Paragraph(indent + country.Name, font));
                    }
                    var customShippingAddressAttributes = _addressAttributeFormatterService
                        .FormatAttributes(shippingAddress.CustomAttributes, $"<br />{indent}");
                    if (!string.IsNullOrEmpty(customShippingAddressAttributes))
                    {
                        var text = HtmlHelper.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true);
                        shippingAddressPdf.AddCell(new Paragraph(indent + text, font));
                    }

                    shippingAddressPdf.AddCell(new Paragraph(" "));
                }
                else if (order.PickupAddressId.HasValue && _addressService.GetAddressById(order.PickupAddressId.Value) is Address pickupAddress)
                {
                    shippingAddressPdf.AddCell(GetParagraph("Pickup point:", lang, titleFont));

                    if (!string.IsNullOrEmpty(pickupAddress.Address1))
                        shippingAddressPdf.AddCell(new Paragraph(
                            $"{indent}{string.Format("Address: {0}", pickupAddress.Address1)}",
                            font));

                    if (!string.IsNullOrEmpty(pickupAddress.City))
                        shippingAddressPdf.AddCell(new Paragraph($"{indent}{pickupAddress.City}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.County))
                        shippingAddressPdf.AddCell(new Paragraph($"{indent}{pickupAddress.County}", font));

                    if (_countryService.GetCountryByAddress(pickupAddress) is Country country)
                        shippingAddressPdf.AddCell(
                            new Paragraph($"{indent}{country.Name}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.ZipPostalCode))
                        shippingAddressPdf.AddCell(new Paragraph($"{indent}{pickupAddress.ZipPostalCode}", font));

                    shippingAddressPdf.AddCell(new Paragraph(" "));
                }

                shippingAddressPdf.AddCell(GetParagraph("Shipping method: {0}", indent, lang, font, order.ShippingMethod));
                shippingAddressPdf.AddCell(new Paragraph());

                addressTable.AddCell(shippingAddressPdf);
            }
            else
            {
                shippingAddressPdf.AddCell(new Paragraph());
                addressTable.AddCell(shippingAddressPdf);
            }
        }

        protected virtual void PrintBillingInfo(int vendorId, Language lang, Font titleFont, Order order, Font font, PdfPTable addressTable)
        {
            const string indent = "   ";
            var billingAddressPdf = new PdfPTable(1) { RunDirection = GetDirection(lang) };
            billingAddressPdf.DefaultCell.Border = Rectangle.NO_BORDER;

            billingAddressPdf.AddCell(GetParagraph("Billing Information:", lang, titleFont));

            var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

            if (InovatiqaDefaults.CompanyEnabled && !string.IsNullOrEmpty(billingAddress.Company))
                billingAddressPdf.AddCell(GetParagraph("Company: {0}", indent, lang, font, billingAddress.Company));

            billingAddressPdf.AddCell(GetParagraph("Name: {0}", indent, lang, font, billingAddress.FirstName + " " + billingAddress.LastName));

            if (InovatiqaDefaults.PhoneEnabled)
                billingAddressPdf.AddCell(GetParagraph("Phone: {0}", indent, lang, font, billingAddress.PhoneNumber));

            if (InovatiqaDefaults.FaxEnabled && !string.IsNullOrEmpty(billingAddress.FaxNumber))
                billingAddressPdf.AddCell(GetParagraph("Fax: {0}", indent, lang, font, billingAddress.FaxNumber));

            if (InovatiqaDefaults.StreetAddressEnabled)
                billingAddressPdf.AddCell(GetParagraph("Address: {0}", indent, lang, font, billingAddress.Address1));

            if (InovatiqaDefaults.StreetAddress2Enabled && !string.IsNullOrEmpty(billingAddress.Address2))
                billingAddressPdf.AddCell(GetParagraph("Address 2: {0}", indent, lang, font, billingAddress.Address2));

            if (InovatiqaDefaults.CityEnabled || InovatiqaDefaults.StateProvinceEnabled ||
                InovatiqaDefaults.CountyEnabled || InovatiqaDefaults.ZipPostalCodeEnabled)
            {
                var addressLine = $"{indent}{billingAddress.City}, " +
                    $"{(!string.IsNullOrEmpty(billingAddress.County) ? $"{billingAddress.County}, " : string.Empty)}" +
                    $"{(_stateProvinceService.GetStateProvinceByAddress(billingAddress) is StateProvince stateProvince ? stateProvince.Name : string.Empty)} " +
                    $"{billingAddress.ZipPostalCode}";
                billingAddressPdf.AddCell(new Paragraph(addressLine, font));
            }

            if (InovatiqaDefaults.CountryEnabled && _countryService.GetCountryByAddress(billingAddress) is Country country)
                billingAddressPdf.AddCell(new Paragraph(indent + country.Name, font));

            if (!string.IsNullOrEmpty(order.VatNumber))
                billingAddressPdf.AddCell(GetParagraph("VAT number: {0}", indent, lang, font, order.VatNumber));

            var customBillingAddressAttributes = _addressAttributeFormatterService
                .FormatAttributes(billingAddress.CustomAttributes, $"<br />{indent}");
            if (!string.IsNullOrEmpty(customBillingAddressAttributes))
            {
                var text = HtmlHelper.ConvertHtmlToPlainText(customBillingAddressAttributes, true, true);
                billingAddressPdf.AddCell(new Paragraph(indent + text, font));
            }

            if (vendorId == 0)
            {
                billingAddressPdf.AddCell(new Paragraph(" "));
                billingAddressPdf.AddCell(GetParagraph("Payment method: {0}", indent, lang, font, "Credit Card"));
                billingAddressPdf.AddCell(new Paragraph());

                var customValues = _paymentService.DeserializeCustomValues(order);
                if (customValues != null)
                {
                    foreach (var item in customValues)
                    {
                        billingAddressPdf.AddCell(new Paragraph(" "));
                        billingAddressPdf.AddCell(new Paragraph(indent + item.Key + ": " + item.Value, font));
                        billingAddressPdf.AddCell(new Paragraph());
                    }
                }
            }

            addressTable.AddCell(billingAddressPdf);
        }

        protected virtual void PrintHeader(Language lang, Order order, Font font, Font titleFont, Document doc, int shipmentId)
        {
            //Update Logic Below To Show Image On Invoice
            //var logoPicture = _pictureService.GetPictureById(pdfSettingsByStore.LogoPictureId);
            //var logoExists = logoPicture != null;
            var logoExists = false;

            var headerTable = new PdfPTable(logoExists ? 2 : 1)
            {
                RunDirection = GetDirection(lang)
            };
            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;

            var anchor = new Anchor(InovatiqaDefaults.StoreUrl.Trim('/'), font)
            {
                Reference = InovatiqaDefaults.StoreUrl
            };

            var cellHeader = GetPdfCell(string.Format("Order# {0}", order.CustomOrderNumber), titleFont);
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(string.Format("Invoice# {0}", shipmentId), titleFont));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(anchor));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(GetParagraph("Date: {0}", lang, font, _dateTimeHelperService.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc).ToString("D", new CultureInfo(lang.LanguageCulture))));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;
            cellHeader.Border = Rectangle.NO_BORDER;

            headerTable.AddCell(cellHeader);

            if (logoExists)
                headerTable.SetWidths(lang.Rtl ? new[] { 0.2f, 0.8f } : new[] { 0.8f, 0.2f });
            headerTable.WidthPercentage = 100f;

            //Update Logic Below To Show Image On Invoice
            //if (logoExists)
            //{
            //    var logoFilePath = _pictureService.GetThumbLocalPath(logoPicture, 0, false);
            //    var logo = Image.GetInstance(logoFilePath);
            //    logo.Alignment = GetAlignment(lang, true);
            //    logo.ScaleToFit(65f, 65f);

            //    var cellLogo = new PdfPCell { Border = Rectangle.NO_BORDER };
            //    cellLogo.AddElement(logo);
            //    headerTable.AddCell(cellLogo);
            //}

            doc.Add(headerTable);
        }

        #endregion

        #region Methods

        public virtual string PrintOrderToPdf(Order order, int languageId = 0, int vendorId = 0)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var fileName = $"order_{order.OrderGuid}_{CommonHelper.GenerateRandomDigitCode(4)}.pdf";
            var filePath = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/exportimport"), fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                var orders = new List<Order> { order };
                PrintOrdersToPdf(fileStream, orders, languageId, vendorId);
            }

            return filePath;
        }

        public virtual void PrintOrdersToPdf(Stream stream, IList<Order> orders, int languageId = 0, int vendorId = 0, int shipmentId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (orders == null)
                throw new ArgumentNullException(nameof(orders));

            var pageSize = PageSize.A4;

            if (InovatiqaDefaults.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();

            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            var ordCount = orders.Count;
            var ordNum = 0;

            Language lang = new Language();
            lang.DefaultCurrencyId = InovatiqaDefaults.LanguageDefaultCurrencyId;
            lang.DisplayOrder = InovatiqaDefaults.DisplayOrder;
            lang.FlagImageFileName = InovatiqaDefaults.FlagImageFileName;
            lang.LimitedToStores = InovatiqaDefaults.LimitedToStores;
            lang.Name = InovatiqaDefaults.LanguageName;
            lang.Rtl = InovatiqaDefaults.Rtl;
            lang.UniqueSeoCode = InovatiqaDefaults.UniqueSeoCode;
            lang.LanguageCulture = InovatiqaDefaults.LanguageCulture;

            foreach (var order in orders)
            {
                PrintHeader(lang, order, font, titleFont, doc, shipmentId);

                PrintAddresses(vendorId, lang, titleFont, order, font, doc);

                PrintProducts(vendorId, lang, titleFont, doc, order, font, attributesFont, shipmentId);

                PrintCheckoutAttributes(vendorId, order, doc, lang, font);

                PrintTotals(vendorId, lang, order, font, titleFont, doc, shipmentId);

                PrintOrderNotes(order, lang, titleFont, doc, font);

                ordNum++;
                if (ordNum < ordCount)
                {
                    doc.NewPage();
                }
            }

            doc.Close();
        }

        public virtual void PrintPackagingSlipsToPdf(Stream stream, IList<Shipment> shipments, int languageId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (shipments == null)
                throw new ArgumentNullException(nameof(shipments));

            var pageSize = PageSize.A4;

            if (InovatiqaDefaults.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            var shipmentCount = shipments.Count;
            var shipmentNum = 0;

            Language lang = new Language();
            lang.DefaultCurrencyId = InovatiqaDefaults.LanguageDefaultCurrencyId;
            lang.DisplayOrder = InovatiqaDefaults.DisplayOrder;
            lang.FlagImageFileName = InovatiqaDefaults.FlagImageFileName;
            lang.LimitedToStores = InovatiqaDefaults.LimitedToStores;
            lang.Name = InovatiqaDefaults.LanguageName;
            lang.Rtl = InovatiqaDefaults.Rtl;
            lang.UniqueSeoCode = InovatiqaDefaults.UniqueSeoCode;

            foreach (var shipment in shipments)
            {
                var order = _orderService.GetOrderById(shipment.OrderId);

                var addressTable = new PdfPTable(1);
                if (lang.Rtl)
                    addressTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                addressTable.DefaultCell.Border = Rectangle.NO_BORDER;
                addressTable.WidthPercentage = 100f;

                addressTable.AddCell(GetParagraph("Shipment #{0}", lang, titleFont, shipment.Id));
                addressTable.AddCell(GetParagraph("Order #{0}", lang, titleFont, order.CustomOrderNumber));

                if (!order.PickupInStore)
                {
                    if (order.ShippingAddressId == null || !(_addressService.GetAddressById(order.ShippingAddressId.Value) is Address shippingAddress))
                        throw new InovatiqaException($"Shipping is required, but address is not available. Order ID = {order.Id}");

                    if (InovatiqaDefaults.CompanyEnabled && !string.IsNullOrEmpty(shippingAddress.Company))
                        addressTable.AddCell(GetParagraph("Company: {0}", lang, font, shippingAddress.Company));

                    addressTable.AddCell(GetParagraph("Name: {0}", lang, font, shippingAddress.FirstName + " " + shippingAddress.LastName));
                    if (InovatiqaDefaults.PhoneEnabled)
                        addressTable.AddCell(GetParagraph("Phone: {0}", lang, font, shippingAddress.PhoneNumber));
                    if (InovatiqaDefaults.StreetAddressEnabled)
                        addressTable.AddCell(GetParagraph("Address: {0}", lang, font, shippingAddress.Address1));

                    if (InovatiqaDefaults.StreetAddress2Enabled && !string.IsNullOrEmpty(shippingAddress.Address2))
                        addressTable.AddCell(GetParagraph("Address 2: {0}", lang, font, shippingAddress.Address2));

                    if (InovatiqaDefaults.CityEnabled || InovatiqaDefaults.StateProvinceEnabled ||
                        InovatiqaDefaults.CountyEnabled || InovatiqaDefaults.ZipPostalCodeEnabled)
                    {
                        var addressLine = $"{shippingAddress.City}, " +
                            $"{(!string.IsNullOrEmpty(shippingAddress.County) ? $"{shippingAddress.County}, " : string.Empty)}" +
                            $"{(_stateProvinceService.GetStateProvinceByAddress(shippingAddress) is StateProvince stateProvince ? stateProvince.Name : string.Empty)} " +
                            $"{shippingAddress.ZipPostalCode}";
                        addressTable.AddCell(new Paragraph(addressLine, font));
                    }

                    if (InovatiqaDefaults.CountryEnabled && _countryService.GetCountryByAddress(shippingAddress) is Country country)
                        addressTable.AddCell(new Paragraph(country.Name, font));

                    var customShippingAddressAttributes = _addressAttributeFormatterService.FormatAttributes(shippingAddress.CustomAttributes);
                    if (!string.IsNullOrEmpty(customShippingAddressAttributes))
                    {
                        addressTable.AddCell(new Paragraph(HtmlHelper.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true), font));
                    }
                }
                else
                    if (order.PickupAddressId.HasValue && _addressService.GetAddressById(order.PickupAddressId.Value) is Address pickupAddress)
                {
                    addressTable.AddCell(new Paragraph("Pickup point:", titleFont));

                    if (!string.IsNullOrEmpty(pickupAddress.Address1))
                        addressTable.AddCell(new Paragraph($"   {string.Format("Address: {0}", pickupAddress.Address1)}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.City))
                        addressTable.AddCell(new Paragraph($"   {pickupAddress.City}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.County))
                        addressTable.AddCell(new Paragraph($"   {pickupAddress.County}", font));

                    if (_countryService.GetCountryByAddress(pickupAddress) is Country country)
                        addressTable.AddCell(new Paragraph($"   {country.Name}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.ZipPostalCode))
                        addressTable.AddCell(new Paragraph($"   {pickupAddress.ZipPostalCode}", font));

                    addressTable.AddCell(new Paragraph(" "));
                }

                addressTable.AddCell(new Paragraph(" "));

                addressTable.AddCell(GetParagraph("Shipping method: {0}", lang, font, order.ShippingMethod));
                addressTable.AddCell(new Paragraph(" "));
                doc.Add(addressTable);

                var productsTable = new PdfPTable(3) { WidthPercentage = 100f };
                if (lang.Rtl)
                {
                    productsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    productsTable.SetWidths(new[] { 20, 20, 60 });
                }
                else
                {
                    productsTable.SetWidths(new[] { 60, 20, 20 });
                }

                var cell = GetPdfCell("Product Name", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                cell = GetPdfCell("SKU", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                cell = GetPdfCell("QTY", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                foreach (var si in _shipmentService.GetShipmentItemsByShipmentId(shipment.Id))
                {
                    var productAttribTable = new PdfPTable(1);
                    if (lang.Rtl)
                        productAttribTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    productAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                    var orderItem = _orderService.GetOrderItemById(si.OrderItemId);
                    if (orderItem == null)
                        continue;

                    var product = _productService.GetProductById(orderItem.ProductId);

                    var name = product.Name;
                    productAttribTable.AddCell(new Paragraph(name, font));
                    if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                    {
                        var attributesParagraph = new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true), attributesFont);
                        productAttribTable.AddCell(attributesParagraph);
                    }


                    productsTable.AddCell(productAttribTable);

                    var sku = _productService.FormatSku(product, orderItem.AttributesXml);
                    cell = GetPdfCell(sku ?? string.Empty, font);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);

                    cell = GetPdfCell(si.Quantity, font);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);
                }

                doc.Add(productsTable);

                shipmentNum++;
                if (shipmentNum < shipmentCount)
                {
                    doc.NewPage();
                }
            }

            doc.Close();
        }

        public virtual void PrintProductsToPdf(Stream stream, IList<Product> products)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (products == null)
                throw new ArgumentNullException(nameof(products));

            Language lang = new Language();
            lang.DefaultCurrencyId = InovatiqaDefaults.LanguageDefaultCurrencyId;
            lang.DisplayOrder = InovatiqaDefaults.DisplayOrder;
            lang.FlagImageFileName = InovatiqaDefaults.FlagImageFileName;
            lang.LimitedToStores = InovatiqaDefaults.LimitedToStores;
            lang.Name = InovatiqaDefaults.LanguageName;
            lang.Rtl = InovatiqaDefaults.Rtl;
            lang.UniqueSeoCode = InovatiqaDefaults.UniqueSeoCode;

            var pageSize = PageSize.A4;

            if (InovatiqaDefaults.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            var font = GetFont();

            var productNumber = 1;
            var prodCount = products.Count;

            foreach (var product in products)
            {
                var productName = product.Name;
                var productDescription = product.FullDescription;

                var productTable = new PdfPTable(1) { WidthPercentage = 100f };
                productTable.DefaultCell.Border = Rectangle.NO_BORDER;
                if (lang.Rtl)
                {
                    productTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                }

                productTable.AddCell(new Paragraph($"{productNumber}. {productName}", titleFont));
                productTable.AddCell(new Paragraph(" "));
                productTable.AddCell(new Paragraph(HtmlHelper.StripTags(HtmlHelper.ConvertHtmlToPlainText(productDescription, decode: true)), font));
                productTable.AddCell(new Paragraph(" "));

                if (product.ProductTypeId == InovatiqaDefaults.SimpleProduct)
                {
                    var priceStr = $"{product.Price:0.00} {InovatiqaDefaults.CurrencyCode}";
    
                    productTable.AddCell(new Paragraph($"{"Price"}: {priceStr}", font));
                    productTable.AddCell(new Paragraph($"{"SKU"}: {product.Sku}", font));

                    if (product.IsShipEnabled && product.Weight > decimal.Zero)
                        productTable.AddCell(new Paragraph($"{"Weight"}: {product.Weight:0.00} {_measureService.GetMeasureWeightById(InovatiqaDefaults.BaseWeightId).Name}", font));

                    if (product.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock)
                        productTable.AddCell(new Paragraph($"{"Stock quantity"}: {_productService.GetTotalStockQuantity(product)}", font));

                    productTable.AddCell(new Paragraph(" "));
                }

                var pictures = _pictureService.GetPicturesByProductId(product.Id);
                if (pictures.Any())
                {
                    var table = new PdfPTable(2) { WidthPercentage = 100f };
                    if (lang.Rtl)
                    {
                        table.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    }

                    foreach (var pic in pictures)
                    {
                        var picBinary = _pictureService.LoadPictureBinary(pic);
                        if (picBinary == null || picBinary.Length <= 0)
                            continue;

                        var pictureLocalPath = _pictureService.GetThumbLocalPath(pic, 200, false);
                        var cell = new PdfPCell(Image.GetInstance(pictureLocalPath))
                        {
                            HorizontalAlignment = Element.ALIGN_LEFT,
                            Border = Rectangle.NO_BORDER
                        };
                        table.AddCell(cell);
                    }

                    if (pictures.Count % 2 > 0)
                    {
                        var cell = new PdfPCell(new Phrase(" "))
                        {
                            Border = Rectangle.NO_BORDER
                        };
                        table.AddCell(cell);
                    }

                    productTable.AddCell(table);
                    productTable.AddCell(new Paragraph(" "));
                }

                doc.Add(productTable);

                productNumber++;

                if (productNumber <= prodCount)
                {
                    doc.NewPage();
                }
            }

            doc.Close();
        }

        #endregion
    }
}