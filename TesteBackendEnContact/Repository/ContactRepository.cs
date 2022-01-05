using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;
using TesteBackendEnContact.Core.Interface.ContactBook.Contact;
using TesteBackendEnContact.Database;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Repository
{
    public class ContactRepository : IContactRepository
    {
        private readonly DatabaseConfig databaseConfig;

        public ContactRepository(DatabaseConfig databaseConfig)
        {
            this.databaseConfig = databaseConfig;
        }

        public async Task<IContact> SaveAsync(IContact contact)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            var dao = new ContactDao(contact);

            if (dao.Id == 0)
                dao.Id = await connection.InsertAsync(dao);
            else
                await connection.UpdateAsync(dao);

            return dao.Export();
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            var sql = "DELETE FROM Contact WHERE Id = @id;";

            await connection.ExecuteAsync(sql.ToString(), new { id }, transaction);

            transaction.Commit();
            connection.Close();
        }

        public async Task<IEnumerable<IContact>> GetAllAsync()
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM Contact";
            var result = await connection.QueryAsync<ContactDao>(query);

            return result?.Select(item => item.Export());
        }

        public async Task<IContact> GetAsync(int id)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM Contact where Id = @id";
            var result = await connection.QuerySingleOrDefaultAsync<ContactDao>(query, new { id });

            return result?.Export();
        }
    }

    [Table("Contact")]
    public class ContactDao : IContact
    {
        [Key]
        public int Id { get; set; }
        public int ContactBookId { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public ContactDao()
        {
        }

        public ContactDao(IContact contact)
        {
            Id = contact.Id;
            ContactBookId = contact.ContactBookId;
            CompanyId = contact.CompanyId;
            Name = contact.Name;
            Phone = contact.Phone;
            Email = contact.Email;
            Address = contact.Address;
        }

        public IContact Export() => new Contact(Id, ContactBookId, CompanyId, Name, Phone, Email, Address);
    }
}
