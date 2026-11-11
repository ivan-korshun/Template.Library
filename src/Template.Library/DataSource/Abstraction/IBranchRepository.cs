// Copyright © Korshun Ivan. All rights reserved.

using Template.Library.Models;

namespace Template.Library.DataSource.Abstraction;

public interface IBranchRepository
{
    Task<ICollection<Branch>> GetAllAsync(CancellationToken token);

    Task<Branch?> GetAsync(int id, CancellationToken token);

    Task<Branch> CreateAsync(BranchCreateDto branchDto, CancellationToken token);

    Task<Branch?> UpdateAsync(int id, BranchUpdateDto branchDto, CancellationToken token);

    Task<bool> DeleteAsync(int id, CancellationToken token);
}
