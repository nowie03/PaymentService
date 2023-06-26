using Microsoft.EntityFrameworkCore;
using PaymentService.Models;

namespace PaymentService.Context
{
    public class ServiceContext:DbContext
    {
        public ServiceContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }

        public DbSet<Payment> Payments { get; set; }


    }
}
