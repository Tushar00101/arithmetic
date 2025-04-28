1>. feature branch with calc
2>. feature-add method addition
3>. feature-sub method subtract 
4>. feature-mul method multiplication
5>. feature-div method division
6>. feture-mod method modulous
7>. feature-add-ten method adding 10



[28/04, 7:28 am] Tushar: prder controller





using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Data;
using PharmacyManagementSystem.DTOs.Order;
using PharmacyManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PharmacyManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Both Admin and Doctor can access
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: api/order
        [HttpPost]
        [Authorize(Roles = "Doctor")] // Only Doctor can place Order
        public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderDto orderDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in orderDto.OrderItems)
            {
                var drug = await _context.Drugs.FindAsync(item.DrugId);
                if (drug == null)
                    return BadRequest($"Drug with ID {item.DrugId} not found.");

                totalAmount += item.Quantity * drug.Price;

                orderItems.Add(new OrderItem
                {
                    DrugId = drug.Id,
                    Quantity = item.Quantity,
                    UnitPrice = drug.Price
                });
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Status = "Pending",
                OrderItems = orderItems
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            return Ok("Order placed successfully.");
        }

        // GET: api/order
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(userId));

            IQueryable<Order> query = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Drug);

            if (userRoles.Contains("Doctor"))
            {
                // Doctor can see only their own orders
                query = query.Where(o => o.UserId == userId);
            }

            var orders = await query.ToListAsync();
            return Ok(orders);
        }

        // GET: api/order/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(userId));

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Drug)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound("Order not found.");

            if (userRoles.Contains("Doctor") && order.UserId != userId)
                return Forbid(); // Doctor can't view other Doctors' orders

            return Ok(order);
        }

        // PUT: api/order/{id}/verify
        [HttpPut("{id}/verify")]
        [Authorize(Roles = "Admin")] // Only Admin can verify orders
        public async Task<IActionResult> VerifyOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound("Order not found.");

            if (order.Status != "Pending")
                return BadRequest("Order is already verified or picked up.");

            order.Status = "Verified";
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Ok("Order verified successfully.");
        }

        // PUT: api/order/{id}/pickup
        [HttpPut("{id}/pickup")]
        [Authorize(Roles = "Admin")] // Only Admin can mark as picked up
        public async Task<IActionResult> MarkAsPickedUp(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound("Order not found.");

            if (order.Status != "Verified")
                return BadRequest("Order must be verified before pickup.");

            order.Status = "PickedUp";
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Ok("Order marked as picked up.");
        }
    }
}
[28/04, 7:28 am] Tushar: pickup controller





using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Data;
using System.Threading.Tasks;

namespace PharmacyManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only Admin can perform pickups
    public class PickupController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PickupController(ApplicationDbContext context)
        {
            _context = context;
        }

        // PUT: api/pickup/{orderId}
        [HttpPut("{orderId}")]
        public async Task<IActionResult> MarkOrderAsPickedUp(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Drug)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound("Order not found.");

            if (order.Status != "Verified")
                return BadRequest("Order must be verified before pickup.");

            // Decrease inventory quantities
            foreach (var item in order.OrderItems)
            {
                var inventory = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.DrugId == item.DrugId);

                if (inventory != null)
                {
                    inventory.CurrentQuantity -= item.Quantity;
                    if (inventory.CurrentQuantity < 0)
                        inventory.CurrentQuantity = 0; // Optional: Don't allow negative stock
                }
            }

            order.Status = "PickedUp";

            await _context.SaveChangesAsync();

            return Ok("Order marked as picked up and inventory updated successfully.");
        }
    }
}









using Microsoft.EntityFrameworkCore;

namespace PharmacyApp.RepositoryLayer.Context
{
	public class PharmacyDbContext : DbContext
	{
		public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<Supplier> Suppliers { get; set; }
		public DbSet<Drug> Drugs { get; set; }
		public DbSet<Inventory> Inventories { get; set; }
		public DbSet<Order> Orders { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// User and Role (One-to-Many)
			modelBuilder.Entity<User>()
				.HasOne(u => u.Role)
				.WithMany(r => r.Users)
				.HasForeignKey(u => u.RoleId);

			// Inventory and Drug (One-to-One)
			modelBuilder.Entity<Inventory>()
				.HasOne(i => i.Drug)
				.WithOne(d => d.Inventory)
				.HasForeignKey<Inventory>(i => i.DrugId);

			// Drug and Supplier (Many-to-One)
			modelBuilder.Entity<Drug>()
				.HasOne(d => d.Supplier)
				.WithMany(s => s.Drugs)
				.HasForeignKey(d => d.SupplierId);

			// Order and Drug (Many-to-One)
			modelBuilder.Entity<Order>()
				.HasOne(o => o.Drug)
				.WithMany(d => d.Orders)
				.HasForeignKey(o => o.DrugId);

			// Order and User (Many-to-One)
			modelBuilder.Entity<Order>()
				.HasOne(o => o.User)
				.WithMany(u => u.Orders)
				.HasForeignKey(o => o.UserId);
		}
	}
}
