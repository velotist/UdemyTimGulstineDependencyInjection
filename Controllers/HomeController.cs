using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace UdemyTimGulestineDependencyInjection.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            CustomerService customerService = new CustomerService();
            IEnumerable<Customer> customers = customerService.GetCustomers();

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
            Customer customer = new Customer
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Address = reader.GetString(2)
            };


            return customer;
        }
    }

    public class Repository<T>
    {
        public IEnumerable<T> Get(string commandText, Func<SqlDataReader, T> mappingFunction)
        {
            List<T> list = new List<T>();
            using (SqlConnection connection = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=Accounting;Trusted_Connection=True;MultipleActiveResultSets=False"))
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

    public class CustomerService
    {
        public IEnumerable<Customer> GetCustomers()
        {
            Repository<Customer> customerRepository = new Repository<Customer>();

            return customerRepository.Get("SELECT Id, Name, Address FROM Customers", Mappers.GetCustomers);
        }
    }
}
