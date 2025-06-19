using System.Net;
using Microsoft.AspNetCore.Mvc;
using Neoverse.ApiBase;
using Neoverse.ApiBase.Controllers;
using Neoverse.Customers.Domain.Entities;
using Neoverse.Customers.Infrastructure;
using Neoverse.Customers.Application;
using Neoverse.Customers.Api;
using Neoverse.Customers.Infrastructure.Client;
using Neoverse.Customers.Infrastructure.Client.Models;
using Neoverse.Customers.Infrastructure.Repository;
using Neoverse.SharedKernel.Constants;
using Neoverse.SharedKernel.Entities;

namespace Neoverse.Customers.Api.Controllers;

public class CustomersController(CustomerService customerService) : BaseController
{
    [HttpPost]
    public async Task<ApiResult<Customer>> Create(string name, string email)
    {
        var result = await customerService.CreateCustomerAsync(name, email);
        return result is null
            ? ApiResult<Customer>.Fail(MessageCodes.NotFound, Messages.NotFound) 
            : ApiResult<Customer>.Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ApiResult<Customer>> GetById(Guid id, [FromQuery] string? lang)
    {
        var result = await customerService.GetByIdAsync(id, lang);
        return result is null
            ? ApiResult<Customer>.Fail(MessageCodes.NotFound, Messages.NotFound) 
            : ApiResult<Customer>.Ok(result);
    }

    [HttpGet("{id}/translations")]
    public async Task<ApiResult<IEnumerable<Translation>>> GetTranslations(Guid id)
    {
        var result = await customerService.GetTranslationsAsync(id);
        return !result.Any() 
            ? ApiResult<IEnumerable<Translation>>.Fail(MessageCodes.NotFound, Messages.NotFound) 
            : ApiResult<IEnumerable<Translation>>.Ok(result);
    }

    [HttpGet("{id}/documents")]
    public async Task<ApiResult<IEnumerable<DocumentDto>>> GetDocuments(Guid id, [FromServices] DocumentClient docs)
    {
        var result = await docs.GetDocumentsForCustomerAsync(id);
        return !result.Any() 
            ? ApiResult<IEnumerable<DocumentDto>>.Fail(MessageCodes.NotFound, Messages.NotFound) 
            : ApiResult<IEnumerable<DocumentDto>>.Ok(result);
    }
}
