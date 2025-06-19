using Microsoft.AspNetCore.Mvc;
using Neoverse.ApiBase;
using Neoverse.ApiBase.Controllers;
using Neoverse.DocumentManagement.Application;
using Neoverse.DocumentManagement.Domain.Entities;
using Neoverse.SharedKernel.Constants;
using Neoverse.SharedKernel.Entities;

namespace Neoverse.DocumentManagement.Api.Controllers;

public class DocumentsController(DocumentService documentService) : BaseController
{
    [HttpGet("customer/{customerId}")]
    public async Task<ApiResult<IEnumerable<Document>>> GetForCustomer(Guid customerId)
    {
        var result = await documentService.GetDocumentsForCustomerAsync(customerId);
        return !result.Any() 
            ? ApiResult<IEnumerable<Document>>.Fail(MessageCodes.NotFound, Messages.NotFound) 
            : ApiResult<IEnumerable<Document>>.Ok(result);    }

    [HttpGet("{id}")]
    public async Task<ApiResult<Document>> GetById(Guid id, [FromQuery] string? lang)
    {
        var result = await documentService.GetByIdAsync(id, lang);
        return result is null
            ? ApiResult<Document>.Fail(MessageCodes.NotFound, Messages.NotFound) 
            : ApiResult<Document>.Ok(result);    }

    [HttpGet("{id}/translations")]
    public async Task<ApiResult<IEnumerable<Translation>>> GetTranslations(Guid id)
    {
        var result = await documentService.GetTranslationsAsync(id);
        return !result.Any() 
            ? ApiResult<IEnumerable<Translation>>.Fail(MessageCodes.NotFound, Messages.NotFound) 
            : ApiResult<IEnumerable<Translation>>.Ok(result);    }
}
