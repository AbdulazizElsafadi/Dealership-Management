using Dealership_Management.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dealership_Management.Services
{
    public interface IPurchaseService
    {
        Task<PurchaseResponseDto?> CreatePurchaseAsync(CreatePurchaseDto purchaseDto);
        Task<PurchaseResponseDto?> GetPurchaseByIdAsync(int id);
        Task<IEnumerable<PurchaseResponseDto>> GetPurchasesByUserIdAsync(int userId);
        Task<PurchaseHistoryItemDto> RequestPurchaseAsync(int userId, PurchaseRequestDto dto);
        Task<IEnumerable<PurchaseHistoryItemDto>> GetCustomerPurchaseHistoryAsync(int userId);
        Task<IEnumerable<AdminPurchaseListItemDto>> GetAllPurchasesForAdminAsync();
        Task<AdminPurchaseDetailDto?> GetPurchaseDetailForAdminAsync(int id);
        Task<(bool found, bool completed)> CompletePurchaseAsync(int purchaseId, int adminId);
    }
}