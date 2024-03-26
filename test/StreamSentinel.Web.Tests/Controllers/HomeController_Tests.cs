using System.Threading.Tasks;
using StreamSentinel.Models.TokenAuth;
using StreamSentinel.Web.Controllers;
using Shouldly;
using Xunit;

namespace StreamSentinel.Web.Tests.Controllers
{
    public class HomeController_Tests: StreamSentinelWebTestBase
    {
        [Fact]
        public async Task Index_Test()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = "123qwe"
            });

            //Act
            var response = await GetResponseAsStringAsync(
                GetUrl<HomeController>(nameof(HomeController.Index))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}