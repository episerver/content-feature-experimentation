using EPiServer.Framework.Web;
using EPiServer.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Wangkanai.Detection.Services;
using Wangkanai.Detection.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Channels;

namespace MyOptimizelySite.Business.Channels
{
    public class WebDisplayChannel : DisplayChannel
    {
        public override bool IsActive(HttpContext context)
        {
            //Code uses package 'Wangkanai.Detection' for device detection
            var detection = context.RequestServices.GetRequiredService<IDetectionService>();
            return detection.Device.Type == Device.Desktop;
        }

        public override string ChannelName
        {
            get { return "Web"; }
        }

        public override string ResolutionId
        {
            get
            {
                return typeof(StandardResolution).FullName;
            }
        }
    }
}