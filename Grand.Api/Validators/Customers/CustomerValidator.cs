﻿using FluentValidation;
using Grand.Api.DTOs.Customers;
using Grand.Core.Domain.Customers;
using Grand.Framework.Validators;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Localization;
using System.Linq;

namespace Grand.Api.Validators.Customers
{
    public class CustomerValidator : BaseGrandValidator<CustomerDto>
    {
        public CustomerValidator(ILocalizationService localizationService, IStateProvinceService stateProvinceService, ICustomerService customerService, CustomerSettings customerSettings)
        {

            RuleFor(x => x).Must((x, context) =>
            {
                var customer = customerService.GetCustomerByEmail(x.Email);
                if (customer != null && customer.Id != x.Id)
                {
                    return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Email is already registered"));

            RuleFor(x => x).Must((x, context) =>
            {
                var username = customerService.GetCustomerByEmail(x.Username);
                if (username != null && username.Id != x.Id && customerSettings.UsernamesEnabled)
                {
                    return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Username is already registered"));


            //form fields
            if (customerSettings.CountryEnabled && customerSettings.CountryRequired)
            {
                RuleFor(x => x.CountryId)
                    .NotEqual("")
                    .WithMessage(localizationService.GetResource("Api.Customers.Customer.Fields.Country.Required"));
            }
            if (customerSettings.CountryEnabled &&
                customerSettings.StateProvinceEnabled &&
                customerSettings.StateProvinceRequired)
            {

                RuleFor(x => x).Must((x, context) =>
                {
                    //does selected country have states?
                    var hasStates = stateProvinceService.GetStateProvincesByCountryId(x.CountryId).Any();
                    if (hasStates)
                    {
                        //if yes, then ensure that a state is selected
                        if (string.IsNullOrEmpty(x.StateProvinceId))
                            return true;
                    }
                    return false;
                }).WithMessage(localizationService.GetResource("Api.Customers.Customer.Fields.StateProvince.Required"));

            }

            if (customerSettings.CompanyRequired && customerSettings.CompanyEnabled)
                RuleFor(x => x.Company).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Customer.Customers.Customers.Fields.Company.Required"));
            if (customerSettings.StreetAddressRequired && customerSettings.StreetAddressEnabled)
                RuleFor(x => x.StreetAddress).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Customer.Customers.Customers.Fields.StreetAddress.Required"));
            if (customerSettings.StreetAddress2Required && customerSettings.StreetAddress2Enabled)
                RuleFor(x => x.StreetAddress2).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Customer.Customers.Customers.Fields.StreetAddress2.Required"));
            if (customerSettings.ZipPostalCodeRequired && customerSettings.ZipPostalCodeEnabled)
                RuleFor(x => x.ZipPostalCode).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Customer.Customers.Customers.Fields.ZipPostalCode.Required"));
            if (customerSettings.CityRequired && customerSettings.CityEnabled)
                RuleFor(x => x.City).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Customer.Customers.Customers.Fields.City.Required"));
            if (customerSettings.PhoneRequired && customerSettings.PhoneEnabled)
                RuleFor(x => x.Phone).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Customer.Customers.Customers.Fields.Phone.Required"));
            if (customerSettings.FaxRequired && customerSettings.FaxEnabled)
                RuleFor(x => x.Fax).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.Customer.Customers.Customers.Fields.Fax.Required"));
        }
    }
}
