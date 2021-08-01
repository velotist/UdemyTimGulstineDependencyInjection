using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace UdemyTimGulstineDependencyInjection.Controllers
{
    public class HomeController : Controller
    {
        private ICustomerService CustomerService { get; set; }

        public HomeController(ICustomerService customerService)
        {
            CustomerService = customerService;
        }

        public IActionResult Index()
        {
            IEnumerable<Customer> customers = CustomerService.GetCustomers();

            return View(customers);
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public static class Mappers
    {
        public static Customer GetCustomers(SqlDataReader reader)
        {
            var customer = new Customer
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Address = reader.GetString(2)
            };


            return customer;
        }
    }

    public class ConfigurationSettings : IConfigurationSettings
    {
        public string ConnectionString { get; set; }
    }

    public interface IConfigurationSettings
    {
        string ConnectionString { get; set; }
    }

    public interface IRepository<T>
    {
        IConfigurationSettings ConfigurationSettings { get; set; }
        IEnumerable<T> Get(string query, Func<SqlDataReader, T> mappingFunction);
    }

    public class Repository<T> : IRepository<T>
    {
        public IConfigurationSettings ConfigurationSettings { get; set; }
        
        public Repository(IConfigurationSettings configurationSettings)
        {
            ConfigurationSettings = configurationSettings;
        }

        public IEnumerable<T> Get(string commandText, Func<SqlDataReader, T> mappingFunction)
        {
            var list = new List<T>();
            using (SqlConnection connection = new SqlConnection(ConfigurationSettings.ConnectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            list.Add(mappingFunction(reader));
                        }
                    }
                }
                connection.Close();
            }

            return list;
        }
    }

    public interface ICustomerService
    {
        IEnumerable<Customer> GetCustomers();
    }

    public class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer> _customerRepository;

        public CustomerService(IRepository<Customer> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public IEnumerable<Customer> GetCustomers()
        {
            return _customerRepository.Get("SELECT Id, Name, Address FROM Customers", Mappers.GetCustomers);
        }
    }
}
