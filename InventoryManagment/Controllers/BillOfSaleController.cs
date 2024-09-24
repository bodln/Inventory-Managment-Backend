using InventoryManagment.DTOs;
using InventoryManagment.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillOfSaleController : ControllerBase
    {
        private readonly IBillOfSaleRepository _repository;

        public BillOfSaleController(IBillOfSaleRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bills = await _repository.GetAllAsync();
            return Ok(bills);
        }

        [HttpGet("{GUID:guid}")]
        public async Task<IActionResult> GetByGuid(Guid GUID)
        {
            var bill = await _repository.GetByGUIDAsync(GUID);
            if (bill == null) return NotFound();
            return Ok(bill);
        }
        
        [HttpGet("Item/{GUID:guid}")]
        public async Task<IActionResult> GetByItemGuid(Guid GUID)
        {
            var bills = await _repository.GetByItemGuid(GUID);
            return Ok(bills);
        }

        [HttpPost]
        public async Task<IActionResult> Create(BillOfSaleDTO billOfSale)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var response = await _repository.AddAsync(token, billOfSale);
            return Ok(response);
        }

        [HttpPut("{GUID:guid}")]
        public async Task<IActionResult> Update(Guid GUID, BillOfSaleDTO billOfSaleDTO)
        {
            await _repository.UpdateAsync(GUID, billOfSaleDTO);
            return Ok();
        }

        [HttpDelete("{GUID:guid}")]
        public async Task<IActionResult> Delete(Guid GUID)
        {
            await _repository.DeleteAsync(GUID);
            return NoContent();
        }
    }
}
