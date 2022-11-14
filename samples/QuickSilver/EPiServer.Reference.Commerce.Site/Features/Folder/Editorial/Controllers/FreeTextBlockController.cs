﻿using EPiServer.Cms.AspNetCore.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Folder.Editorial.Blocks;
using EPiServer.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Folder.Editorial.Controllers
{
    public class FreeTextBlockController : BlockComponent<FreeTextBlock>
    {
        protected override IViewComponentResult InvokeComponent(FreeTextBlock currentBlock)
        {
            return View(currentBlock);
        }
    }
}