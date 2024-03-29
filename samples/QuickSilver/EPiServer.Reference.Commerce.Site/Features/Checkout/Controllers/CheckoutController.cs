﻿using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    [ControllerExceptionFilter("purchase")]
    public class CheckoutController : PageController<CheckoutPage>
    {
        private readonly ICurrencyService _currencyService;
        private readonly CheckoutViewModelFactory _checkoutViewModelFactory;
        private readonly OrderSummaryViewModelFactory _orderSummaryViewModelFactory;
        private readonly IOrderRepository _orderRepository;
        private readonly ICartService _cartService;
        private readonly IRecommendationService _recommendationService;
        private readonly OrderValidationService _orderValidationService;
        private ICart _cart;
        private readonly CheckoutService _checkoutService;
        private readonly IContentRepository _contentRepository;
        private readonly IContentLoader _contentLoader;

        public CheckoutController(
            ICurrencyService currencyService,
            IOrderRepository orderRepository,
            CheckoutViewModelFactory checkoutViewModelFactory,
            ICartService cartService,
            OrderSummaryViewModelFactory orderSummaryViewModelFactory,
            IRecommendationService recommendationService,
            CheckoutService checkoutService,
            OrderValidationService orderValidationService,
            IContentRepository contentRepository,
            IContentLoader contentLoader)
        {
            _currencyService = currencyService;
            _orderRepository = orderRepository;
            _checkoutViewModelFactory = checkoutViewModelFactory;
            _cartService = cartService;
            _orderSummaryViewModelFactory = orderSummaryViewModelFactory;
            _recommendationService = recommendationService;
            _checkoutService = checkoutService;
            _orderValidationService = orderValidationService;
            _contentRepository = contentRepository;
            _contentLoader = contentLoader;
        }

        [HttpGet]
        [ResponseCache(Duration = 0, NoStore = true)]
        public async Task<ActionResult> Index(CheckoutPage currentPage)
        {
            if (CartIsNullOrEmpty())
            {
                return View("EmptyCart");
            }

            if (currentPage == null)
            {
                var currentPageContentReference = _contentLoader.Get<StartPage>(ContentReference.StartPage).CheckoutPage;
                currentPage = _contentRepository.Get<CheckoutPage>(currentPageContentReference);
            }

            var viewModel = CreateCheckoutViewModel(currentPage);

            Cart.Currency = _currencyService.GetCurrentCurrency();

            _checkoutService.UpdateShippingAddresses(Cart, viewModel);
            _checkoutService.UpdateShippingMethods(Cart, viewModel.Shipments);

            _cartService.ApplyDiscounts(Cart);
            _orderRepository.Save(Cart);

            await _recommendationService.TrackCheckoutAsync(HttpContext);

            _checkoutService.ProcessPaymentCancel(viewModel, TempData, ControllerContext);

            return View(viewModel.ViewName, viewModel);
        }

        [HttpGet]
        public ActionResult SingleShipment(CheckoutPage currentPage)
        {
            if (!CartIsNullOrEmpty())
            {
                _cartService.MergeShipments(Cart);
                _orderRepository.Save(Cart);
            }

            if (currentPage == null)
            {
                var currentPageContentReference = _contentLoader.Get<StartPage>(ContentReference.StartPage).CheckoutPage;
                currentPage = _contentRepository.Get<CheckoutPage>(currentPageContentReference);
            }

            return RedirectToAction("Index", new { node = currentPage.ContentLink });
        }

        [HttpPost]
        public ActionResult ChangeAddress(UpdateAddressViewModel addressViewModel)
        {
            ModelState.Clear();

            var viewModel = CreateCheckoutViewModel(addressViewModel.CurrentPage);

            // Set random value for Name/Id if null.
            if (addressViewModel.BillingAddress.AddressId == null)
            {
                addressViewModel.BillingAddress.Name = addressViewModel.BillingAddress.AddressId = Guid.NewGuid().ToString();
            }

            foreach (var shipment in addressViewModel.Shipments.Where(x => x.Address.AddressId == null))
            {
                shipment.Address.Name = shipment.Address.AddressId = Guid.NewGuid().ToString();
            }

            _checkoutService.CheckoutAddressHandling.ChangeAddress(viewModel, addressViewModel);

            _checkoutService.UpdateShippingAddresses(Cart, viewModel);

            _orderRepository.Save(Cart);

            var addressViewName = addressViewModel.ShippingAddressIndex > -1 ? "SingleShippingAddress" : "BillingAddress";

            return PartialView(addressViewName, viewModel);
        }

        [HttpGet]
        [HttpPost]
        public ActionResult OrderSummary()
        {
            return ViewComponent("Checkout");
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult AddCouponCode(CheckoutPage currentPage, string couponCode)
        {
            if (_cartService.AddCouponCode(Cart, couponCode))
            {
                _orderRepository.Save(Cart);
            }

            if (currentPage == null)
            {
                var currentPageContentReference = _contentLoader.Get<StartPage>(ContentReference.StartPage).CheckoutPage;
                currentPage = _contentRepository.Get<CheckoutPage>(currentPageContentReference);
            }

            var viewModel = CreateCheckoutViewModel(currentPage);
            return View(viewModel.ViewName, viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult RemoveCouponCode(CheckoutPage currentPage, string couponCode)
        {
            _cartService.RemoveCouponCode(Cart, couponCode);
            _orderRepository.Save(Cart);

            if (currentPage == null)
            {
                var currentPageContentReference = _contentLoader.Get<StartPage>(ContentReference.StartPage).CheckoutPage;
                currentPage = _contentRepository.Get<CheckoutPage>(currentPageContentReference);
            }

            var viewModel = CreateCheckoutViewModel(currentPage);
            return View(viewModel.ViewName, viewModel);
        }

        [HttpPost]
        public ActionResult Purchase(CheckoutViewModel viewModel, string currentPageReference, IPaymentMethod paymentMethod, Dictionary<string, string> additionalPaymentData)
        {
            if (CartIsNullOrEmpty())
            {
                return Redirect(Url.ContentUrl(ContentReference.StartPage));
            }
            viewModel.Payment = paymentMethod;

            viewModel.IsAuthenticated = User.Identity.IsAuthenticated;

            if (ContentReference.TryParse(currentPageReference, out var currentPageContentReference))
            {
                viewModel.CurrentPage = _contentRepository.Get<CheckoutPage>(currentPageContentReference);
            }

            _checkoutService.CheckoutAddressHandling.UpdateUserAddresses(viewModel);

            if (!_checkoutService.ValidateOrder(ModelState, viewModel, _orderValidationService.ValidateOrder(Cart)))
            {
                return View(viewModel);
            }

            if (!paymentMethod.ValidateData())
            {
                return View(viewModel);
            }

            _checkoutService.UpdateShippingAddresses(Cart, viewModel);

            _checkoutService.CreateAndAddPaymentToCart(Cart, viewModel, additionalPaymentData);

            var purchaseOrder = _checkoutService.PlaceOrder(Cart, ModelState, viewModel);
            if (!string.IsNullOrEmpty(viewModel.RedirectUrl))
            {
                return Redirect(viewModel.RedirectUrl);
            }

            if (purchaseOrder == null)
            {
                return View(viewModel);
            }

            var confirmationSentSuccessfully = _checkoutService.SendConfirmation(viewModel, purchaseOrder);
            var redirectUrl = _checkoutService.BuildRedirectionUrl(viewModel, purchaseOrder, confirmationSentSuccessfully);

            return paymentMethod.SystemKeyword.Equals("adyen", StringComparison.OrdinalIgnoreCase) ? Ok(redirectUrl) : Redirect(redirectUrl);
        }

        private ViewResult View(CheckoutViewModel checkoutViewModel)
        {
            return View(checkoutViewModel.ViewName, CreateCheckoutViewModel(checkoutViewModel.CurrentPage, checkoutViewModel.Payment));
        }

        private CheckoutViewModel CreateCheckoutViewModel(CheckoutPage currentPage, IPaymentMethod paymentMethod = null)
        {
            var checkoutViewModel = _checkoutViewModelFactory.CreateCheckoutViewModel(Cart, currentPage, paymentMethod);
            return checkoutViewModel;
        }

        private ICart Cart => _cart ?? (_cart = _cartService.LoadCart(_cartService.DefaultCartName));

        private bool CartIsNullOrEmpty()
        {
            return Cart == null || !Cart.GetAllLineItems().Any();
        }
    }
}
