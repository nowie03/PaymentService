using Microsoft.EntityFrameworkCore;
using PaymentService.Models;

namespace PaymentService.Context
{
    public class ServiceContext : DbContext
    {
        public ServiceContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Message> Outbox { get; set; }
        public DbSet<ConsumedMessage> ConsumedMessages { get; set; }

        override
        protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConsumedMessage>().HasIndex(message => message.MessageId).IsUnique();


        }
    }
}
