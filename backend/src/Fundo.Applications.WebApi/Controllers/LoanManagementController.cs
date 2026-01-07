using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Fundo.Application.Interfaces;
using Fundo.Application.DTO;

namespace Fundo.Applications.WebApi.Controllers
{
    [ApiController]
    [Route("api/loans")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        /// <summary>
        /// GET /api/loans - List all loans
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllLoans()
        {
            var loans = await _loanService.GetAllLoansAsync();
            return Ok(loans);
        }

        /// <summary>
        /// GET /api/loans/{id} - Retrieve loan details
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetLoanById(Guid id)
        {
            var loan = await _loanService.GetLoanByIdAsync(id);
            
            if (loan == null)
            {
                return NotFound(new { message = $"Loan with ID {id} not found" });
            }

            return Ok(loan);
        }

        /// <summary>
        /// POST /api/loans - Create a new loan
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] CreateLoanRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var loan = await _loanService.CreateLoanAsync(request);
                return CreatedAtAction(nameof(GetLoanById), new { id = loan.Id }, loan);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/loans/{id}/payment - Process payment (deduct from currentBalance)
        /// </summary>
        [HttpPost("{id:guid}/payment")]
        public async Task<IActionResult> ProcessPayment(Guid id, [FromBody] PaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var loan = await _loanService.ProcessPaymentAsync(id, request);
                return Ok(loan);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}