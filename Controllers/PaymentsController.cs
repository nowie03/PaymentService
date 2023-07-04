using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PaymentService.Context;
using PaymentService.MessageBroker;
using PaymentService.Models;

namespace PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ServiceContext _context;
        private readonly IMessageBrokerClient _rabbitMQClient;

        public PaymentsController(ServiceContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _rabbitMQClient = serviceProvider.GetRequiredService<IMessageBrokerClient>();
        }

        // GET: api/Payments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
        {
            if (_context.Payments == null)
            {
                return NotFound();
            }
            return await _context.Payments.ToListAsync();
        }

        // GET: api/Payments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            if (_context.Payments == null)
            {
                return NotFound();
            }
            var payment = await _context.Payments.FindAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            return payment;
        }

        // PUT: api/Payments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPayment(int id, Payment payment)
        {
            if (id != payment.Id)
            {
                return BadRequest();
            }

            //check if payment status is payment completed
            //if yes then initiate a shipment
            if (payment.Status == Constants.Enums.PaymentStatus.COMPLETED)
            {
                string serializedPayment = JsonConvert.SerializeObject(payment);
                ulong nextSequenceNumber = _rabbitMQClient.GetNextSequenceNumber();

                Message outboxMessage = new(Constants.EventTypes.PAYMENT_COMPLETED, serializedPayment,
                    nextSequenceNumber, Constants.EventStates.EVENT_ACK_PENDING);

                await _context.Outbox.AddAsync(outboxMessage);
                await _context.SaveChangesAsync();
            }

            _context.Entry(payment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Payments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Payment>> PostPayment(Payment payment)
        {
            if (_context.Payments == null)
            {
                return Problem("Entity set 'ServiceContext.Payments'  is null.");
            }
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPayment", new { id = payment.Id }, payment);
        }

        // DELETE: api/Payments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            if (_context.Payments == null)
            {
                return NotFound();
            }
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PaymentExists(int id)
        {
            return (_context.Payments?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
