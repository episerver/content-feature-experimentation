﻿using EPiServer.ServiceLocation;
using Moq;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Core;
using System;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class UIHelperTests
    {
        private Mock<IServiceProvider> _serviceLocator;
        private Mock<IContentRepository> _contentrepository;

        private UIHelper GetUnitUnderTest()
        {
            _contentrepository = new Mock<IContentRepository>();
            _contentrepository.Setup(cr => cr.Get<IContent>(It.IsAny<Guid>())).Throws<NotSupportedException>();
            _serviceLocator = new Mock<IServiceProvider>();
            _serviceLocator.Setup(sl => sl.GetService(typeof(IContentRepository))).Returns(_contentrepository.Object);

            return new UIHelper(_serviceLocator.Object);
        }

        [Fact]
        public void Get_ContentCallsServiceLocator()
        {
            var helper = GetUnitUnderTest();
            Guid theGuid = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE8");
            BasicContent tc = new BasicContent();

            _contentrepository.Setup(cr => cr.Get<IContent>(It.Is<Guid>(guid => guid.Equals(theGuid)))).Returns(tc);
            helper.getContent(theGuid);

            _serviceLocator.Verify(sl => sl.GetService(typeof(IContentRepository)), Times.Once, "GetInstance was never called");
        }

        [Fact]
        public void Get_ContentCallsContentRepository()
        {
            Guid theGuid = Guid.NewGuid();
            GetUnitUnderTest().getContent(theGuid);
            _contentrepository.Verify(cr => cr.Get<IContent>(It.Is<Guid>(arg => arg.Equals(theGuid))), Times.Once, "content repository get was never called");
        }

        [Fact]
        public void Get_ContentCallsContentRepositoryAndReturnsContentNotFound()
        {
            Guid theGuid = Guid.NewGuid();
            IContent content = GetUnitUnderTest().getContent(theGuid);

            _contentrepository.Verify(cr => cr.Get<IContent>(It.Is<Guid>(arg => arg.Equals(theGuid))), Times.Once, "content repository get was never called");

            // Now verify the name of the content returned (should be what the api specifies - ContentNotFound)
            Assert.Equal("ContentNotFound", content.Name, false);
        }
    }
}
