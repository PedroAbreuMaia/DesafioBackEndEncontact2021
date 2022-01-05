using TesteBackendEnContact.Core.Interface.ContactBook.Contact;

namespace TesteBackendEnContact.Core.Domain.ContactBook.Contact
{
    public class Contact : IContact
    {
        public int Id { get; private set; }
        public int ContactBookId { get; private set; }
        public int CompanyId { get; private set; }
        public string Name { get; private set; }
        public string Phone { get; private set; }
        public string Email { get; private set; }
        public string Address { get; private set; }

        public Contact(int id, int contactBookId, int companyId, string name, string phone, string email, string address)
        {
            Id = id;
            ContactBookId = contactBookId;
            CompanyId = companyId;
            Name = name;
            Phone = phone;
            Email = email;
            Address = address;            
        }
    }
}
