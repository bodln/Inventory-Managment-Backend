using InventoryManagment.Data;
using InventoryManagment.DTOs;
using InventoryManagment.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace InventoryManagment.Repositories
{
    public interface IShipmentOrderRepository
    {
        Task<List<ShipmentOrderReturnDTO>> GetAllAsync();
        Task<ShipmentOrder> GetByGuidAsync(Guid guid);
        Task<ShipmentOrderReturnDTO> AddAsync(string token, ShipmentOrderDTO shipmentOrderDTO);
        Task UpdateAsync(Guid guid, ShipmentOrderDTO shipmentOrderDTO);
        Task DeleteAsync(Guid guid);
        Task<ShipmentOrderReturnDTO> GetByDTOGuidAsync(Guid guid);
        Task<List<ShipmentOrderReturnDTO>> GetByItemGuidAsync(Guid itemGUID);
        Task Conclude(Guid GUID, ConcludeOrderDTO conclusion, string token);
    }

    public class ShipmentOrderRepository : IShipmentOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public ShipmentOrderRepository(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<ShipmentOrderReturnDTO>> GetAllAsync()
        {
            var shipmentOrders = await _context.ShipmentOrders
                .Include(so => so.Item)
                .Include(so => so.Manager)
                .ToListAsync();

            List<ShipmentOrderReturnDTO> shipmentOrderReturnDTOs = new List<ShipmentOrderReturnDTO>();

            foreach (ShipmentOrder so in shipmentOrders)
            {
                var roles = so.Manager != null
                ? await _userManager.GetRolesAsync(so.Manager)
                : new List<string>();

                ItemDTO itemDTO = new ItemDTO
                {
                    GUID = so.Item.GUID,
                    Naziv = so.Item.Naziv,
                    Weight = so.Item.Weight,
                    Type = so.Item.Type,
                    Price = so.Item.Price,
                    MaxAmount = so.Item.MaxAmount,
                    MinAmount = so.Item.MinAmount
                };

                shipmentOrderReturnDTOs.Add(new ShipmentOrderReturnDTO
                {
                    GUID = so.GUID,
                    Item = itemDTO,
                    Quantity = so.Quantity,
                    Manager = new ReturnUserDTO
                    {
                        Email = so.Manager.Email,
                        Roles = roles.ToList(), 
                        FirstName = so.Manager.FirstName,
                        LastName = so.Manager.LastName,
                        Address = so.Manager.Address,
                        DateOfBirth = so.Manager.DateOfBirth,
                        DateOfHire = so.Manager.DateOfHire,
                        Salary = so.Manager.Salary
                    },
                    DateOfCreation = so.DateOfCreation,
                    DateOfArrival = so.DateOfArrival,
                    Price = so.Price,
                    Unloaded = so.Unloaded
                });
            }

            return shipmentOrderReturnDTOs;
        }

        public async Task<ShipmentOrderReturnDTO> GetByDTOGuidAsync(Guid guid)
        {
            var shipmentOrder = await _context.ShipmentOrders
                .Include(so => so.Item)
                .Include(so => so.Manager)
                .FirstOrDefaultAsync(so => so.GUID == guid);

            if (shipmentOrder == null)
            {
                throw new KeyNotFoundException("Shipment order not found");
            }

            var itemDTO = new ItemDTO
            {
                GUID = shipmentOrder.Item.GUID,
                Naziv = shipmentOrder.Item.Naziv,
                Weight = shipmentOrder.Item.Weight,
                Type = shipmentOrder.Item.Type,
                Price = shipmentOrder.Item.Price,
                MaxAmount = shipmentOrder.Item.MaxAmount,
                MinAmount = shipmentOrder.Item.MinAmount
            };

            var roles = shipmentOrder.Manager != null
                ? await _userManager.GetRolesAsync(shipmentOrder.Manager)
                : new List<string>();

            var managerDTO = new ReturnUserDTO
            {
                Email = shipmentOrder.Manager.Email,
                FirstName = shipmentOrder.Manager.FirstName,
                LastName = shipmentOrder.Manager.LastName,
                Address = shipmentOrder.Manager.Address,
                DateOfBirth = shipmentOrder.Manager.DateOfBirth,
                DateOfHire = shipmentOrder.Manager.DateOfHire,
                Salary = shipmentOrder.Manager.Salary,
                Roles = roles.ToList()
            };

            var shipmentOrderDTO = new ShipmentOrderReturnDTO
            {
                GUID = shipmentOrder.GUID,
                Item = itemDTO,
                Quantity = shipmentOrder.Quantity,
                Manager = managerDTO,
                DateOfCreation = shipmentOrder.DateOfCreation,
                DateOfArrival = shipmentOrder.DateOfArrival,
                Price = shipmentOrder.Price,
                Unloaded = shipmentOrder.Unloaded
            };

            return shipmentOrderDTO;
        }


        public async Task<ShipmentOrder> GetByGuidAsync(Guid guid)
        {
            return await _context.ShipmentOrders
                .Include(so => so.Item)
                .Include(so => so.Manager)
                .FirstOrDefaultAsync(so => so.GUID == guid);
        }

        public async Task<ShipmentOrderReturnDTO> AddAsync(string token, ShipmentOrderDTO shipmentOrderDTO)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            string email = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;

            var manager = await _userManager.FindByEmailAsync(email);

            if (manager == null)
            {
                throw new KeyNotFoundException("Manager not found");
            }

            var shipmentOrder = new ShipmentOrder
            {
                GUID = Guid.NewGuid(),
                DateOfCreation = DateTime.UtcNow,
                Price = shipmentOrderDTO.Price,
                Manager = manager,
                Item = await _context.Items.FirstOrDefaultAsync(it => it.GUID == shipmentOrderDTO.ItemGUID),
                Quantity = shipmentOrderDTO.Quantity
            };

            shipmentOrder.Price = (float)(shipmentOrder.Item.Price * shipmentOrder.Quantity * 0.8);

            _context.ShipmentOrders.Add(shipmentOrder);
            await _context.SaveChangesAsync();

            var itemDTO = new ItemDTO
            {
                GUID = shipmentOrder.Item.GUID,
                Naziv = shipmentOrder.Item.Naziv,
                Weight = shipmentOrder.Item.Weight,
                Type = shipmentOrder.Item.Type,
                Price = shipmentOrder.Item.Price,
                MaxAmount = shipmentOrder.Item.MaxAmount,
                MinAmount = shipmentOrder.Item.MinAmount
            };

            var roles = manager != null
                ? await _userManager.GetRolesAsync(manager)
                : new List<string>();

            var returnDTO = new ShipmentOrderReturnDTO
            {
                GUID = shipmentOrder.GUID,
                Item = itemDTO,
                Manager = new ReturnUserDTO
                {
                    Email = manager.Email,
                    FirstName = manager.FirstName,
                    LastName = manager.LastName,
                    Address = manager.Address,
                    DateOfBirth = manager.DateOfBirth,
                    DateOfHire = manager.DateOfHire,
                    Salary = manager.Salary,
                    Roles = roles.ToList()
                },
                DateOfCreation = shipmentOrder.DateOfCreation,
                DateOfArrival = shipmentOrder.DateOfArrival,
                Price = shipmentOrder.Price,
                Unloaded = shipmentOrder.Unloaded
            };

            return returnDTO;
        }

        public async Task<List<ShipmentOrderReturnDTO>> GetByItemGuidAsync(Guid itemGUID)
        {
            var shipmentOrders = await _context.ShipmentOrders
                .Include(so => so.Item)
                .Include(so => so.Manager)
                .Where(so => so.Item.GUID == itemGUID)
                .ToListAsync();

            List<ShipmentOrderReturnDTO> shipmentOrderReturnDTOs = new List<ShipmentOrderReturnDTO>();

            foreach (var shipmentOrder in shipmentOrders)
            {
                var itemDTO = new ItemDTO
                {
                    GUID = shipmentOrder.Item.GUID,
                    Naziv = shipmentOrder.Item.Naziv,
                    Weight = shipmentOrder.Item.Weight,
                    Type = shipmentOrder.Item.Type,
                    Price = shipmentOrder.Item.Price,
                    MaxAmount = shipmentOrder.Item.MaxAmount,
                    MinAmount = shipmentOrder.Item.MinAmount
                };

                var roles = shipmentOrder.Manager != null
                    ? await _userManager.GetRolesAsync(shipmentOrder.Manager)
                    : new List<string>();

                var managerDTO = new ReturnUserDTO
                {
                    Email = shipmentOrder.Manager.Email,
                    FirstName = shipmentOrder.Manager.FirstName,
                    LastName = shipmentOrder.Manager.LastName,
                    Address = shipmentOrder.Manager.Address,
                    DateOfBirth = shipmentOrder.Manager.DateOfBirth,
                    DateOfHire = shipmentOrder.Manager.DateOfHire,
                    Salary = shipmentOrder.Manager.Salary,
                    Roles = roles.ToList()
                };

                var shipmentOrderDTO = new ShipmentOrderReturnDTO
                {
                    GUID = shipmentOrder.GUID,
                    Item = itemDTO,
                    Quantity = shipmentOrder.Quantity,
                    Manager = managerDTO,
                    DateOfCreation = shipmentOrder.DateOfCreation,
                    DateOfArrival = shipmentOrder.DateOfArrival,
                    Price = shipmentOrder.Price,
                    Unloaded = shipmentOrder.Unloaded
                };

                shipmentOrderReturnDTOs.Add(shipmentOrderDTO);
            }

            return shipmentOrderReturnDTOs;
        }

        public async Task UpdateAsync(Guid guid, ShipmentOrderDTO shipmentOrderDTO)
        {
            var existingOrder = await GetByGuidAsync(guid);
            if (existingOrder == null)
            {
                throw new KeyNotFoundException("Shipment order not found");
            }

            existingOrder.Item = await _context.Items.FirstOrDefaultAsync(it => it.GUID == shipmentOrderDTO.ItemGUID);
            
            existingOrder.DateOfCreation = shipmentOrderDTO.DateOfCreation;
            existingOrder.Price = shipmentOrderDTO.Price;
            existingOrder.Unloaded = shipmentOrderDTO.Unloaded;
            existingOrder.Quantity = shipmentOrderDTO.Quantity;

            _context.ShipmentOrders.Update(existingOrder);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid guid)
        {
            var shipmentOrder = await GetByGuidAsync(guid);
            if (shipmentOrder != null)
            {
                _context.ShipmentOrders.Remove(shipmentOrder);
                await _context.SaveChangesAsync();
            }
        }

        public async Task Conclude(Guid GUID, ConcludeOrderDTO conclusion, string token)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            string email = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;

            var warehouseman = await _userManager.FindByEmailAsync(email);

            ShipmentOrder shipmentOrder = await _context.ShipmentOrders
                .Where(so => so.GUID == GUID)
                .FirstOrDefaultAsync();

            if (shipmentOrder == null)
            {
                throw new InvalidOperationException("Shipment order not found.");
            }

            if (shipmentOrder.Unloaded)
            {
                throw new InvalidOperationException("Shipment order already processed.");
            }

            Item item = await _context.Items
                .Where(i => i.GUID == conclusion.ItemGuid)
                .FirstOrDefaultAsync();

            if (item == null)
            {
                throw new InvalidOperationException("Item not found.");
            }

            var existingInventoryLocation = await _context.LocationHistories
                .Where(lh => lh.Inventory.Item.GUID == item.GUID && lh.LocationName == conclusion.Location)
                .Include(lh => lh.Inventory) 
                .FirstOrDefaultAsync();

            if (existingInventoryLocation != null)
            {
                existingInventoryLocation.Inventory.AvailableAmount += shipmentOrder.Quantity;
                existingInventoryLocation.Inventory.LastShipment = DateTime.UtcNow;

                var locationHistory = new LocationHistory
                {
                    Inventory = existingInventoryLocation.Inventory,
                    LocationName = conclusion.Location,
                    Warehouseman = warehouseman, 
                };

                _context.LocationHistories.Add(locationHistory);
            }
            else
            {
                var newInventory = new Inventory
                {
                    Item = item,
                    AvailableAmount = shipmentOrder.Quantity,
                    LastShipment = DateTime.UtcNow, 
                };

                _context.Inventories.Add(newInventory);

                var locationHistory = new LocationHistory
                {
                    Inventory = newInventory,
                    LocationName = conclusion.Location,
                    Warehouseman = shipmentOrder.Manager, 
                };

                _context.LocationHistories.Add(locationHistory);
            }

            shipmentOrder.Unloaded = true;

            await _context.SaveChangesAsync();
        }

    }
}
