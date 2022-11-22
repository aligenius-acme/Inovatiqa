using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial class AddressAttributeModelFactory : IAddressAttributeModelFactory
    {
        #region Fields

        private readonly IAddressAttributeParserService _addressAttributeParserService;
        private readonly IAddressAttributeService _addressAttributeService;

        #endregion

        #region Ctor

        public AddressAttributeModelFactory(IAddressAttributeParserService addressAttributeParserService,
            IAddressAttributeService addressAttributeService)
        {
            _addressAttributeParserService = addressAttributeParserService;
            _addressAttributeService = addressAttributeService;
        }

        #endregion

        #region Utilities


        #endregion

        #region Methods

        public virtual void PrepareCustomAddressAttributes(IList<AddressModel.AddressAttributeModel> models, Address address)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            var attributes = _addressAttributeService.GetAllAddressAttributes();
            foreach (var attribute in attributes)
            {
                var attributeModel = new AddressModel.AddressAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.Name,
                    IsRequired = attribute.IsRequired,
                    AttributeControlTypeId = attribute.AttributeControlTypeId,
                };

                if (attribute.ShouldHaveValues())
                {
                    var attributeValues = _addressAttributeService.GetAddressAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new AddressModel.AddressAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                var selectedAddressAttributes = address?.CustomAttributes;
                switch (attribute.AttributeControlTypeId)
                {
                    case (int)AttributeControlType.DropdownList:
                    case (int)AttributeControlType.RadioList:
                    case (int)AttributeControlType.Checkboxes:
                    {
                        if (!string.IsNullOrEmpty(selectedAddressAttributes))
                        {
                            foreach (var item in attributeModel.Values)
                                item.IsPreSelected = false;

                            var selectedValues = _addressAttributeParserService.ParseAddressAttributeValues(selectedAddressAttributes);
                            foreach (var attributeValue in selectedValues)
                                foreach (var item in attributeModel.Values)
                                    if (attributeValue.Id == item.Id)
                                        item.IsPreSelected = true;
                        }
                    }
                    break;
                    case (int)AttributeControlType.ReadonlyCheckboxes:
                    {
                    }
                    break;
                    case (int)AttributeControlType.TextBox:
                    case (int)AttributeControlType.MultilineTextbox:
                    {
                        if (!string.IsNullOrEmpty(selectedAddressAttributes))
                        {
                            var enteredText = _addressAttributeParserService.ParseValues(selectedAddressAttributes, attribute.Id);
                            if (enteredText.Any())
                                attributeModel.DefaultValue = enteredText[0];
                        }
                    }
                    break;
                    case (int)AttributeControlType.ColorSquares:
                    case (int)AttributeControlType.ImageSquares:
                    case (int)AttributeControlType.Datepicker:
                    case (int)AttributeControlType.FileUpload:
                    default:
                        break;
                }

                models.Add(attributeModel);
            }
        }

        #endregion
    }
}