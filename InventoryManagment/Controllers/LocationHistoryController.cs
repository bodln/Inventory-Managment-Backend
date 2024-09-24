using InventoryManagment.DTOs;
using InventoryManagment.Models;
using InventoryManagment.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;

namespace InventoryManagment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationHistoryController : ControllerBase
    {
        private readonly ILocationHistoryRepository _repository;

        public LocationHistoryController(ILocationHistoryRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var histories = await _repository.GetAllAsync();
            return Ok(histories);
        }

        [HttpGet("{GUID:guid}")]
        public async Task<IActionResult> GetById(Guid GUID)
        {
            var history = await _repository.GetByGuidAsync(GUID);
            if (history == null) return NotFound();
            return Ok(history);
        }

        [HttpPost]
        public async Task<IActionResult> Create(LocationHistoryDTO locationHistoryDTO)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var response = await _repository.AddAsync(token, locationHistoryDTO);
            return Ok(response);
        }

        [HttpPut("{GUID:guid}")]
        public async Task<IActionResult> Update(Guid GUID, LocationHistoryDTO locationHistoryDTO)
        {
            await _repository.UpdateAsync(GUID, locationHistoryDTO);
            return Ok("Updated.");
        }

        [HttpDelete("{GUID:guid}")]
        public async Task<IActionResult> Delete(Guid GUID)
        {
            await _repository.DeleteAsync(GUID);
            return NoContent();
        }
    }
}
