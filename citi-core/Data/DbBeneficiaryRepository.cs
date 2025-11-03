using citi_core.Data;
using citi_core.Models;
using Microsoft.EntityFrameworkCore;
public class DbBeneficiaryRepository : IBeneficiaryRepository
{
    private readonly ApplicationDbContext _context;
    public DbBeneficiaryRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<Beneficiary>> GetBeneficiaryByUserIdAsync(Guid userId)
    {
        return await _context.Beneficiaries
            .Where(b => b.UserId == userId)
            .OrderBy(b => b.BeneficiaryName)
            .ToListAsync();
    }
    public async Task<Beneficiary?> GetBeneficiaryByIdAsync(Guid beneficiaryId, Guid userId)
    {
        return await _context.Beneficiaries
            .FirstOrDefaultAsync(b => b.BeneficiaryId == beneficiaryId && b.UserId == userId);
    }
    public async Task<bool> ExistsAsync(Guid userId, string accountNumber)
    {
        return await _context.Beneficiaries
            .AnyAsync(b => b.UserId == userId && b.AccountNumber == accountNumber);
    }
    public async Task AddBeneficiaryAsync(Beneficiary beneficiary)
    {
        await _context.Beneficiaries.AddAsync(beneficiary);
    }
    public async Task UpdateBeneficiaryAsync(Beneficiary beneficiary)
    {
        _context.Beneficiaries.Update(beneficiary);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteBeneficiaryAsync(Beneficiary beneficiary)
    {
        _context.Beneficiaries.Remove(beneficiary);
        await _context.SaveChangesAsync();
    }
}