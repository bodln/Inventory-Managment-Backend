using InventoryManagment.DTOs;
using InventoryManagment.Models;
using InventoryManagment.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentOrderController : ControllerBase
    {
        private readonly IShipmentOrderRepository _repository;

        public ShipmentOrderController(IShipmentOrderRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _repository.GetAllAsync();
            return Ok(orders);
        }

        [HttpGet("{GUID:Guid}")]
        public async Task<IActionResult> GetById(Guid GUID)
        {
            var order = await _repository.GetByDTOGuidAsync(GUID);
            if (order == null) return NotFound();
            return Ok(order);
        }
        
        [HttpGet("Item/{GUID:Guid}")]
        public async Task<IActionResult> GetByItemGuidAsync(Guid GUID)
        {
            var orders = await _repository.GetByItemGuidAsync(GUID);
            return Ok(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ShipmentOrderDTO shipmentOrderDTO)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var response = await _repository.AddAsync(token, shipmentOrderDTO);
            return Ok(response);
        }

        [HttpPut("{GUID:Guid}")]
        public async Task<IActionResult> Update(Guid GUID, ShipmentOrderDTO shipmentOrderDTO)
        {
            await _repository.UpdateAsync(GUID, shipmentOrderDTO);
            return Ok();
        }

        [HttpPut("Conclude/{GUID:Guid}")]
        public async Task<IActionResult> Conclude(Guid GUID, ConcludeOrderDTO conclusion)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            await _repository.Conclude(GUID, conclusion, token);
            return Ok();
        }

        [HttpDelete("{GUID:Guid}")]
        public async Task<IActionResult> Delete(Guid GUID)
        {
            await _repository.DeleteAsync(GUID);
            return Ok();
        }
    }
}
