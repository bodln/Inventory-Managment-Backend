using InventoryManagment.Data;
using InventoryManagment.DTOs;
using InventoryManagment.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagment.Repositories
{
    public interface IInventoryRepository
    {
        Task<List<InventoryReturnDTO>> GetAllAsync();
        Task<InventoryReturnDTO> GetByGuidAsync(Guid guid);
        Task<InventoryReturnDTO> AddAsync(InventoryDTO inventoryDto);
        Task UpdateAsync(Guid guid, InventoryDTO inventoryDto);
        Task DeleteAsync(Guid guid);
        Task<StockDTO> GetStockForItemAsync(Guid itemGUID);
        Task<List<StockDTO>> GetStockForItemsAsync();
    }

    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryReturnDTO>> GetAllAsync()
        {
            return await _context.Inventories
                .Include(i => i.Item)
                .Select(i => new InventoryReturnDTO
                {
                    GUID = i.GUID,
                    Item = new ItemDTO
                    {
                        GUID = i.Item.GUID,
                        Naziv = i.Item.Naziv,
                        Weight = i.Item.Weight,
                        Type = i.Item.Type,
                        Price = i.Item.Price,
                        MaxAmount = i.Item.MaxAmount,
                        MinAmount = i.Item.MinAmount
                    },
                    AvailableAmount = i.AvailableAmount,
                    LastShipment = i.LastShipment,
                    CurrentLocationName = _context.LocationHistories            // <--------------------
                        .Where(lh => lh.Inventory.GUID == i.GUID)               // <--------------------
                        .OrderByDescending(lh => lh.DateOfStoring)              // <--------------------
                        .Select(lh => lh.LocationName)                          // <--------------------
                        .FirstOrDefault()                                       // <--------------------
                })
                .ToListAsync();
        }

        public async Task<InventoryReturnDTO> GetByGuidAsync(Guid guid)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Item)
                .FirstOrDefaultAsync(i => i.GUID == guid);

            if (inventory == null)
            {
                return null;
            }

            return new InventoryReturnDTO
            {
                GUID = inventory.GUID,
                Item = new ItemDTO
                {
                    GUID = inventory.Item.GUID,
                    Naziv = inventory.Item.Naziv,
                    Weight = inventory.Item.Weight,
                    Type = inventory.Item.Type,
                    Price = inventory.Item.Price,
                    MaxAmount = inventory.Item.MaxAmount,
                    MinAmount = inventory.Item.MinAmount
                },
                AvailableAmount = inventory.AvailableAmount,
                LastShipment = inventory.LastShipment,
                CurrentLocationName = await _context.LocationHistories           // <--------------------
                    .Where(lh => lh.Inventory.GUID == inventory.GUID)            // <--------------------
                    .OrderByDescending(lh => lh.DateOfStoring)                   // <--------------------
                    .Select(lh => lh.LocationName)                               // <--------------------
                    .FirstOrDefaultAsync()                                       // <--------------------
            };
        }

        public async Task<InventoryReturnDTO> AddAsync(InventoryDTO inventoryDto)
        {
            var inventory = new Inventory
            {
                Item = await _context.Items.FirstOrDefaultAsync(it => it.GUID == inventoryDto.ItemGUID),
                AvailableAmount = inventoryDto.AvailableAmount,
                LastShipment = inventoryDto.LastShipment
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            return new InventoryReturnDTO
            {
                GUID = inventory.GUID,
                Item = new ItemDTO
                {
                    GUID = inventory.Item.GUID,
                    Naziv = inventory.Item.Naziv,
                    Weight = inventory.Item.Weight,
                    Type = inventory.Item.Type,
                    Price = inventory.Item.Price,
                    MaxAmount = inventory.Item.MaxAmount,
                    MinAmount = inventory.Item.MinAmount
                },
                AvailableAmount = inventory.AvailableAmount,
                LastShipment = inventory.LastShipment,
                CurrentLocationName = await _context.LocationHistories           // <--------------------
                    .Where(lh => lh.Inventory.GUID == inventory.GUID)            // <--------------------
                    .OrderByDescending(lh => lh.DateOfStoring)                   // <--------------------
                    .Select(lh => lh.LocationName)                               // <--------------------
                    .FirstOrDefaultAsync()                                       // <--------------------
            };
        }

        public async Task UpdateAsync(Guid guid, InventoryDTO inventoryDto)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(it => it.GUID == guid);
            if (inventory == null)
            {
                throw new KeyNotFoundException("Inventory not found");
            }

            inventory.Item = await _context.Items.FirstOrDefaultAsync(it => it.GUID == inventoryDto.ItemGUID);
            inventory.AvailableAmount = inventoryDto.AvailableAmount;
            inventory.LastShipment = inventoryDto.LastShipment;

            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task<StockDTO> GetStockForItemAsync(Guid itemGUID)
        {
            // Fetch the item from the database
            var item = await _context.Items
                .FirstOrDefaultAsync(it => it.GUID == itemGUID);

            if (item == null)
            {
                throw new KeyNotFoundException("Item not found");
            }

            // Calculate the total available amount from inventories
            var totalAvailableAmount = await _context.Inventories
                .Where(inv => inv.Item.GUID == itemGUID)
                .SumAsync(inv => inv.AvailableAmount);

            // Check if the total available amount is over or under capacity
            bool overCapacity = totalAvailableAmount > item.MaxAmount;
            bool underCapacity = totalAvailableAmount < item.MinAmount;
            bool moreInStorage = await _context.ShipmentOrders
                .AnyAsync(so => so.Item.GUID == itemGUID && !so.Unloaded);

            // Populate the StockDTO
            var stockDTO = new StockDTO
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
                AvilableAmount = totalAvailableAmount,
                OverCapacity = overCapacity,
                UnderCapacity = underCapacity,
                MoreInStorage = moreInStorage
            };

            return stockDTO;
        }

        public async Task<List<StockDTO>> GetStockForItemsAsync()
        {
            var items = await _context.Items.ToListAsync();
            List<StockDTO> stocks = new List<StockDTO>();

            foreach (Item item in items)
            {
                var totalAvailableAmount = await _context.Inventories
                    .Where(inv => inv.Item.GUID == item.GUID)
                    .SumAsync(inv => inv.AvailableAmount);

                bool overCapacity = totalAvailableAmount > item.MaxAmount;
                bool underCapacity = totalAvailableAmount < item.MinAmount;
                bool moreInStorage = await _context.ShipmentOrders
                    .AnyAsync(so => so.Item.GUID == item.GUID && !so.Unloaded);

                var stockDTO = new StockDTO
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
                    AvilableAmount = totalAvailableAmount,
                    OverCapacity = overCapacity,
                    UnderCapacity = underCapacity,
                    MoreInStorage = moreInStorage
                };

                stocks.Add(stockDTO);
            }

            return stocks;
        }

        public async Task DeleteAsync(Guid guid)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(it => it.GUID == guid);
            if (inventory != null)
            {
                var histories = _context.LocationHistories.Where(lh => lh.Inventory.GUID == guid);
                if (histories.Count() != 0)
                {
                    _context.LocationHistories.RemoveRange(histories);
                }
                _context.Inventories.Remove(inventory);
                await _context.SaveChangesAsync();
            }
        }
    }
}
