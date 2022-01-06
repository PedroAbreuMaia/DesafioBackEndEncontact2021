using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;
using TesteBackendEnContact.Core.Interface.ContactBook.Contact;
using TesteBackendEnContact.Repository.Interface;
using System;

namespace TesteBackendEnContact.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ILogger<ContactController> _logger;

        public ContactController(ILogger<ContactController> logger)
        {
            _logger = logger;
        }


        [HttpPost("{fileName}")]
        public async Task<int[]> Post(string fileName, [FromServices] IContactRepository contactRepository)
        {
            return await contactRepository.readFromFile(fileName);
        }

        [HttpPost]
        public async Task<IContact> Post(Contact contact, [FromServices] IContactRepository contactRepository)
        {
            return await contactRepository.SaveAsync(contact);
        }

        private Task WriteFile(IFormFile file)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        public async Task Delete(int id, [FromServices] IContactRepository contactRepository)
        {
            await contactRepository.DeleteAsync(id);
        }

        [HttpGet("GetAll/{low:int}/{high:int}")]
        public async Task<IEnumerable<IContact>> Get(int low, int high, [FromServices] IContactRepository contactRepository)
        {
            return await contactRepository.GetAllAsync(low, high);
        }

        [HttpGet("{id}")]
        public async Task<IContact> Get(int id, [FromServices] IContactRepository contactRepository)
        {
            return await contactRepository.GetAsync(id);
        }

        [HttpGet("GetValues/{type:int}/{input}/{low:int}/{high:int}")]
        public async Task<IEnumerable<IContact>> GetValues(int type, string input, int low, int high, [FromServices] IContactRepository contactRepository)
        {
            switch (type)
            {
                case 1:
                    return await contactRepository.GetContactBookIdAsync(int.Parse(input), low, high);
                case 2:
                    return await contactRepository.GetCompanyIdAsync(int.Parse(input), low, high);
                case 3:
                    return await contactRepository.GetNameAsync(input, low, high);
                case 4: 
                    return await contactRepository.GetCompanyNameAsync(input, low, high);
                case 5:
                    return await contactRepository.GetPhoneAsync(input, low, high);
                case 6:
                    return await contactRepository.GetEmailAsync(input, low, high);
                case 7:
                    return await contactRepository.GetAdressAsync(input, low, high);
                default: 
                    throw new ArgumentOutOfRangeException(nameof(type));   

            }
        }

        [HttpGet("GetValues/{ContactBookId:int}/{CompanyId:int}/{low:int}/{high:int}")]
        public async Task<IEnumerable<IContact>> GetValues(int ContactBookId, int CompanyId, int low, int high, [FromServices] IContactRepository contactRepository)
        {
            return await contactRepository.GetOfAContactBookTheContactsOfACompanyAsync(ContactBookId, CompanyId, low, high);
        }
    }
}
