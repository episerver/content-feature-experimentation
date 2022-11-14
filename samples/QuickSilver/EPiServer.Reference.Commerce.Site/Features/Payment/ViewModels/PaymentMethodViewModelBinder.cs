using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels
{
    public class PaymentMethodViewModelBinder : IModelBinder
    {
        private readonly IEnumerable<IPaymentMethod> _paymentMethods;
        private readonly IModelMetadataProvider _defaultProvider;

        public PaymentMethodViewModelBinder(IEnumerable<IPaymentMethod> paymentMethods, IModelMetadataProvider defaultProvider)
        {
            _paymentMethods = paymentMethods;
            _defaultProvider = defaultProvider;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var systemKeyword = bindingContext.ValueProvider.GetValue("SystemKeyword").FirstOrDefault();
            var selectedPaymentMethod = _paymentMethods.FirstOrDefault(p => !string.IsNullOrEmpty(p.SystemKeyword) && p.SystemKeyword.ToString() == systemKeyword);
            if (selectedPaymentMethod != null)
            {
                var modelType = selectedPaymentMethod.GetType();
                var model = ActivatorUtilities.CreateInstance(ServiceLocator.Current, modelType);
                bindingContext.ModelMetadata = _defaultProvider.GetMetadataForType(modelType);
                bindingContext.Result = ModelBindingResult.Success(model);
            }

            return Task.CompletedTask;
        }
    }
}