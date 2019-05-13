using System;
using FluentAssertions;
using Machete.Web.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Language.Flow;

namespace Machete.Test.UnitTests.Web.Tenancy
{
    [TestClass]
    public class TenantServiceTests
    {
        public ITenantService _tenantService;
        private Mock<IHttpContextAccessor> _httpContextAccessor;
        private Mock<ITenantIdentificationService> _tenantIdentificationService;
        private string _expectedTenant = "fakeTenant";
        private string _tenant;

        [TestMethod]
        public void GetCurrentTenant_ReturnsExpectedTenant()
        {
            GivenATenantService()
                .WithAnHttpContext()
                .WithATenantIdentificationService();

            WhenTenantServiceIsInstantiated()
                .AndGetCurrentTenantIsInvoked();

            ThenTenantMatchesExpected();
        }

        private TenantServiceTests ThenTenantMatchesExpected()
        {
            _tenant.Should().Match(_expectedTenant);
            return this;
        }

        private TenantServiceTests AndGetCurrentTenantIsInvoked()
        {
            _tenant = _tenantService.GetCurrentTenant();
            return this;
        }

        private TenantServiceTests WhenTenantServiceIsInstantiated()
        {
            _tenantService = new TenantService(_httpContextAccessor.Object, _tenantIdentificationService.Object);
            return this;
        }

        private TenantServiceTests WithATenantIdentificationService()
        {
            _tenantIdentificationService = new Mock<ITenantIdentificationService>();
            _tenantIdentificationService
                .Setup(service => service.GetCurrentTenant(_httpContextAccessor.Object.HttpContext))
                .Returns(_expectedTenant);
            return this;
        }

        private TenantServiceTests WithAnHttpContext()
        {
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _httpContextAccessor
                .Setup(accessor => accessor.HttpContext)
                .Returns(new DefaultHttpContext());
            return this;
        }

        private TenantServiceTests GivenATenantService()
        {
            // could do some fancy delegate work here and instantiate the type later, but... time...
            return this;
        }
    }
}
