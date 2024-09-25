using InventoryManagment.Data;
using InventoryManagment.DTOs;
using InventoryManagment.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace InventoryManagment.Repositories
{
    public interface IItemRepository
    {
        Task<List<ItemDTO>> GetAllAsync();
        Task<ItemDTO> GetByGuidAsync(Guid guid);
        Task<ItemDTO> AddAsync(ItemDTO itemDto);
        Task<AnalysisDTO> GetItemAnalysisAsync(Guid itemGuid);
        Task<List<AnalysisDTO>> GetItemsAnalysisAsync();
        Task UpdateAsync(Guid guid, ItemDTO itemDto);
        Task DeleteAsync(Guid guid);
    }

    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public ItemRepository(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<ItemDTO>> GetAllAsync()
        {
            // Convert the Items to ItemDTO for returning
            return await _context.Items
                .Select(i => new ItemDTO
                {
                    GUID = i.GUID,
                    Naziv = i.Naziv,
                    Weight = i.Weight,
                    Type = i.Type,
                    Price = i.Price,
                    MaxAmount = i.MaxAmount,
                    MinAmount = i.MinAmount
                })
                .ToListAsync();
        }

        public async Task<ItemDTO> GetByGuidAsync(Guid guid)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.GUID == guid);

            return item == null ? null : new ItemDTO
            {
                GUID = item.GUID,
                Naziv = item.Naziv,
                Weight = item.Weight,
                Type = item.Type,
                Price = item.Price,
                MaxAmount = item.MaxAmount,
                MinAmount = item.MinAmount
            };
        }

        public async Task<ItemDTO> AddAsync(ItemDTO itemDto)
        {
            var item = new Item
            {
                Naziv = itemDto.Naziv,
                Weight = itemDto.Weight,
                Type = itemDto.Type,
                Price = itemDto.Price,
                MaxAmount = itemDto.MaxAmount,
                MinAmount = itemDto.MinAmount
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return new ItemDTO
            {
                GUID = item.GUID,
                Naziv = item.Naziv,
                Weight = item.Weight,
                Type = item.Type,
                Price = item.Price,
                MaxAmount = item.MaxAmount,
                MinAmount = item.MinAmount
            };
        }

        public async Task UpdateAsync(Guid guid, ItemDTO itemDto)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.GUID == guid);
            if (item == null)
            {
                throw new KeyNotFoundException("Item not found");
            }

            item.Naziv = itemDto.Naziv;
            item.Weight = itemDto.Weight;
            item.Type = itemDto.Type;
            item.Price = itemDto.Price;
            item.MaxAmount = itemDto.MaxAmount;
            item.MinAmount = itemDto.MinAmount;

            _context.Items.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task<AnalysisDTO> GetItemAnalysisAsync(Guid itemGuid)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.GUID == itemGuid);

            if (item == null)
            {
                throw new KeyNotFoundException("Item not found");
            }

            var billsOfSale = await _context.BillsOfSale
                .Where(b => b.Item.GUID == itemGuid)
                .ToListAsync();

            var unitsSold = billsOfSale.Sum(b => b.Quantity);
            var totalProfit = billsOfSale.Sum(b => b.Quantity * b.Item.Price);

            var locationHistories = await _context.LocationHistories
                .Where(lh => lh.Inventory.Item.GUID == itemGuid)
                .Include(lh => lh.Warehouseman)  
                .Include(lh => lh.Inventory)
                .ThenInclude(inv => inv.Item)
                .ToListAsync();

            List<LocationHistoryReturnDTO> locationHistoryDTOs = new List<LocationHistoryReturnDTO>();

            foreach (LocationHistory lh in locationHistories) {
                var roles = lh.Warehouseman != null
                    ? await _userManager.GetRolesAsync(lh.Warehouseman)
                    : new List<string>();

                LocationHistoryReturnDTO locationHistoryDTO = new LocationHistoryReturnDTO
                {
                    DateOfStoring = lh.DateOfStoring,
                    Warehouseman = new ReturnUserDTO
                    {
                        Email = lh.Warehouseman.Email,
                        FirstName = lh.Warehouseman.FirstName,
                        LastName = lh.Warehouseman.LastName,
                        Address = lh.Warehouseman.Address,
                        DateOfBirth = lh.Warehouseman.DateOfBirth,
                        DateOfHire = lh.Warehouseman.DateOfHire,
                        Salary = lh.Warehouseman.Salary,
                        Roles = roles.ToList()
                    },
                    Inventory = new InventoryReturnDTO
                    {
                        GUID = lh.Inventory.GUID,
                        AvailableAmount = lh.Inventory.AvailableAmount,
                        LastShipment = lh.Inventory.LastShipment
                    },
                    LocationName = lh.LocationName,
                    Quantity = lh.Quantity
                };

                locationHistoryDTOs.Add(locationHistoryDTO);
            }

            var analysisDTO = new AnalysisDTO
            {
                Item = new ItemDTO
                {
                    GUID = item.GUID,
                    Naziv = item.Naziv,
                    Weight = item.Weight,
                    Type = item.Type,
                    Price = item.Price,
                    MaxAmount = item.MaxAmount,
                    MinAmount = item.MinAmount
                },
                UnitsSold = unitsSold,
                TotalProfit = totalProfit,
                Locations = locationHistoryDTOs
            };

            return analysisDTO;
        }

        public async Task<List<AnalysisDTO>> GetItemsAnalysisAsync()
        {
            var items = await _context.Items
                .ToListAsync();

            List <AnalysisDTO> analysisDTOs = new List<AnalysisDTO>();

            foreach (Item item in items)
            {
                var billsOfSale = await _context.BillsOfSale
                .Where(b => b.Item.GUID == item.GUID)
                .ToListAsync();

                var unitsSold = billsOfSale.Sum(b => b.Quantity);
                var totalProfit = billsOfSale.Sum(b => b.Quantity * b.Item.Price);

                var locationHistories = await _context.LocationHistories
                    .Where(lh => lh.Inventory.Item.GUID == item.GUID)
                    .Include(lh => lh.Warehouseman)
                    .Include(lh => lh.Inventory)
                    .ToListAsync();

                List<LocationHistoryReturnDTO> locationHistoryDTOs = new List<LocationHistoryReturnDTO>();

                foreach (LocationHistory lh in locationHistories)
                {
                    var roles = lh.Warehouseman != null
                        ? await _userManager.GetRolesAsync(lh.Warehouseman)
                        : new List<string>();

                    LocationHistoryReturnDTO locationHistoryDTO = new LocationHistoryReturnDTO
                    {
                        DateOfStoring = lh.DateOfStoring,
                        Warehouseman = new ReturnUserDTO
                        {
                            Email = lh.Warehouseman.Email,
                            FirstName = lh.Warehouseman.FirstName,
                            LastName = lh.Warehouseman.LastName,
                            Address = lh.Warehouseman.Address,
                            DateOfBirth = lh.Warehouseman.DateOfBirth,
                            DateOfHire = lh.Warehouseman.DateOfHire,
                            Salary = lh.Warehouseman.Salary,
                            Roles = roles.ToList()
                        },
                        Inventory = new InventoryReturnDTO
                        {
                            GUID = lh.Inventory.GUID,
                            AvailableAmount = lh.Inventory.AvailableAmount,
                            LastShipment = lh.Inventory.LastShipment
                        },
                        LocationName = lh.LocationName,
                        Quantity = lh.Quantity
                    };

                    locationHistoryDTOs.Add(locationHistoryDTO);
                }

                var analysisDTO = new AnalysisDTO
                {
                    Item = new ItemDTO
                    {
                        GUID = item.GUID,
                        Naziv = item.Naziv,
                        Weight = item.Weight,
                        Type = item.Type,
                        Price = item.Price,
                        MaxAmount = item.MaxAmount,
                        MinAmount = item.MinAmount
                    },
                    UnitsSold = unitsSold,
                    TotalProfit = totalProfit,
                    Locations = locationHistoryDTOs
                };

                analysisDTOs.Add(analysisDTO);
            }

            return analysisDTOs;
        }

        public async Task DeleteAsync(Guid guid)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.GUID == guid);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}
