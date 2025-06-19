using System.Net.Http.Json;
using Neoverse.Customers.Infrastructure.Client.Models;

namespace Neoverse.Customers.Infrastructure.Client;

public class DocumentClient
{
    private readonly HttpClient _client;

    public DocumentClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<DocumentDto>> GetDocumentsForCustomerAsync(Guid id, CancellationToken ct = default)
    {
        var docs = await _client.GetFromJsonAsync<IEnumerable<DocumentDto>>($"/api/documents/customer/{id}", ct);
        return docs ?? Enumerable.Empty<DocumentDto>();
    }
}
