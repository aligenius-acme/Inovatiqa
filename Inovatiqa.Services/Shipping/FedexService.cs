using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.Shipping.Tracking;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Services.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Inovatiqa.Services.Shipping
{
    public class FedexService:IFedexService
    {
        #region Fields

        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly ILoggerService _loggerService;
        private readonly IMeasureService _measureService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IProductService _productService;
        private readonly IShippingService _shippingService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IWorkContextService _workContextService;

        #endregion

        #region Ctor

        public FedexService(ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerService customerservice,
            ILoggerService loggerService,
            IMeasureService measureService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IProductService productService,
            IShippingService shippingService,
            IStateProvinceService stateProvinceService,
            IWorkContextService workContextService)
        {
            _countryService = countryService;
            _currencyService = currencyService;
            _customerService = customerservice;
            _loggerService = loggerService;
            _measureService = measureService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _productService = productService;
            _shippingService = shippingService;
            _stateProvinceService = stateProvinceService;
            _workContextService = workContextService;
        }

        #endregion

        #region Utilities

        private decimal ConvertChargeToPrimaryCurrency(RateServiceWebReference.Money charge, Currency requestedShipmentCurrency)
        {
            decimal amount = 0.0m;
            var primaryStoreCurrency = _currencyService.GetCurrencyById(InovatiqaDefaults.PrimaryStoreCurrencyId);

            if (primaryStoreCurrency.CurrencyCode.Equals(charge.Currency, StringComparison.InvariantCultureIgnoreCase))
            {
                amount = charge.Amount;
            }
            //else
            //{
            //    var amountCurrency = charge.Currency == requestedShipmentCurrency.CurrencyCode ? requestedShipmentCurrency : _currencyService.GetCurrencyByCode(charge.Currency);

            //    amountCurrency ??= primaryStoreCurrency;

            //    amount = _currencyService.ConvertToPrimaryStoreCurrency(charge.Amount, amountCurrency);

            //    Debug.WriteLine($"ConvertChargeToPrimaryCurrency - from {charge.Amount} ({charge.Currency}) to {amount} ({primaryStoreCurrency.CurrencyCode})");
            //}

            return amount;
        }

        private (decimal width, decimal length, decimal height) GetDimensions(IList<GetShippingOptionRequest.PackageItem> items, int minRate = 1)
        {
            var measureDimension = _measureService.GetMeasureDimensionBySystemKeyword(InovatiqaDefaults.MEASURE_DIMENSION_SYSTEM_KEYWORD) ??
                throw new InovatiqaException($"FedEx shipping service. Could not load \"{InovatiqaDefaults.MEASURE_DIMENSION_SYSTEM_KEYWORD}\" measure dimension");

            _shippingService.GetDimensions(items, out var width, out var length, out var height, true);
            width = convertAndRoundDimension(width);
            length = convertAndRoundDimension(length);
            height = convertAndRoundDimension(height);

            return (width, length, height);

            #region Local functions

            decimal convertAndRoundDimension(decimal dimension)
            {
                dimension = _measureService.ConvertFromPrimaryMeasureDimension(dimension, measureDimension);
                dimension = Convert.ToInt32(Math.Ceiling(dimension));
                return Math.Max(dimension, minRate);
            }

            #endregion
        }

        private (decimal width, decimal length, decimal height) GetDimensionsForSingleItem(ShoppingCartItem item)
        {
            var product = _productService.GetProductById(item.ProductId);

            var items = new[] { new GetShippingOptionRequest.PackageItem(item, product, 1) };

            return GetDimensions(items);
        }

        private decimal GetWeight(GetShippingOptionRequest shippingOptionRequest, int minRate = 1)
        {
            var measureWeight = _measureService.GetMeasureWeightBySystemKeyword(InovatiqaDefaults.MEASURE_WEIGHT_SYSTEM_KEYWORD) ??
                throw new InovatiqaException($"FedEx shipping service. Could not load \"{InovatiqaDefaults.MEASURE_WEIGHT_SYSTEM_KEYWORD}\" measure weight");

            var weight = _shippingService.GetTotalWeight(shippingOptionRequest, ignoreFreeShippedItems: true);
            weight = _measureService.ConvertFromPrimaryMeasureWeight(weight, measureWeight);
            weight = Convert.ToInt32(Math.Ceiling(weight));
            return Math.Max(weight, minRate);
        }

        private decimal GetWeightForSingleItem(ShoppingCartItem item)
        {
            var customer = _customerService.GetCustomerById(item.CustomerId);
            var product = _productService.GetProductById(item.ProductId);

            var shippingOptionRequest = new GetShippingOptionRequest
            {
                Customer = customer,
                Items = new[] { new GetShippingOptionRequest.PackageItem(item, product, 1) }
            };

            return GetWeight(shippingOptionRequest);
        }

        private TrackServiceWebReference.TrackRequest CreateTrackRequest(string trackingNumber)
        {
            return new TrackServiceWebReference.TrackRequest
            {
                WebAuthenticationDetail = new Inovatiqa.Services.TrackServiceWebReference.WebAuthenticationDetail
                {
                    UserCredential = new Inovatiqa.Services.TrackServiceWebReference.WebAuthenticationCredential
                    {
                        Key = InovatiqaDefaults.FEDEXKey,
                        Password = InovatiqaDefaults.Password
                    }
                },
                ClientDetail = new Inovatiqa.Services.TrackServiceWebReference.ClientDetail
                {
                    AccountNumber = InovatiqaDefaults.AccountNumber,
                    MeterNumber = InovatiqaDefaults.MeterNumber
                },
                TransactionDetail = new Inovatiqa.Services.TrackServiceWebReference.TransactionDetail
                {
                    CustomerTransactionId = Guid.NewGuid().ToString()
                },
                Version = new Inovatiqa.Services.TrackServiceWebReference.VersionId(),
                SelectionDetails = new[]
                    {
                        new TrackServiceWebReference.TrackSelectionDetail
                        {
                            PackageIdentifier = new TrackServiceWebReference.TrackPackageIdentifier
                            {
                                Value = trackingNumber,
                                Type = TrackServiceWebReference.TrackIdentifierType.TRACKING_NUMBER_OR_DOORTAG
                            }
                        }
                    }
            };
        }

        private RateServiceWebReference.RequestedPackageLineItem CreatePackage(decimal width, decimal length, decimal height, decimal weight, decimal orderSubTotal, string sequenceNumber, string currencyCode)
        {
            return new RateServiceWebReference.RequestedPackageLineItem
            {
                SequenceNumber = sequenceNumber,                
                GroupPackageCount = "1",
                Weight = new RateServiceWebReference.Weight
                {
                    Units = RateServiceWebReference.WeightUnits.LB,
                    UnitsSpecified = true,
                    Value = weight,
                    ValueSpecified = true
                },   

                Dimensions = new RateServiceWebReference.Dimensions
                {
                    Length = InovatiqaDefaults.PassDimensions ? length.ToString() : "0",
                    Width = InovatiqaDefaults.PassDimensions ? width.ToString() : "0",
                    Height = InovatiqaDefaults.PassDimensions ? height.ToString() : "0",
                    Units = RateServiceWebReference.LinearUnits.IN,
                    UnitsSpecified = true
                },   
                InsuredValue = new RateServiceWebReference.Money
                {
                    Amount = orderSubTotal,
                    Currency = currencyCode
                }   
            };
        }

        private RateServiceWebReference.RateRequest CreateRateRequest(GetShippingOptionRequest shippingOptionRequest, out Currency requestedShipmentCurrency)
        {
            var request = new RateServiceWebReference.RateRequest
            {
                WebAuthenticationDetail = new RateServiceWebReference.WebAuthenticationDetail
                {
                    UserCredential = new RateServiceWebReference.WebAuthenticationCredential
                    {
                        Key = InovatiqaDefaults.FEDEXKey,
                        Password = InovatiqaDefaults.Password
                    }
                },

                ClientDetail = new RateServiceWebReference.ClientDetail
                {
                    AccountNumber = InovatiqaDefaults.AccountNumber,
                    MeterNumber = InovatiqaDefaults.MeterNumber
                },

                TransactionDetail = new RateServiceWebReference.TransactionDetail
                {
                    CustomerTransactionId = "***Rate Available Services v16 Request - nopCommerce***"                      
                },

                Version = new RateServiceWebReference.VersionId(),                      

                ReturnTransitAndCommit = true,
                ReturnTransitAndCommitSpecified = true,
                CarrierCodes = new[] {
                    RateServiceWebReference.CarrierCodeType.FDXE,
                    RateServiceWebReference.CarrierCodeType.FDXG
                }
            };

            _orderTotalCalculationService.GetShoppingCartSubTotal(
                shippingOptionRequest.Items.Select(x => x.ShoppingCartItem).ToList(),
                false, out var _, out var _, out var _, out var subTotalWithDiscountBase);

            request.RequestedShipment = new RateServiceWebReference.RequestedShipment();

            SetOrigin(request, shippingOptionRequest);
            SetDestination(request, shippingOptionRequest);

            requestedShipmentCurrency = GetRequestedShipmentCurrency(
                request.RequestedShipment.Shipper.Address.CountryCode,     
                request.RequestedShipment.Recipient.Address.CountryCode);  

            decimal subTotalShipmentCurrency = 0.0m;
            var primaryStoreCurrency = _currencyService.GetCurrencyById(InovatiqaDefaults.PrimaryStoreCurrencyId);

            if (requestedShipmentCurrency.CurrencyCode == primaryStoreCurrency.CurrencyCode)
                subTotalShipmentCurrency = subTotalWithDiscountBase;
            //else
            //    subTotalShipmentCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(subTotalWithDiscountBase, requestedShipmentCurrency);

            Debug.WriteLine($"SubTotal (Primary Currency) : {subTotalWithDiscountBase} ({primaryStoreCurrency.CurrencyCode})");
            Debug.WriteLine($"SubTotal (Shipment Currency): {subTotalShipmentCurrency} ({requestedShipmentCurrency.CurrencyCode})");

            SetShipmentDetails(request, subTotalShipmentCurrency, requestedShipmentCurrency.CurrencyCode);
            SetPayment(request);

            switch (InovatiqaDefaults.DefaultPackageType)
            {
                case InovatiqaDefaults.PackByOneItemPerPackage:
                    SetIndividualPackageLineItemsOneItemPerPackage(request, shippingOptionRequest, requestedShipmentCurrency.CurrencyCode);
                    break;
                case InovatiqaDefaults.PackByVolume:
                    SetIndividualPackageLineItemsCubicRootDimensions(request, shippingOptionRequest, subTotalShipmentCurrency, requestedShipmentCurrency.CurrencyCode);
                    break;
                case InovatiqaDefaults.PackByDimensions:
                default:
                    SetIndividualPackageLineItems(request, shippingOptionRequest, subTotalShipmentCurrency, requestedShipmentCurrency.CurrencyCode);
                    break;
            }
            return request;
        }

        private Currency GetRequestedShipmentCurrency(string originCountryCode, string destinCountryCode)
        {
            var primaryStoreCurrency = _currencyService.GetCurrencyById(InovatiqaDefaults.PrimaryStoreCurrencyId);

            var originCurrencyCode = getCurrencyCode(originCountryCode);
            var destinCurrencyCode = getCurrencyCode(destinCountryCode);

            if (originCurrencyCode == primaryStoreCurrency.CurrencyCode || destinCurrencyCode == primaryStoreCurrency.CurrencyCode)
            {
                return primaryStoreCurrency;
            }

            return _currencyService.GetCurrencyByCode(originCurrencyCode) ?? primaryStoreCurrency;

            #region Local functions

            string getCurrencyCode(string countryCode)
            {
                return countryCode switch
                {
                    "US" => "USD",
                    "CA" => "CAD",
                    "IN" => "INR",
                    _ => primaryStoreCurrency.CurrencyCode
                };
            }

            #endregion
        }

        private bool IsPackageTooHeavy(decimal weight)
        {
            return weight > InovatiqaDefaults.MAX_PACKAGE_WEIGHT;
        }

        private bool IsPackageTooLarge(decimal length, decimal height, decimal width)
        {
            return TotalPackageSize(length, height, width) > 165;
        }

        private bool IncludeStateProvinceCode(string countryCode)
        {
            return (countryCode.Equals("US", StringComparison.InvariantCultureIgnoreCase) ||
                    countryCode.Equals("CA", StringComparison.InvariantCultureIgnoreCase));
        }

        private IList<ShippingOption> ParseResponse(RateServiceWebReference.RateReply reply, Currency requestedShipmentCurrency)
        {
            var result = new List<ShippingOption>();

            Debug.WriteLine("RateReply details:");
            Debug.WriteLine("**********************************************************");
            foreach (var rateDetail in reply.RateReplyDetails)
            {
                var shippingOption = new ShippingOption();
                var serviceName = FedexServices.GetServiceName(rateDetail.ServiceType.ToString());

                if (!string.IsNullOrEmpty(InovatiqaDefaults.CarrierServicesOffered) && !InovatiqaDefaults.CarrierServicesOffered.Contains(rateDetail.ServiceType.ToString()))
                {
                    continue;
                }

                Debug.WriteLine("ServiceType: " + rateDetail.ServiceType);
                if (!serviceName.Equals("UNKNOWN"))
                {
                    shippingOption.Name = serviceName;

                    foreach (var shipmentDetail in rateDetail.RatedShipmentDetails)
                    {
                        Debug.WriteLine("RateType : " + shipmentDetail.ShipmentRateDetail.RateType);
                        Debug.WriteLine("Total Billing Weight : " + shipmentDetail.ShipmentRateDetail.TotalBillingWeight.Value);
                        Debug.WriteLine("Total Base Charge : " + shipmentDetail.ShipmentRateDetail.TotalBaseCharge.Amount);
                        Debug.WriteLine("Total Discount : " + shipmentDetail.ShipmentRateDetail.TotalFreightDiscounts.Amount);
                        Debug.WriteLine("Total Surcharges : " + shipmentDetail.ShipmentRateDetail.TotalSurcharges.Amount);
                        Debug.WriteLine($"Net Charge : {shipmentDetail.ShipmentRateDetail.TotalNetCharge.Amount} ({shipmentDetail.ShipmentRateDetail.TotalNetCharge.Currency})");
                        Debug.WriteLine("*********");

                        if (InovatiqaDefaults.ApplyDiscounts &
                            (shipmentDetail.ShipmentRateDetail.RateType == RateServiceWebReference.ReturnedRateType.PAYOR_ACCOUNT_PACKAGE ||
                            shipmentDetail.ShipmentRateDetail.RateType == RateServiceWebReference.ReturnedRateType.PAYOR_ACCOUNT_SHIPMENT))
                        {
                            var amount = ConvertChargeToPrimaryCurrency(shipmentDetail.ShipmentRateDetail.TotalNetCharge, requestedShipmentCurrency);
                            shippingOption.Rate = amount + InovatiqaDefaults.AdditionalHandlingCharge;
                            break;
                        }
                        else if (shipmentDetail.ShipmentRateDetail.RateType == RateServiceWebReference.ReturnedRateType.PAYOR_LIST_PACKAGE ||
                            shipmentDetail.ShipmentRateDetail.RateType == RateServiceWebReference.ReturnedRateType.PAYOR_LIST_SHIPMENT)       
                        {
                            var amount = ConvertChargeToPrimaryCurrency(shipmentDetail.ShipmentRateDetail.TotalNetCharge, requestedShipmentCurrency);
                            shippingOption.Rate = amount + InovatiqaDefaults.AdditionalHandlingCharge;
                            break;
                        }
                        else        
                        {
                            continue;
                        }
                    }
                    result.Add(shippingOption);
                }
                Debug.WriteLine("**********************************************************");
            }
            return result;
        }

        private void SetDestination(RateServiceWebReference.RateRequest request, GetShippingOptionRequest getShippingOptionRequest)
        {
            request.RequestedShipment.Recipient = new RateServiceWebReference.Party
            {
                Address = new RateServiceWebReference.Address()
            };
            if (InovatiqaDefaults.UseResidentialRates)
            {
                request.RequestedShipment.Recipient.Address.Residential = true;
                request.RequestedShipment.Recipient.Address.ResidentialSpecified = true;
            }

            request.RequestedShipment.Recipient.Address.StreetLines = new[] { getShippingOptionRequest.ShippingAddress.Address1 };
            request.RequestedShipment.Recipient.Address.City = getShippingOptionRequest.ShippingAddress.City;

            var recipientCountryCode = _countryService.GetCountryByAddress(getShippingOptionRequest.ShippingAddress)?.TwoLetterIsoCode ?? string.Empty;

            if (_stateProvinceService.GetStateProvinceByAddress(getShippingOptionRequest.ShippingAddress) is StateProvince stateProvince &&
                IncludeStateProvinceCode(recipientCountryCode))
            {
                request.RequestedShipment.Recipient.Address.StateOrProvinceCode = stateProvince.Abbreviation;
            }
            else
            {
                request.RequestedShipment.Recipient.Address.StateOrProvinceCode = string.Empty;
            }
            request.RequestedShipment.Recipient.Address.PostalCode = getShippingOptionRequest.ShippingAddress.ZipPostalCode;
            request.RequestedShipment.Recipient.Address.CountryCode = recipientCountryCode;
        }

        private void SetIndividualPackageLineItems(RateServiceWebReference.RateRequest request, GetShippingOptionRequest getShippingOptionRequest, decimal orderSubTotal, string currencyCode)
        {
            var (length, height, width) = GetDimensions(getShippingOptionRequest.Items);
            var weight = GetWeight(getShippingOptionRequest);

            if (!IsPackageTooHeavy(weight) && !IsPackageTooLarge(length, height, width))
            {
                request.RequestedShipment.PackageCount = "1";

                var package = CreatePackage(width, length, height, weight, orderSubTotal, "1", currencyCode);
                package.GroupPackageCount = "1";

                request.RequestedShipment.RequestedPackageLineItems = new[] { package };
            }
            else
            {
                var totalPackagesDims = 1;
                var totalPackagesWeights = 1;
                if (IsPackageTooHeavy(weight))
                {
                    totalPackagesWeights = Convert.ToInt32(Math.Ceiling(weight / InovatiqaDefaults.MAX_PACKAGE_WEIGHT));
                }
                if (IsPackageTooLarge(length, height, width))
                {
                    totalPackagesDims = Convert.ToInt32(Math.Ceiling(TotalPackageSize(length, height, width) / 108M));
                }
                var totalPackages = totalPackagesDims > totalPackagesWeights ? totalPackagesDims : totalPackagesWeights;
                if (totalPackages == 0)
                    totalPackages = 1;

                width = Math.Max(width / totalPackages, 1);
                length = Math.Max(length / totalPackages, 1);
                height = Math.Max(height / totalPackages, 1);
                weight = Math.Max(weight / totalPackages, 1);

                var orderSubTotal2 = orderSubTotal / totalPackages;

                request.RequestedShipment.PackageCount = totalPackages.ToString();

                request.RequestedShipment.RequestedPackageLineItems = Enumerable.Range(1, totalPackages - 1)
                    .Select(i => CreatePackage(width, length, height, weight, orderSubTotal2, i.ToString(), currencyCode)).ToArray();
            }
        }

        private void SetIndividualPackageLineItemsCubicRootDimensions(RateServiceWebReference.RateRequest request, GetShippingOptionRequest getShippingOptionRequest, decimal orderSubTotal, string currencyCode)
        {
            var totalPackagesDims = 1;
            var length = 0M;
            var height = 0M;
            var width = 0M;

            if (getShippingOptionRequest.Items.Count == 1 && getShippingOptionRequest.Items[0].GetQuantity() == 1)
            {
                var sci = getShippingOptionRequest.Items[0].ShoppingCartItem;

                var item = getShippingOptionRequest.Items.FirstOrDefault().ShoppingCartItem;
                (width, length, height) = GetDimensionsForSingleItem(item);
            }
            else
            {
                var dimension = 0;

                var totalVolume = getShippingOptionRequest.Items.Sum(item =>
                {
                    var (itemWidth, itemLength, itemHeight) = GetDimensionsForSingleItem(item.ShoppingCartItem);
                    return item.GetQuantity() * itemWidth * itemLength * itemHeight;
                });
                if (totalVolume > decimal.Zero)
                {
                    var packageVolume = InovatiqaDefaults.PackingPackageVolume;
                    if (packageVolume <= 0)
                        packageVolume = 5184;

                    dimension = Convert.ToInt32(Math.Floor(Math.Pow(Convert.ToDouble(packageVolume), 1.0 / 3.0)));
                    if (IsPackageTooLarge(dimension, dimension, dimension))
                        throw new InovatiqaException("fedexSettings.PackingPackageVolume exceeds max package size");

                    packageVolume = dimension * dimension * dimension;

                    totalPackagesDims = Convert.ToInt32(Math.Ceiling(totalVolume / packageVolume));
                }

                width = length = height = dimension;
            }

            width = Math.Max(width, 1);
            length = Math.Max(length, 1);
            height = Math.Max(height, 1);

            var weight = GetWeight(getShippingOptionRequest);

            var totalPackagesWeights = 1;
            if (IsPackageTooHeavy(weight))
            {
                totalPackagesWeights = Convert.ToInt32(Math.Ceiling(weight / InovatiqaDefaults.MAX_PACKAGE_WEIGHT));
            }

            var totalPackages = totalPackagesDims > totalPackagesWeights ? totalPackagesDims : totalPackagesWeights;

            var orderSubTotalPerPackage = orderSubTotal / totalPackages;
            var weightPerPackage = weight / totalPackages;

            request.RequestedShipment.PackageCount = totalPackages.ToString();

            request.RequestedShipment.RequestedPackageLineItems = Enumerable.Range(1, totalPackages)
                    .Select(i => CreatePackage(width, length, height, weightPerPackage, orderSubTotalPerPackage, i.ToString(), currencyCode))
                    .ToArray();
        }

        private void SetIndividualPackageLineItemsOneItemPerPackage(RateServiceWebReference.RateRequest request, GetShippingOptionRequest getShippingOptionRequest, string currencyCode)
        {
            var i = 1;
            var items = getShippingOptionRequest.Items;
            var totalItems = items.Sum(x => x.GetQuantity());

            request.RequestedShipment.PackageCount = totalItems.ToString();
            request.RequestedShipment.RequestedPackageLineItems = getShippingOptionRequest.Items.SelectMany(packageItem =>
            {
                var (width, length, height) = GetDimensionsForSingleItem(packageItem.ShoppingCartItem);
                var weight = GetWeightForSingleItem(packageItem.ShoppingCartItem);

                var product = _productService.GetProductById(packageItem.ShoppingCartItem.ProductId);
                var package = CreatePackage(width, length, height, weight, product.Price, (i + 1).ToString(), currencyCode);
                package.GroupPackageCount = "1";

                var packs = Enumerable.Range(i, packageItem.GetQuantity())
                    .Select(j => CreatePackage(width, length, height, weight, product.Price, j.ToString(), currencyCode)).ToArray();
                i += packageItem.GetQuantity();

                return packs;
            }).ToArray();
        }

        private void SetOrigin(RateServiceWebReference.RateRequest request, GetShippingOptionRequest getShippingOptionRequest)
        {
            request.RequestedShipment.Shipper = new RateServiceWebReference.Party
            {
                Address = new RateServiceWebReference.Address()
            };

            if (getShippingOptionRequest.CountryFrom is null)
                throw new Exception("FROM country is not specified");

            request.RequestedShipment.Shipper.Address.StreetLines = new[] { getShippingOptionRequest.AddressFrom };
            request.RequestedShipment.Shipper.Address.City = getShippingOptionRequest.CityFrom;
            if (IncludeStateProvinceCode(getShippingOptionRequest.CountryFrom.TwoLetterIsoCode))
            {
                var stateProvinceAbbreviation = getShippingOptionRequest.StateProvinceFrom?.Abbreviation ?? "";
                request.RequestedShipment.Shipper.Address.StateOrProvinceCode = stateProvinceAbbreviation;
            }
            request.RequestedShipment.Shipper.Address.PostalCode = getShippingOptionRequest.ZipPostalCodeFrom;
            request.RequestedShipment.Shipper.Address.CountryCode = getShippingOptionRequest.CountryFrom.TwoLetterIsoCode;
        }

        private void SetPayment(RateServiceWebReference.RateRequest request)
        {
            request.RequestedShipment.ShippingChargesPayment = new RateServiceWebReference.Payment
            {
                PaymentType = RateServiceWebReference.PaymentType.SENDER,       
                PaymentTypeSpecified = true,
                Payor = new RateServiceWebReference.Payor
                {
                    ResponsibleParty = new RateServiceWebReference.Party
                    {
                        AccountNumber = InovatiqaDefaults.AccountNumber
                    }
                }
            };   
        }

        private void SetShipmentDetails(RateServiceWebReference.RateRequest request, decimal orderSubTotal, string currencyCode)
        {
            request.RequestedShipment.DropoffType = InovatiqaDefaults.DropoffType switch
            {
                InovatiqaDefaults.BusinessServiceCenter => RateServiceWebReference.DropoffType.BUSINESS_SERVICE_CENTER,
                InovatiqaDefaults.DropBox => RateServiceWebReference.DropoffType.DROP_BOX,
                InovatiqaDefaults.RegularPickup => RateServiceWebReference.DropoffType.REGULAR_PICKUP,
                InovatiqaDefaults.RequestCourier => RateServiceWebReference.DropoffType.REQUEST_COURIER,
                InovatiqaDefaults.Station => RateServiceWebReference.DropoffType.STATION,
                _ => RateServiceWebReference.DropoffType.BUSINESS_SERVICE_CENTER
            };

            request.RequestedShipment.TotalInsuredValue = new RateServiceWebReference.Money
            {
                Amount = orderSubTotal,
                Currency = currencyCode
            };

            var shipTimestamp = DateTime.Now;
            if (shipTimestamp.DayOfWeek == DayOfWeek.Saturday)
                shipTimestamp = shipTimestamp.AddDays(2);
            request.RequestedShipment.ShipTimestamp = shipTimestamp;     
            request.RequestedShipment.ShipTimestampSpecified = true;

            request.RequestedShipment.RateRequestTypes = new[] {
                RateServiceWebReference.RateRequestType.PREFERRED,
                RateServiceWebReference.RateRequestType.LIST
            };
            if (request.RequestedShipment.Shipper.Address.CountryCode.Equals("IN", StringComparison.InvariantCultureIgnoreCase) &&
                request.RequestedShipment.Recipient.Address.CountryCode.Equals("IN", StringComparison.InvariantCultureIgnoreCase))
            {
                var commodity = new RateServiceWebReference.Commodity
                {
                    Name = "1",
                    NumberOfPieces = "1",
                    CustomsValue = new RateServiceWebReference.Money
                    {
                        Amount = orderSubTotal,
                        AmountSpecified = true,
                        Currency = currencyCode
                    }
                };

                request.RequestedShipment.CustomsClearanceDetail = new RateServiceWebReference.CustomsClearanceDetail
                {
                    CommercialInvoice = new RateServiceWebReference.CommercialInvoice
                    {
                        Purpose = RateServiceWebReference.PurposeOfShipmentType.SOLD,
                        PurposeSpecified = true
                    },
                    Commodities = new[] { commodity }
                };
            }
        }

        private decimal TotalPackageSize(decimal length, decimal height, decimal width)
        {
            return height * 2 + width * 2 + length;
        }

        private async Task<TrackServiceWebReference.TrackReply> TrackAsync(TrackServiceWebReference.TrackRequest request)
        {
            TrackServiceWebReference.TrackPortTypeClient service = new TrackServiceWebReference.TrackPortTypeClient(TrackServiceWebReference.TrackPortTypeClient.EndpointConfiguration.TrackServicePort, InovatiqaDefaults.FEDEXUrl);

            var trackResponse = await service.trackAsync(request);

            return trackResponse.TrackReply;
        }

        #endregion

        #region Methods

        public virtual IList<ShipmentStatusEvent> GetShipmentEvents(string trackingNumber)
        {
            try
            {
                var request = CreateTrackRequest(trackingNumber);

                var reply = TrackAsync(request).Result;

                if (new[] { TrackServiceWebReference.NotificationSeverityType.SUCCESS, TrackServiceWebReference.NotificationSeverityType.NOTE, TrackServiceWebReference.NotificationSeverityType.WARNING }.Contains(reply.HighestSeverity))       
                {

                    return reply.CompletedTrackDetails?
                        .SelectMany(completedTrackDetails => completedTrackDetails.TrackDetails?
                            .SelectMany(trackDetails => trackDetails.Events?
                                .Select(trackEvent => new ShipmentStatusEvent
                                {
                                    EventName = $"{trackEvent.EventDescription} ({trackEvent.EventType})",
                                    Location = trackEvent?.Address?.City,
                                    CountryCode = trackEvent?.Address?.CountryCode,
                                    Date = trackEvent.TimestampSpecified ? trackEvent.Timestamp as DateTime? : null
                                })))
                        .ToList();
                }
            }
            catch (Exception exception)
            {
                _loggerService.Error($"Error while getting Fedex shipment tracking info - {trackingNumber}{Environment.NewLine}{exception.Message}", exception, _workContextService.CurrentCustomer);
            }

            return new List<ShipmentStatusEvent>();
        }

        public virtual GetShippingOptionResponse GetRates(GetShippingOptionRequest shippingOptionRequest)
        {
            var response = new GetShippingOptionResponse();

            var request = CreateRateRequest(shippingOptionRequest, out var requestedShipmentCurrency);

            var service = new RateServiceWebReference.RatePortTypeClient(RateServiceWebReference.RatePortTypeClient.EndpointConfiguration.RateServicePort, InovatiqaDefaults.FEDEXUrl);

            try
            {
                var reply = service.getRatesAsync(request).Result.RateReply;   

                if (new[] { RateServiceWebReference.NotificationSeverityType.SUCCESS, RateServiceWebReference.NotificationSeverityType.NOTE, RateServiceWebReference.NotificationSeverityType.WARNING }.Contains(reply.HighestSeverity))       
                {
                    if (reply.RateReplyDetails != null)
                    {
                        var shippingOptions = ParseResponse(reply, requestedShipmentCurrency);
                        foreach (var shippingOption in shippingOptions)
                            response.ShippingOptions.Add(shippingOption);
                    }
                    else
                    {
                        if (reply.Notifications?.Length > 0 && !string.IsNullOrEmpty(reply.Notifications[0].Message))
                        {
                            response.AddError($"{reply.Notifications[0].Message} (code: {reply.Notifications[0].Code})");
                        }
                        else
                        {
                            response.AddError("Could not get reply from shipping server");
                        }
                    }
                }
                else
                {
                    Debug.WriteLine(reply.Notifications[0].Message);
                    response.AddError(reply.Notifications[0].Message);
                }

                return response;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                response.AddError(e.Message);
                return response;
            }
        }

        #endregion
    }
}
