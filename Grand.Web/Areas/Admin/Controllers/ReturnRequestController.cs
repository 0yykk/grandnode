﻿using Microsoft.AspNetCore.Mvc;
using Grand.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using Grand.Web.Areas.Admin.Models.Orders;
using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using System.Linq;
using Grand.Core.Data;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Core.Domain.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Web.Services;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Core.Domain.Directory;
using Grand.Services.Common;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class ReturnRequestController : BaseAdminController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly IRepository<ReturnRequest> _returnRequest;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly AddressSettings _addressSettings;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressWebService _addressWebService;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        #endregion Fields

        #region Constructors

        public ReturnRequestController(IOrderService orderService,
            IProductService productService,
            ICustomerService customerService, IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService, IWorkContext workContext,
            IWorkflowMessageService workflowMessageService, LocalizationSettings localizationSettings,
            ICustomerActivityService customerActivityService, IPermissionService permissionService,
            IRepository<ReturnRequest> returnRequest,
            IReturnRequestService returnRequestService,
            IPriceFormatter priceFormatter,
            ICurrencyService currencyService,
            AddressSettings addressSettings,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IAddressWebService addressWebService,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeParser addressAttributeParser)
        {
            this._orderService = orderService;
            this._productService = productService;
            this._customerService = customerService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._workflowMessageService = workflowMessageService;
            this._localizationSettings = localizationSettings;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            this._returnRequest = returnRequest;
            this._returnRequestService = returnRequestService;
            this._priceFormatter = priceFormatter;
            this._currencyService = currencyService;
            this._addressSettings = addressSettings;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._addressWebService = addressWebService;
            this._addressAttributeService = addressAttributeService;
            this._addressAttributeParser = addressAttributeParser;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual ReturnRequestModel PrepareReturnRequestModel(ReturnRequestModel model,
            ReturnRequest returnRequest, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

            var order = _orderService.GetOrderById(returnRequest.OrderId);
            decimal unitPriceInclTaxInCustomerCurrency = 0;
            foreach (var item in returnRequest.ReturnRequestItems)
            {
                var orderItem = order.OrderItems.Where(x => x.Id == item.OrderItemId).First();
                unitPriceInclTaxInCustomerCurrency += _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate) * item.Quantity;
            }

            model.Total = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency);
            model.Quantity = returnRequest.ReturnRequestItems.Sum(x => x.Quantity);
            model.Id = returnRequest.Id;
            model.OrderId = order.Id;
            model.OrderNumber = order.OrderNumber;
            model.ReturnNumber = returnRequest.ReturnNumber;
            model.CustomerId = returnRequest.CustomerId;
            var customer = _customerService.GetCustomerById(returnRequest.CustomerId);
            model.CustomerInfo = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
            model.ReturnRequestStatusStr = returnRequest.ReturnRequestStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(returnRequest.CreatedOnUtc, DateTimeKind.Utc);
            model.PickupDate = returnRequest.PickupDate;

            if (!excludeProperties)
            {
                var addr = new AddressModel();
                PrepareAddressModel(ref addr, returnRequest.PickupAddress, excludeProperties);
                model.PickupAddress = addr;
                model.CustomerComments = returnRequest.CustomerComments;
                model.StaffNotes = returnRequest.StaffNotes;
                model.ReturnRequestStatusId = returnRequest.ReturnRequestStatusId;
            }

            return model;
        }

        [NonAction]
        protected virtual void PrepareAddressModel(ref AddressModel model, Address address, bool excludeProperties)
        {
            if (address != null)
            {
                if (!excludeProperties)
                {
                    model = address.ToModel();
                }
            }

            if (model == null)
                model = new AddressModel();

            model.FirstNameEnabled = true;
            model.FirstNameRequired = true;
            model.LastNameEnabled = true;
            model.LastNameRequired = true;
            model.EmailEnabled = true;
            model.EmailRequired = true;
            model.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.CompanyRequired = _addressSettings.CompanyRequired;
            model.VatNumberEnabled = _addressSettings.VatNumberEnabled;
            model.VatNumberRequired = _addressSettings.VatNumberRequired;
            model.CountryEnabled = _addressSettings.CountryEnabled;
            model.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.CityEnabled = _addressSettings.CityEnabled;
            model.CityRequired = _addressSettings.CityRequired;
            model.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.PhoneRequired = _addressSettings.PhoneRequired;
            model.FaxEnabled = _addressSettings.FaxEnabled;
            model.FaxRequired = _addressSettings.FaxRequired;
            //countries
            model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "" });
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.CountryId) });
            //states
            var states = !String.IsNullOrEmpty(model.CountryId) ? _stateProvinceService.GetStateProvincesByCountryId(model.CountryId, showHidden: true).ToList() : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
            }
            else
                model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "" });
            //customer attribute services
            model.PrepareCustomAddressAttributes(address, _addressAttributeService, _addressAttributeParser);
        }

        #endregion

        #region Methods

        //list
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            var returnRequests = _returnRequestService.SearchReturnRequests("", "", "", null, command.Page - 1, command.PageSize);
            var returnRequestModels = new List<ReturnRequestModel>();
            foreach (var rr in returnRequests)
            {
                var model = new ReturnRequestModel();
                returnRequestModels.Add(PrepareReturnRequestModel(model, rr, true));
            }
            var gridModel = new DataSourceResult
            {
                Data = returnRequestModels,
                Total = returnRequests.TotalCount,
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductsForReturnRequest(string returnRequestId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            var returnRequest = _returnRequestService.GetReturnRequestById(returnRequestId);
            List<ReturnRequestModel.ReturnRequestItemModel> items = new List<ReturnRequestModel.ReturnRequestItemModel>();
            var order = _orderService.GetOrderById(returnRequest.OrderId);

            foreach (var item in returnRequest.ReturnRequestItems)
            {
                var orderItem = order.OrderItems.Where(x => x.Id == item.OrderItemId).FirstOrDefault();

                items.Add(new ReturnRequestModel.ReturnRequestItemModel
                {
                    ProductId = orderItem.ProductId,
                    ProductName = _productService.GetProductByIdIncludeArch(orderItem.ProductId).Name,
                    Quantity = item.Quantity,
                    UnitPrice = _priceFormatter.FormatPrice(orderItem.UnitPriceInclTax),
                    ReasonForReturn = item.ReasonForReturn,
                    RequestedAction = item.RequestedAction
                });
            }

            var gridModel = new DataSourceResult
            {
                Data = items,
                Total = items.Count,
            };

            return Json(gridModel);
        }

        //edit
        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            var returnRequest = _returnRequestService.GetReturnRequestById(id);
            if (returnRequest == null)
                //No return request found with the specified id
                return RedirectToAction("List");

            var model = new ReturnRequestModel();
            PrepareReturnRequestModel(model, returnRequest, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Edit(ReturnRequestModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            var returnRequest = _returnRequestService.GetReturnRequestById(model.Id);
            if (returnRequest == null)
                //No return request found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                returnRequest.CustomerComments = model.CustomerComments;
                returnRequest.StaffNotes = model.StaffNotes;
                returnRequest.ReturnRequestStatusId = model.ReturnRequestStatusId;
                returnRequest.UpdatedOnUtc = DateTime.UtcNow;
                returnRequest.PickupAddress = model.PickupAddress.ToEntity();

                _returnRequest.Update(returnRequest);
                //_customerService.UpdateCustomer(returnRequest.Customer);

                //activity log
                _customerActivityService.InsertActivity("EditReturnRequest", returnRequest.Id, _localizationService.GetResource("ActivityLog.EditReturnRequest"), returnRequest.Id);

                if (model.NotifyCustomer)
                    NotifyCustomer(model);

                SuccessNotification(_localizationService.GetResource("Admin.ReturnRequests.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = returnRequest.Id }) : RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
            PrepareReturnRequestModel(model, returnRequest, false);
            return View(model);
        }

        public IActionResult NotifyCustomer(ReturnRequestModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            var returnRequest = _returnRequestService.GetReturnRequestById(model.Id);
            if (returnRequest == null)
                //No return request found with the specified id
                return RedirectToAction("List");

            //var customer = returnRequest.Customer;
            var order = _orderService.GetOrderById(returnRequest.OrderId);
            int queuedEmailId = _workflowMessageService.SendReturnRequestStatusChangedCustomerNotification(returnRequest, order, _localizationSettings.DefaultAdminLanguageId);
            if (queuedEmailId > 0)
                SuccessNotification(_localizationService.GetResource("Admin.ReturnRequests.Notified"));
            return RedirectToAction("Edit", new { id = returnRequest.Id });
        }

        //delete
        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            var returnRequest = _returnRequestService.GetReturnRequestById(id);
            if (returnRequest == null)
                //No return request found with the specified id
                return RedirectToAction("List");

            _returnRequestService.DeleteReturnRequest(returnRequest);

            //activity log
            _customerActivityService.InsertActivity("DeleteReturnRequest", returnRequest.Id, _localizationService.GetResource("ActivityLog.DeleteReturnRequest"), returnRequest.Id);

            SuccessNotification(_localizationService.GetResource("Admin.ReturnRequests.Deleted"));
            return RedirectToAction("List");
        }

        #endregion
    }
}
