namespace StoreOps.Tests.Activities;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using StoreOps.Api.Modules.Activities.Dtos;
using Xunit;

public sealed class BulkStatusEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public BulkStatusEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PatchBulkStatus_WithCreatedActivity_ReturnsUpdatedList()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/activities", new CreateActivityRequest
        {
            Title = "Compliance check freezer unit",
            ProgrammeId = Guid.NewGuid(),
            Category = "Compliance"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<CreatedActivity>();
        created.Should().NotBeNull();

        var patchRequest = new HttpRequestMessage(HttpMethod.Patch, "/api/activities/bulk-status")
        {
            Content = JsonContent.Create(new BulkStatusUpdateRequest
            {
                ActivityIds = new List<Guid> { created!.Id },
                Status = "Done",
                ActorId = Guid.NewGuid()
            })
        };

        var bulkResponse = await _client.SendAsync(patchRequest);
        bulkResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await bulkResponse.Content.ReadFromJsonAsync<BulkStatusUpdateResult>();
        body!.Updated.Should().Contain(created.Id);
        body.Failed.Should().BeEmpty();
    }

    [Fact]
    public async Task PatchBulkStatus_WithInvalidStatus_ReturnsBadRequest()
    {
        var patchRequest = new HttpRequestMessage(HttpMethod.Patch, "/api/activities/bulk-status")
        {
            Content = JsonContent.Create(new BulkStatusUpdateRequest
            {
                ActivityIds = new List<Guid> { Guid.NewGuid() },
                Status = "NotAStatus",
                ActorId = Guid.NewGuid()
            })
        };

        var response = await _client.SendAsync(patchRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed class CreatedActivity
    {
        public Guid Id { get; set; }
    }
}
