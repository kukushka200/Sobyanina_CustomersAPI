using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CustomersAPI.Data;
using CustomersAPI.Models;

namespace CustomersAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context)
        {
            _context = context;
        }

        public class CustomerDto
        {
            public long Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
        }
// DTO для создания нового клиента
        public class CreateCustomerDto
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
        }
//  DTO для обновления существующего клиента
        public class UpdateCustomerDto
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
        }
   // Получить список всех клиентов
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"User ID: {userId}");

            var customers = await _context.Customers
                .OrderBy(c => c.Id)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Address = c.Address
                })
                .ToListAsync();

            return Ok(customers);
        }
  // Получить клиента по ID
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomer(long id)
        {
            var customer = await _context.Customers
                .Where(c => c.Id == id)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Address = c.Address
                })
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                return NotFound(new { message = $"Клиент с ID {id} не найден" });
            }

            return Ok(customer);
        }
    // Создать нового клиента
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto dto)
        {
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == dto.Email);

            if (existingCustomer != null)
            {
                return BadRequest(new { message = "Клиент с таким email уже существует" });
            }

            var customer = new Customer
            {
                Name = dto.Name,
                Email = dto.Email,
                Address = dto.Address
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var result = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Address = customer.Address
            };

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, result);
        }
  // Обновить существующего клиента
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(long id, UpdateCustomerDto dto)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound(new { message = $"Клиент с ID {id} не найден" });
            }

            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != customer.Email)
            {
                var emailExists = await _context.Customers
                    .AnyAsync(c => c.Email == dto.Email && c.Id != id);

                if (emailExists)
                {
                    return BadRequest(new { message = "Клиент с таким email уже существует" });
                }
                customer.Email = dto.Email;
            }

            if (!string.IsNullOrEmpty(dto.Name))
                customer.Name = dto.Name;

            if (!string.IsNullOrEmpty(dto.Address))
                customer.Address = dto.Address;

            await _context.SaveChangesAsync();
            return NoContent();
        }
     // Удалить клиента
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(long id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound(new { message = $"Клиент с ID {id} не найден" });
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> Search([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { message = "Параметр name обязателен" });
            }

            var customers = await _context.Customers
                .Where(c => c.Name.Contains(name))
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Address = c.Address
                })
                .ToListAsync();

            return Ok(customers);
        }

        [HttpGet("by-address/{address}")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> ByAddress(string address)
        {
            var customers = await _context.Customers
                .Where(c => c.Address == address)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Address = c.Address
                })
                .ToListAsync();

            return Ok(customers);
        }
    }
}