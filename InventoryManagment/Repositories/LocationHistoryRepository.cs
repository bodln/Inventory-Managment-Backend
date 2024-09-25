using InventoryManagment.Data;
using InventoryManagment.DTOs;
using InventoryManagment.Migrations;
using InventoryManagment.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace InventoryManagment.Repositories
{
    public interface ILocationHistoryRepository
    {
        Task<List<LocationHistoryReturnDTO>> GetAllAsync();
        Task<LocationHistoryReturnDTO> GetByGuidAsync(Guid guid);
        Task<LocationHistoryReturnDTO> AddAsync(string token, LocationHistoryDTO locationHistoryDto);
        Task UpdateAsync(Guid guid, LocationHistoryDTO locationHistoryDto);
        Task DeleteAsync(Guid guid);
    }

    public class LocationHistoryRepository : ILocationHistoryRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public LocationHistoryRepository(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<LocationHistoryReturnDTO>> GetAllAsync()
        {
            List<LocationHistory> locationHistories = await _context.LocationHistories
                .Include(lh => lh.Inventory)
                .ThenInclude(inv => inv.Item)
                .Include(lh => lh.Warehouseman)
                .ToListAsync();

            List<LocationHistoryReturnDTO> locationHistoriesDTOs = new List<LocationHistoryReturnDTO>();

            foreach (LocationHistory lh in locationHistories)
            {
                var roles = lh.Warehouseman != null
                ? await _userManager.GetRolesAsync(lh.Warehouseman)
                : new List<string>();

                locationHistoriesDTOs.Add(
                    new LocationHistoryReturnDTO
                    {
                        DateOfStoring = lh.DateOfStoring,
                        Warehouseman = new ReturnUserDTO
                        {
                            Email = lh.Warehouseman.Email,
                            Roles = roles.ToList(),
                            FirstName = lh.Warehouseman.FirstName,
                            LastName = lh.Warehouseman.LastName,
                            Address = lh.Warehouseman.Address,
                            DateOfBirth = lh.Warehouseman.DateOfBirth,
                            DateOfHire = lh.Warehouseman.DateOfHire,
                            Salary = lh.Warehouseman.Salary
                        },
                        Inventory = new InventoryReturnDTO
                        {
                            GUID = lh.Inventory.GUID,
                            Item = new ItemDTO
                            {
                                GUID = lh.Inventory.Item.GUID,
                                Naziv = lh.Inventory.Item.Naziv,
                                Weight = lh.Inventory.Item.Weight,
                                Type = lh.Inventory.Item.Type,
                                Price = lh.Inventory.Item.Price,
                                MaxAmount = lh.Inventory.Item.MaxAmount,
                                MinAmount = lh.Inventory.Item.MinAmount
                            },
                            AvailableAmount = lh.Inventory.AvailableAmount,
                            LastShipment = lh.Inventory.LastShipment
                        },
                        LocationName = lh.LocationName,
                        Quantity = lh.Quantity
                    }
                );
            }

            return locationHistoriesDTOs;
        }


        public async Task<LocationHistoryReturnDTO> GetByGuidAsync(Guid guid)
        {
            var locationHistory = await _context.LocationHistories
                .Include(lh => lh.Inventory)
                .ThenInclude(inv => inv.Item)
                .Include(lh => lh.Warehouseman)
                .FirstOrDefaultAsync(lh => lh.Inventory.GUID == guid);

            if (locationHistory == null)
            {
                return null;
            }

            var roles = locationHistory.Warehouseman != null
                ? await _userManager.GetRolesAsync(locationHistory.Warehouseman)
                : new List<string>();

            return new LocationHistoryReturnDTO
            {
                DateOfStoring = locationHistory.DateOfStoring,
                Warehouseman = new ReturnUserDTO
                {
                    Email = locationHistory.Warehouseman.Email,
                    Roles = roles.ToList(),
                    FirstName = locationHistory.Warehouseman.FirstName,
                    LastName = locationHistory.Warehouseman.LastName,
                    Address = locationHistory.Warehouseman.Address,
                    DateOfBirth = locationHistory.Warehouseman.DateOfBirth,
                    DateOfHire = locationHistory.Warehouseman.DateOfHire,
                    Salary = locationHistory.Warehouseman.Salary
                },
                Inventory = new InventoryReturnDTO
                {
                    GUID = locationHistory.Inventory.GUID,
                    Item = new ItemDTO
                    {
                        GUID = locationHistory.Inventory.Item.GUID,
                        Naziv = locationHistory.Inventory.Item.Naziv,
                        Weight = locationHistory.Inventory.Item.Weight,
                        Type = locationHistory.Inventory.Item.Type,
                        Price = locationHistory.Inventory.Item.Price,
                        MaxAmount = locationHistory.Inventory.Item.MaxAmount,
                        MinAmount = locationHistory.Inventory.Item.MinAmount
                    },
                    AvailableAmount = locationHistory.Inventory.AvailableAmount,
                    LastShipment = locationHistory.Inventory.LastShipment
                },
                LocationName = locationHistory.LocationName
            };
        }

        public async Task<LocationHistoryReturnDTO> AddAsync(string token, LocationHistoryDTO locationHistoryDto)
        {
            if (locationHistoryDto.Quantity > 0)
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = handler.ReadToken(token) as JwtSecurityToken;

                string email = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;

                var warehouseman = await _userManager.FindByEmailAsync(email);
                if (warehouseman == null)
                {
                    throw new KeyNotFoundException("Warehouseman not found");
                }

                var inventory = await _context.Inventories
                    .Include(i => i.Item)
                    .FirstOrDefaultAsync(i => i.GUID == locationHistoryDto.InventoryGUID);

                if (inventory == null)
                {
                    throw new KeyNotFoundException("Inventory not found.");
                }

                if (inventory.AvailableAmount < locationHistoryDto.Quantity)
                {
                    locationHistoryDto.Quantity = inventory.AvailableAmount;
                }

                LocationHistory existing = null;

                try
                {
                    existing = await _context.LocationHistories
                                .Include(lh => lh.Inventory)
                                .ThenInclude(i => i.Item)
                                .Where(lh => lh.Inventory.Item.GUID == inventory.Item.GUID && 
                                lh.LocationName == locationHistoryDto.LocationName && 
                                lh.Inventory.GUID != inventory.GUID)
                                .OrderBy(lh => lh.DateOfStoring)
                                .LastOrDefaultAsync();
                }
                catch (Exception ex)
                {

                    throw new Exception(ex.Message);
                }

                if (existing != null)
                {
                    existing.Inventory.AvailableAmount += locationHistoryDto.Quantity;
                    var locationHistoryExists = new LocationHistory
                    {
                        DateOfStoring = DateTime.UtcNow,
                        Warehouseman = warehouseman,
                        Inventory = existing.Inventory,
                        LocationName = locationHistoryDto.LocationName,
                        Quantity = -locationHistoryDto.Quantity
                    };

                    _context.LocationHistories.Add(locationHistoryExists);
                    await _context.SaveChangesAsync();
                }

                inventory.AvailableAmount -= locationHistoryDto.Quantity;

                Inventory newInv = null;
                if (existing == null)
                {
                    newInv = new Inventory
                    {
                        Item = inventory.Item,
                        AvailableAmount = locationHistoryDto.Quantity,
                        LastShipment = DateTime.UtcNow
                    };

                    await _context.Inventories.AddAsync(newInv);
                }

                var locationHistory = new LocationHistory
                {
                    DateOfStoring = DateTime.UtcNow,
                    Warehouseman = warehouseman,
                    Inventory = existing==null ? newInv : existing.Inventory,
                    LocationName = locationHistoryDto.LocationName,
                    Quantity = locationHistoryDto.Quantity
                };

                _context.LocationHistories.Add(locationHistory);
                await _context.SaveChangesAsync();

                var roles = warehouseman != null
                    ? await _userManager.GetRolesAsync(warehouseman)
                    : new List<string>();

                ReturnUserDTO returnUserDTO = new ReturnUserDTO
                {
                    Email = warehouseman.Email,
                    Roles = roles.ToList(),
                    FirstName = warehouseman.FirstName,
                    LastName = warehouseman.LastName,
                    Address = warehouseman.Address,
                    DateOfBirth = warehouseman.DateOfBirth,
                    DateOfHire = warehouseman.DateOfHire,
                    Salary = warehouseman.Salary
                };

                InventoryReturnDTO inventoryReturnDTO = new InventoryReturnDTO
                {
                    GUID = locationHistory.Inventory.GUID,
                    Item = new ItemDTO
                    {
                        GUID = locationHistory.Inventory.Item.GUID,
                        Naziv = locationHistory.Inventory.Item.Naziv,
                        Weight = locationHistory.Inventory.Item.Weight,
                        Type = locationHistory.Inventory.Item.Type,
                        Price = locationHistory.Inventory.Item.Price,
                        MaxAmount = locationHistory.Inventory.Item.MaxAmount,
                        MinAmount = locationHistory.Inventory.Item.MinAmount
                    },
                    AvailableAmount = locationHistory.Inventory.AvailableAmount,
                    LastShipment = locationHistory.Inventory.LastShipment
                };

                return new LocationHistoryReturnDTO
                {
                    DateOfStoring = locationHistory.DateOfStoring,
                    Warehouseman = returnUserDTO,
                    Inventory = inventoryReturnDTO,
                    LocationName = locationHistory.LocationName
                }; 
            }

            throw new FormatException("Invalid transfer amount.");
        }


        public async Task UpdateAsync(Guid guid, LocationHistoryDTO locationHistoryDto)
        {
            var locationHistory = await _context.LocationHistories
                .Include(lh => lh.Warehouseman)
                .FirstOrDefaultAsync(lh => lh.Inventory.GUID == guid);

            if (locationHistory == null)
            {
                throw new KeyNotFoundException("Location history not found");
            }

            locationHistory.Inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.GUID == locationHistoryDto.InventoryGUID);

            _context.LocationHistories.Update(locationHistory);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid guid)
        {
            var locationHistory = await _context.LocationHistories
                .FirstOrDefaultAsync(lh => lh.Inventory.GUID == guid);

            if (locationHistory != null)
            {
                _context.LocationHistories.Remove(locationHistory);
                await _context.SaveChangesAsync();
            }
        }
    }
}
