using InventoryManagment.DTOs;
using InventoryManagment.Models;
using InventoryManagment.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryRepository _repository;

        public InventoryController(IInventoryRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var inventories = await _repository.GetAllAsync();
            return Ok(inventories);
        }

        [HttpGet("{GUID:guid}")]
        public async Task<IActionResult> GetByGuid(Guid GUID)
        {
            var inventory = await _repository.GetByGuidAsync(GUID);
            if (inventory == null) return NotFound();
            return Ok(inventory);
        }

        [HttpGet("Stock")]
        public async Task<IActionResult> GetStockForItemsAsync()
        {
            var itemStock = await _repository.GetStockForItemsAsync();
            if (itemStock == null) return NotFound();
            return Ok(itemStock);
        }

        [HttpGet("Stock/{GUID:guid}")]
        public async Task<IActionResult> GetStockForItemAsync(Guid GUID)
        {
            var itemStock = await _repository.GetStockForItemAsync(GUID);
            if (itemStock == null) return NotFound();
            return Ok(itemStock);
        }

        [HttpPost]
        public async Task<IActionResult> Create(InventoryDTO inventoryDTO)
        {
            var response = await _repository.AddAsync(inventoryDTO);
            return Ok(response);
        }

        [HttpPut("{GUID:guid}")]
        public async Task<IActionResult> Update(Guid GUID, InventoryDTO inventoryDto)
        {
            await _repository.UpdateAsync(GUID, inventoryDto);
            return NoContent();
        }

        [HttpDelete("{GUID:guid}")]
        public async Task<IActionResult> Delete(Guid GUID)
        {
            await _repository.DeleteAsync(GUID);
            return Ok();
        }
    }
}
