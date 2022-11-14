using EPiServer.Framework.Web;
using EPiServer.Web;
using Wangkanai.Detection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Wangkanai.Detection.Services;
using Wangkanai.Detection.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Channels;

namespace MyOptimizelySite.Business.Channels
{
    public class MobileDisplayChannel : DisplayChannel
    {
        public override bool IsActive(HttpContext context)
        {
            //Code uses package 'Wangkanai.Detection' for device detection
            var detection = context.RequestServices.GetRequiredService<IDetectionService>();
            return detection.Device.Type == Device.Mobile;
        }

        public override string ChannelName
        {
            get { return "Mobile"; }
        }

        public override string ResolutionId
        {
            get
            {
                return typeof(IphoneVerticalResolution).FullName;
            }
        }
    }
}