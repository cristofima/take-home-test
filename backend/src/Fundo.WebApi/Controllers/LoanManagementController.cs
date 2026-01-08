using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Fundo.Application.Interfaces;
using Fundo.Application.DTO;

namespace Fundo.WebApi.Controllers
{
    /// <summary>
    /// Manages loan operations including creation, retrieval, and payment processing
    /// </summary>
    [ApiController]
    [Route("api/loans")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    public class LoanManagementController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoanManagementController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        /// <summary>
        /// Retrieves all loans from the system
        /// </summary>
        /// <returns>A list of all loans ordered by creation date (newest first)</returns>
        /// <response code="200">Returns the list of loans</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LoanResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllLoans()
        {
            var loans = await _loanService.GetAllLoansAsync();
            return Ok(loans);
        }

        /// <summary>
        /// Retrieves a specific loan by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the loan</param>
        /// <returns>The loan details if found</returns>
        /// <response code="200">Returns the requested loan</response>
        /// <response code="404">If the loan is not found</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        /// Creates a new loan with the specified amount and applicant name
        /// </summary>
        /// <param name="request">The loan creation request containing amount and applicant name</param>
        /// <returns>The newly created loan</returns>
        /// <response code="201">Returns the newly created loan</response>
        /// <response code="400">If the request is invalid</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/loans
        ///     {
        ///        "amount": 25000.00,
        ///        "applicantName": "John Doe"
        ///     }
        /// 
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        /// Processes a payment for an existing loan, reducing the current balance
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the loan</param>
        /// <param name="request">The payment request containing the payment amount</param>
        /// <returns>The updated loan with new balance</returns>
        /// <response code="200">Returns the updated loan</response>
        /// <response code="400">If the payment amount is invalid or exceeds the current balance</response>
        /// <response code="404">If the loan is not found</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/loans/{id}/payment
        ///     {
        ///        "amount": 1000.00
        ///     }
        /// 
        /// When the payment brings the balance to zero, the loan status automatically changes to "paid"
        /// </remarks>
        [HttpPost("{id:guid}/payment")]
        [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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