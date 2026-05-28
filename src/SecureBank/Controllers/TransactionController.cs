using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureBank.Helpers.Authorization.Attributes;
using SecureBank.DAL.DBModels;
using SecureBank.DAL;
using SecureBank.Interfaces;
using SecureBank.Helpers;
using SecureBank.Filters;

namespace SecureBank.Controllers
{
    // Modified by Rezilant AI, 2026-05-28 21:35:41 GMT, Added DTO to prevent mass assignment vulnerability
    // Create a DTO with only allowed properties
    public class TransactionInputDto
    {
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public string Reference { get; set; }
        // Only include properties that should be user-modifiable
        // Exclude sensitive fields like: Id, SenderId, ReceiverId, TransactionDateTime, etc.
    }

    [AuthorizeNormal(AuthorizeAttributeTypes.Mvc)]

    public class TransactionController : MvcBaseContoller
    {
        private readonly PortalDBContext _context;

        private readonly ITransactionBL _transactionBL;

        public TransactionController(PortalDBContext context, ITransactionBL transactionBL)
        {
            _context = context;

            _transactionBL = transactionBL;
        }

        // GET: Transaction
        public IActionResult Index()
        {
            string viewName = _transactionBL.GetIndexViewName(HttpContext);

            return View(viewName);
        }

        // GET: Transaction/Details/5
        public IActionResult Details(int? id)
        {
            TransactionDBModel transaction = _transactionBL.Details(id, HttpContext);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transaction/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Transaction/Create
        // To protect from over posting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        // Modified by Rezilant AI, 2026-05-28 21:35:41 GMT, Replaced direct model binding with DTO to prevent mass assignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TransactionInputDto input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            // Manually map from DTO to domain model with server-side assignments
            var transaction = new TransactionDBModel
            {
                Amount = input.Amount,
                Reason = input.Reason,
                Reference = input.Reference,
                // Set protected properties explicitly in code - server-side assignment
                SenderId = GetCurrentUserId(), // Server-side assignment
                TransactionDateTime = System.DateTime.UtcNow // Server-side assignment
            };

            bool createResult = _transactionBL.Create(transaction, HttpContext);
            if (!createResult)
            {
                ModelState.AddModelError(string.Empty, "Error");
                return View(input);
            }

            return RedirectToAction(nameof(Index));
        }
        // Original Code
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Create([Bind("Id,SenderId,ReceiverId,TransactionDateTime,Reason,Amount,Reference")] TransactionDBModel transaction)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(transaction);
        //    }
        //
        //    bool createResult = _transactionBL.Create(transaction, HttpContext);
        //    if (!createResult)
        //    {
        //        ModelState.AddModelError(string.Empty, "Error");
        //        return View(transaction);
        //    }
        //
        //    return RedirectToAction(nameof(Index));
        //}

        // GET: Transaction/Edit/5
        [UnknownGeneration]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transactionTable = await _context.Transactions.FindAsync(id);
            if (transactionTable == null)
            {
                return NotFound();
            }

            return View(transactionTable);
        }

        // POST: Transaction/Edit/5
        // To protect from overpoting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [UnknownGeneration]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SenderId,ReceiverId,TransactionDateTime,Reason,Amount,Reference")] TransactionDBModel transactionTable)
        {

            if (id != transactionTable.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transactionTable);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionTableExists(transactionTable.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(transactionTable);
        }

        [UnknownGeneration]
        // GET: Transaction/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transactionTable = await _context.Transactions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transactionTable == null)
            {
                return NotFound();
            }

            return View(transactionTable);
        }

        // POST: Transaction/Delete/5
        [UnknownGeneration]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transactionTable = await _context.Transactions.FindAsync(id);
            _context.Transactions.Remove(transactionTable);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionTableExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }

        // Modified by Rezilant AI, 2026-05-28 21:35:41 GMT, Helper method to get current user ID for transaction creation
        private int GetCurrentUserId()
        {
            // Implementation depends on your authentication system
            // This is a placeholder - replace with actual user retrieval logic
            var userIdClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }
}