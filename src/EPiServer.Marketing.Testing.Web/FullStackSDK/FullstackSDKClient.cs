using Microsoft.AspNetCore.Http;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.Options;
using OptimizelySDK.Entity;
using System;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullSttack;

namespace EPiServer.Marketing.Testing.Web.FullStackSDK
{
    public partial class FullstackSDKClient : IFullstackSDKClient
    {
        private readonly HttpContext _httpContext;
        private readonly CookieService _cookieService;
        public FullstackSDKClient(CookieService cookieService)
        {
            _cookieService = cookieService;
            _httpContext = ServiceLocator.Current.GetInstance<IHttpContextAccessor>().HttpContext;
        }

        public bool TrackPageViewEvent(string eventName, int itemVersion)
        {
            var userContext = GetUserContext();
            if (userContext == null)
                return false;

            userContext.TrackEvent(eventName); //pass event name

            return true;
        }
        public bool LogUserDecideEvent(string flagName, out string variationKey)
        {
            var userContext = GetUserContext();
            if (userContext == null)
            {
                variationKey = string.Empty;
                return false;
            }
            var decision = userContext.Decide(flagName);//pass flag name

            variationKey = decision.VariationKey;

            return true;
        }

        private OptimizelySDK.OptimizelyUserContext GetUserContext()
        {
            var userInCookie = _cookieService.Get(FullStackConstants.FullStackUserGUID);

            if (string.IsNullOrEmpty(userInCookie)) {
                userInCookie = Guid.NewGuid().ToString();
                _cookieService.Set(FullStackConstants.FullStackUserGUID, userInCookie);
            }
            var client = FSExpClient.Get.Value;
            var user = client.CreateUserContext(userInCookie, null);

            return user;
        }

        //private UserAttributes GetUserAttribute(out string userId)
        //{
        //    var userAttributes = new OptimizelySDK.Entity.UserAttributes();
        //    _httpContext.Request.Cookies.TryGetValue("FullStackUserGUID", out userId);
        //    if (!string.IsNullOrEmpty(userId))
        //    {
        //        userAttributes.Add("FullStackUserGUID", userId);
        //    }
        //    return userAttributes;
        //}

        #region commented code

        //public ExperimentBanner GetBannerBasedOnExperiment()
        //{
        //    var userContext = GetUserContext();
        //    if (userContext == null)
        //        return null;

        //    userContext.TrackEvent("Recorded views");
        //    var decision = userContext.Decide("banner");
        //    if (!decision.Enabled)
        //        return null;

        //    var output = new ExperimentBanner
        //    {
        //        Text = decision.Variables.GetValue<string>("banner_text"),
        //        BackgroundColor = decision.Variables.GetValue<string>("background_color"),
        //        TextColor = decision.Variables.GetValue<string>("text_color")
        //    };
        //    return output;
        //}

        //public ExperimentProductListing GetProductListingBasedOnExperiment()
        //{
        //    var userContext = GetUserContext();
        //    if (userContext == null)
        //        return new ExperimentProductListing();

        //    userContext.TrackEvent("Recorded views");
        //    var decision = userContext.Decide("product_listing");
        //    if (!decision.Enabled)
        //        return new ExperimentProductListing();

        //    var numberOfResultsPerRow = decision.Variables.GetValue<int>("number_of_products_per_row");
        //    if (numberOfResultsPerRow < 1 || numberOfResultsPerRow > 4)
        //        numberOfResultsPerRow = 4;

        //    var numberOfResultsPerMobileRow = decision.Variables.GetValue<int>("number_of_products_per_mobile_row");
        //    if (numberOfResultsPerMobileRow < 1 || numberOfResultsPerMobileRow > 4)
        //        numberOfResultsPerMobileRow = 1;

        //    var output = new ExperimentProductListing
        //    {
        //        BootstrapSizeNormal = GetBootstrapSize(numberOfResultsPerRow, "lg"),
        //        NumberOfResultsPerMobileRow = GetBootstrapSize(numberOfResultsPerMobileRow, "sm"),
        //    };
        //    return output;

        //    string GetBootstrapSize(int input, string prefix)
        //    {
        //        if (input == 4)
        //            return $"col-{prefix}-3";
        //        else if (input == 3)
        //            return $"col-{prefix}-4";
        //        else if (input == 2)
        //            return $"col-{prefix}-6";

        //        return $"col-{prefix}-12";
        //    }
        //}
        #endregion

    }
}
