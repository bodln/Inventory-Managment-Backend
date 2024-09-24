using InventoryManagment.Data;
using InventoryManagment.DTOs;
using InventoryManagment.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace InventoryManagment.Repositories
{
    public interface IBillOfSaleRepository
    {
        Task<List<BillOfSaleReturnDTO>> GetAllAsync();
        Task<BillOfSaleReturnDTO> GetByGUIDAsync(Guid GUID);
        Task<BillOfSaleReturnDTO> AddAsync(string token, BillOfSaleDTO billOfSale);
        Task UpdateAsync(Guid GUID, BillOfSaleDTO billOfSaleDTO);
        Task DeleteAsync(Guid GUID);
        Task<List<BillOfSaleReturnDTO>> GetByItemGuid(Guid GUID);
    }

    public class BillOfSaleRepository : IBillOfSaleRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public BillOfSaleRepository(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<BillOfSaleReturnDTO>> GetAllAsync()
        {
            var billsOfSale = await _context.BillsOfSale
                .Include(b => b.Item)
                .Include(b => b.Warehouseman)
                .ToListAsync();

            var billOfSaleReturnDTOs = new List<BillOfSaleReturnDTO>();

            foreach (var billOfSale in billsOfSale)
            {
                ItemDTO itemDTO = new ItemDTO
                {
                    GUID = billOfSale.Item.GUID,
                    Naziv = billOfSale.Item.Naziv,
                    Weight = billOfSale.Item.Weight,
                    Type = billOfSale.Item.Type,
                    Price = billOfSale.Item.Price,
                    MaxAmount = billOfSale.Item.MaxAmount,
                    MinAmount = billOfSale.Item.MinAmount
                };
                var billOfSaleReturnDTO = new BillOfSaleReturnDTO
                {
                    GUID = billOfSale.GUID,
                    DateOfSale = billOfSale.DateOfSale,
                    NameOfBuyer = billOfSale.NameOfBuyer,
                    ContactInfo = billOfSale.ContactInfo,
                    DeliveryAddress = billOfSale.DeliveryAddress,
                    Item = itemDTO,
                    Quantity = billOfSale.Quantity
                };

                if (billOfSale.Warehouseman != null)
                {
                    var roles = await _userManager.GetRolesAsync(billOfSale.Warehouseman);
                    billOfSaleReturnDTO.Warehouseman = new ReturnUserDTO
                    {
                        Email = billOfSale.Warehouseman.Email,
                        Roles = roles.ToList(),
                        FirstName = billOfSale.Warehouseman.FirstName,
                        LastName = billOfSale.Warehouseman.LastName,
                        Address = billOfSale.Warehouseman.Address,
                        DateOfBirth = billOfSale.Warehouseman.DateOfBirth,
                        DateOfHire = billOfSale.Warehouseman.DateOfHire,
                        Salary = billOfSale.Warehouseman.Salary
                    };
                }

                billOfSaleReturnDTOs.Add(billOfSaleReturnDTO);
            }

            return billOfSaleReturnDTOs;
        }

        public async Task<BillOfSaleReturnDTO> GetByGUIDAsync(Guid GUID)
        {
            var billOfSale = await _context.BillsOfSale
                .Include(b => b.Item)
                .Include(b => b.Warehouseman)
                .FirstOrDefaultAsync(b => b.GUID == GUID);

            if (billOfSale == null)
            {
                return null;
            }

            var roles = billOfSale.Warehouseman != null
                ? await _userManager.GetRolesAsync(billOfSale.Warehouseman)
                : new List<string>();

            return new BillOfSaleReturnDTO
            {
                GUID = billOfSale.GUID,
                DateOfSale = billOfSale.DateOfSale,
                NameOfBuyer = billOfSale.NameOfBuyer,
                ContactInfo = billOfSale.ContactInfo,
                DeliveryAddress = billOfSale.DeliveryAddress,
                Quantity = billOfSale.Quantity,
                Item = new ItemDTO
                {
                    GUID = billOfSale.Item.GUID,
                    Naziv = billOfSale.Item.Naziv,
                    Weight = billOfSale.Item.Weight,
                    Type = billOfSale.Item.Type,
                    Price = billOfSale.Item.Price,
                    MaxAmount = billOfSale.Item.MaxAmount,
                    MinAmount = billOfSale.Item.MinAmount
                },
                Warehouseman = billOfSale.Warehouseman != null ? new ReturnUserDTO
                {
                    Email = billOfSale.Warehouseman.Email,
                    Roles = roles.ToList(),
                    FirstName = billOfSale.Warehouseman.FirstName,
                    LastName = billOfSale.Warehouseman.LastName,
                    Address = billOfSale.Warehouseman.Address,
                    DateOfBirth = billOfSale.Warehouseman.DateOfBirth,
                    DateOfHire = billOfSale.Warehouseman.DateOfHire,
                    Salary = billOfSale.Warehouseman.Salary
                } : null
            };
        }

        public async Task<BillOfSaleReturnDTO> AddAsync(string token, BillOfSaleDTO billOfSaleDTO)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            string email = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;

            var warehouseman = await _userManager.FindByEmailAsync(email);

            if (warehouseman == null)
            {
                throw new FileNotFoundException("User not found");
            }

            var item = await _context.Items
                              .FirstOrDefaultAsync(item => billOfSaleDTO.ItemGUID == item.GUID);

            var billOfSale = new BillOfSale
            {
                GUID = Guid.NewGuid(),
                Item = item,
                Quantity = billOfSaleDTO.Quantity,
                Warehouseman = warehouseman,
                DateOfSale = billOfSaleDTO.DateOfSale,
                NameOfBuyer = billOfSaleDTO.NameOfBuyer,
                ContactInfo = billOfSaleDTO.ContactInfo,
                DeliveryAddress = billOfSaleDTO.DeliveryAddress
            };

            _context.BillsOfSale.Add(billOfSale);
            await _context.SaveChangesAsync();

            var roles = warehouseman != null ? await _userManager.GetRolesAsync(warehouseman) : new List<string>();

            var billOfSaleReturnDTO = new BillOfSaleReturnDTO
            {
                GUID = billOfSale.GUID,
                DateOfSale = billOfSale.DateOfSale,
                NameOfBuyer = billOfSale.NameOfBuyer,
                ContactInfo = billOfSale.ContactInfo,
                DeliveryAddress = billOfSale.DeliveryAddress,
                Item = new ItemDTO
                {
                    GUID = billOfSale.Item.GUID,
                    Naziv = billOfSale.Item.Naziv,
                    Weight = billOfSale.Item.Weight,
                    Type = billOfSale.Item.Type,
                    Price = billOfSale.Item.Price,
                    MaxAmount = billOfSale.Item.MaxAmount,
                    MinAmount = billOfSale.Item.MinAmount
                },
                Warehouseman = warehouseman != null ? new ReturnUserDTO
                {
                    Email = warehouseman.Email,
                    Roles = roles.ToList(),
                    FirstName = warehouseman.FirstName,
                    LastName = warehouseman.LastName,
                    Address = warehouseman.Address,
                    DateOfBirth = warehouseman.DateOfBirth,
                    DateOfHire = warehouseman.DateOfHire,
                    Salary = warehouseman.Salary
                } : null
            };

            return billOfSaleReturnDTO;
        }

        public async Task UpdateAsync(Guid GUID, BillOfSaleDTO billOfSaleDTO)
        {
            var existingBillOfSale = await _context.BillsOfSale
                .Include(b => b.Item) 
                .FirstOrDefaultAsync(b => b.GUID == GUID);

            if (existingBillOfSale == null)
            {
                throw new KeyNotFoundException($"Bill of Sale with GUID '{GUID}' not found.");
            }

            existingBillOfSale.DateOfSale = billOfSaleDTO.DateOfSale;
            existingBillOfSale.NameOfBuyer = billOfSaleDTO.NameOfBuyer;
            existingBillOfSale.ContactInfo = billOfSaleDTO.ContactInfo;
            existingBillOfSale.DeliveryAddress = billOfSaleDTO.DeliveryAddress;

            if (billOfSaleDTO.ItemGUID != null)
            {
                existingBillOfSale.Item = await _context.Items
                    .FirstOrDefaultAsync(item => billOfSaleDTO.ItemGUID == item.GUID);
            }

            _context.BillsOfSale.Update(existingBillOfSale);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid GUID)
        {
            var billOfSale = await _context.BillsOfSale.FirstOrDefaultAsync(b => b.GUID == GUID);
            if (billOfSale != null)
            {
                _context.BillsOfSale.Remove(billOfSale);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<BillOfSaleReturnDTO>> GetByItemGuid(Guid itemGUID)
        {
            var billsOfSale = await _context.BillsOfSale
                .Include(bos => bos.Item)
                .Include(bos => bos.Warehouseman)
                .Where(bos => bos.Item.GUID == itemGUID)
                .ToListAsync();

            List<BillOfSaleReturnDTO> billOfSaleReturnDTOs = new List<BillOfSaleReturnDTO>();

            foreach (var billOfSale in billsOfSale)
            {
                var itemDTO = new ItemDTO
                {
                    GUID = billOfSale.Item.GUID,
                    Naziv = billOfSale.Item.Naziv,
                    Weight = billOfSale.Item.Weight,
                    Type = billOfSale.Item.Type,
                    Price = billOfSale.Item.Price,
                    MaxAmount = billOfSale.Item.MaxAmount,
                    MinAmount = billOfSale.Item.MinAmount
                };

                var roles = billOfSale.Warehouseman != null
                    ? await _userManager.GetRolesAsync(billOfSale.Warehouseman)
                    : new List<string>();

                var warehousemanDTO = new ReturnUserDTO
                {
                    Email = billOfSale.Warehouseman.Email,
                    FirstName = billOfSale.Warehouseman.FirstName,
                    LastName = billOfSale.Warehouseman.LastName,
                    Address = billOfSale.Warehouseman.Address,
                    DateOfBirth = billOfSale.Warehouseman.DateOfBirth,
                    DateOfHire = billOfSale.Warehouseman.DateOfHire,
                    Salary = billOfSale.Warehouseman.Salary,
                    Roles = roles.ToList()
                };

                var billOfSaleReturnDTO = new BillOfSaleReturnDTO
                {
                    GUID = billOfSale.GUID,
                    Item = itemDTO,
                    Quantity = billOfSale.Quantity,
                    Warehouseman = warehousemanDTO,
                    DateOfSale = billOfSale.DateOfSale,
                    NameOfBuyer = billOfSale.NameOfBuyer,
                    ContactInfo = billOfSale.ContactInfo,
                    DeliveryAddress = billOfSale.DeliveryAddress
                };

                billOfSaleReturnDTOs.Add(billOfSaleReturnDTO);
            }

            return billOfSaleReturnDTOs;
        }

    }
}
