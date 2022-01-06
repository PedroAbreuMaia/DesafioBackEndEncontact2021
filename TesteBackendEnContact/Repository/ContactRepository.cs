using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;
using TesteBackendEnContact.Core.Interface.ContactBook.Contact;
using TesteBackendEnContact.Database;
using TesteBackendEnContact.Repository.Interface;
using LumenWorks.Framework.IO.Csv;
using System.Data;
using System.IO;
using System;

namespace TesteBackendEnContact.Repository
{
    public class ContactRepository : IContactRepository
    {
        private readonly DatabaseConfig databaseConfig;

        public ContactRepository(DatabaseConfig databaseConfig)
        {
            this.databaseConfig = databaseConfig;
        }

        public async Task<int[]> readFromFile(string fileName)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var csvTable = readCSV(fileName);   
            List<int> ids = new List<int>();

            string[] contactBookIds = await getContactBookIds().ConfigureAwait(false);
            string[] companyIds = await getCompanyIds().ConfigureAwait(false);

            for (int i = 0; i < csvTable.Rows.Count; i++)
            {
                ContactDao contactDao;
                ContactDao validator = new ContactDao();

                string ContactBookId = csvTable.Rows[i][0].ToString();
                string CompanyId = csvTable.Rows[i][1].ToString();
                string Name = csvTable.Rows[i][2].ToString();
                string Phone = csvTable.Rows[i][3].ToString();
                string Email = csvTable.Rows[i][4].ToString();
                string Address = csvTable.Rows[i][5].ToString();

                if (validator.validateContact(ContactBookId, Name) && contactBookIds.Contains(ContactBookId))
                {
                    if ( CompanyId.Equals("") || !companyIds.Contains(CompanyId) )
                    {
                        contactDao = new ContactDao(new Contact(0, int.Parse(ContactBookId), 0, Name, Phone, Email, Address));
                        contactDao.Id = await connection.InsertAsync(contactDao);
                        ids.Add(contactDao.Id);
                    } 
                    else
                    {
                        contactDao = new ContactDao(new Contact(0, int.Parse(ContactBookId), int.Parse(CompanyId), Name, Phone, Email, Address));
                        contactDao.Id = await connection.InsertAsync(contactDao);
                        ids.Add(contactDao.Id);
                    }                    
                }                
            }
            
            return ids.ToArray();
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

        public async Task<IEnumerable<IContact>> GetAllAsync(int low, int high)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM Contact LIMIT @low, @high";
            var result = await connection.QueryAsync<ContactDao>(query, new { low, high });

            return result?.Select(item => item.Export());
        }

        public async Task<IContact> GetAsync(int id)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM Contact WHERE Id = @id";
            var result = await connection.QuerySingleOrDefaultAsync<ContactDao>(query, new { id });

            return result?.Export();
        }

        public async Task<IEnumerable<IContact>> GetContactBookIdAsync(int id, int low, int high)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM Contact WHERE ContactBookId = @id LIMIT @low, @high";
            var result = await connection.QueryAsync<ContactDao>(query, new { id, low, high });

            return result?.Select(item => item.Export());
        }

        public async Task<IEnumerable<IContact>> GetCompanyIdAsync(int id, int low, int high)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM Contact WHERE CompanyId = @id LIMIT @low, @high";
            var result = await connection.QueryAsync<ContactDao>(query, new { id, low, high });

            return result?.Select(item => item.Export());
        }

        public async Task<IEnumerable<IContact>> GetNameAsync(string name, int low, int high)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM Contact WHERE Name = @name LIMIT @low, @high";
            var result = await connection.QueryAsync<ContactDao>(query, new { name, low, high });

            return result?.Select(item => item.Export());
        }

        public async Task<IEnumerable<IContact>> GetCompanyNameAsync(string name, int low, int high)
        {
            string id = await getCompanyIdFromNames(name);

            if( id.Equals("") )
            {
                return null;
            }

            return await GetCompanyIdAsync(int.Parse(id), low, high);
        }

        public async Task<IEnumerable<IContact>> GetPhoneAsync(string phone, int low, int high)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM Contact WHERE Phone = @phone LIMIT @low, @high";
            var result = await connection.QueryAsync<ContactDao>(query, new { phone, low, high });

            return result?.Select(item => item.Export());
        }

        public async Task<IEnumerable<IContact>> GetEmailAsync(string email, int low, int high)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM Contact WHERE Email = @email LIMIT @low, @high";
            var result = await connection.QueryAsync<ContactDao>(query, new { email, low, high });

            return result?.Select(item => item.Export());
        }

        public async Task<IEnumerable<IContact>> GetAdressAsync(string address, int low, int high)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM Contact WHERE Address = @address LIMIT @low, @high";
            var result = await connection.QueryAsync<ContactDao>(query, new { address, low, high });

            return result?.Select(item => item.Export());
        }

        public async Task<IEnumerable<IContact>> GetOfAContactBookTheContactsOfACompanyAsync(int contactbookid, int companyid, int low, int high)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM Contact WHERE ContactBookId = @contactbookid AND CompanyId = @companyid LIMIT @low, @high";
            var result = await connection.QueryAsync<ContactDao>(query, new { contactbookid, companyid, low, high });

            return result?.Select(item => item.Export());
        }
        private async Task<string[]> getContactBookIds() 
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM ContactBook";
            var result = await connection.QueryAsync<ContactBookDao>(query);
            List<string> ids = new List<string>();

            foreach (var r in result.ToList())
            {
                ids.Add(r.Id.ToString());
            }

            return ids.ToArray();
        }

        private async Task<string[]> getCompanyIds()
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM Company";
            var result = await connection.QueryAsync<CompanyDao>(query);
            List<string> ids = new List<string>();

            foreach (var r in result.ToList())
            {
                ids.Add(r.Id.ToString());
            }

            return ids.ToArray();
        }

        private async Task<string> getCompanyIdFromNames(string name)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM Company WHERE Name = @name";
            var result = await connection.QuerySingleOrDefaultAsync<CompanyDao>(query, new { name });
            
            return result.Id.ToString();
        }

        private DataTable readCSV(string fileName)
        {
            String folderPath = Path.Combine(Directory.GetCurrentDirectory(), "files/");
            DataTable csvTable = new DataTable();

            using (var csvReader = new CsvReader(new StreamReader(System.IO.File.OpenRead(folderPath + fileName)), true))
            {
                csvTable.Load(csvReader);
            }

            return csvTable;
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

        public bool validateContact(string contactBookId, string name)
        {
            if (contactBookId.Equals("") || name.Equals("") )
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
