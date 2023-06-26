using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public Constants.Enums.PaymentStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
