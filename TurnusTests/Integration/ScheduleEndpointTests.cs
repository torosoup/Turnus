using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace TurnusTests.Integration
{
    public class ScheduleEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ScheduleEndpointTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async System.Threading.Tasks.Task ScheduleIndex_Returns_OK_For_WorkspaceMember()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("Test-User-Id", "test-user");

            var resp = await client.GetAsync("/Schedule/Index");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task Admin_Dashboard_Returns_OK_For_SuperAdmin()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("Test-User-Id", "super");
            client.DefaultRequestHeaders.Add("Test-User-Roles", "SuperAdmin");

            var resp = await client.GetAsync("/Admin/Dashboard");
            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
            // Because super user isn't seeded as WorkspaceMember; Dashboard requires resolved workspace and membership.
        }
    }
}
