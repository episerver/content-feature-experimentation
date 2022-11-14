using EPiServer.Commerce.UI.CustomerService.Extensibility;
using EPiServer.Commerce.UI.CustomerService.Routing;
using Microsoft.AspNetCore.Mvc;

namespace EPiServer.Reference.Commerce.Site.CSRExtensibility.Controllers
{
    [EpiRoutePrefix("csr-demo")]
    public class DemoApiController: CSRAPIController
    {
        [HttpGet]
        [EpiRoute("getData")]
        public IActionResult Get()
        {
            return Ok("Sample data");
        }
    }
}