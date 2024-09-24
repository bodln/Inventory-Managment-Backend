using InventoryManagment.DTOs;
using InventoryManagment.Models;
using InventoryManagment.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace InventoryManagment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly IItemRepository _repository;

        public ItemController(IItemRepository repository)
        {
            _repository = repository;
        }

        [HttpGet, Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var items = await _repository.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{GUID:guid}")]
        public async Task<IActionResult> GetByGuidAsync(Guid GUID)
        {
            var item = await _repository.GetByGuidAsync(GUID);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("Analyze")]
        public async Task<IActionResult> GetAnalyzeForItemsAsync()
        {
            var itemStock = await _repository.GetItemsAnalysisAsync();
            if (itemStock == null) return NotFound();
            return Ok(itemStock);
        }

        [HttpGet("Analyze/{GUID:guid}")]
        public async Task<IActionResult> GetAnalyzeForItemAsync(Guid GUID)
        {
            var itemStock = await _repository.GetItemAnalysisAsync(GUID);
            if (itemStock == null) return NotFound();
            return Ok(itemStock);
        }

        [HttpPost, Authorize(Roles = "Warehouseman")]
        public async Task<IActionResult> Create(ItemDTO item)
        {
            var response = await _repository.AddAsync(item);
            return Ok(response);
        }

        [HttpPut("{GUID:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid GUID, ItemDTO itemDto)
        {
            await _repository.UpdateAsync(GUID, itemDto);
            return NoContent();
        }

        [HttpDelete("{GUID:guid}")]
        public async Task<IActionResult> Delete(Guid GUID)
        {
            await _repository.DeleteAsync(GUID);
            return NoContent();
        }
    }
}
