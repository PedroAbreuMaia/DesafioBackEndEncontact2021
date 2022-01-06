using System.Collections.Generic;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Interface.ContactBook.Contact;

namespace TesteBackendEnContact.Repository.Interface
{
    public interface IContactRepository
    {
        Task<int[]> readFromFile(string fileName);
        Task<IContact> SaveAsync(IContact contact);
        Task DeleteAsync(int id);
        Task<IEnumerable<IContact>> GetAllAsync(int low, int high);
        Task<IContact> GetAsync(int id);
        Task<IEnumerable<IContact>> GetContactBookIdAsync(int id, int low, int high);
        Task<IEnumerable<IContact>> GetCompanyIdAsync(int id, int low, int high);
        Task<IEnumerable<IContact>> GetNameAsync(string name, int low, int high);
        Task<IEnumerable<IContact>> GetCompanyNameAsync(string name, int low, int high);
        Task<IEnumerable<IContact>> GetPhoneAsync(string phone, int low, int high);
        Task<IEnumerable<IContact>> GetEmailAsync(string email, int low, int high);
        Task<IEnumerable<IContact>> GetAdressAsync(string address, int low, int high);
        Task<IEnumerable<IContact>> GetOfAContactBookTheContactsOfACompanyAsync(int ContactBookId, int CompanyId, int low, int high);
    }
}
