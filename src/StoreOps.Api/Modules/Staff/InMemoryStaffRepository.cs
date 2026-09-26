namespace StoreOps.Api.Modules.Staff;

using StoreOps.Api.Modules.Staff.Models;

public sealed class InMemoryStaffRepository : IStaffRepository
{
    private readonly Dictionary<Guid, User> _store;

    public InMemoryStaffRepository()
    {
        var seedUsers = new[]
        {
            new User { Name = "Priya Shah", Email = "priya.shah@storeops.example", Role = StaffRole.StoreManager },
            new User { Name = "Arun Mehta", Email = "arun.mehta@storeops.example", Role = StaffRole.DepartmentLead },
            new User { Name = "Kavya Nair", Email = "kavya.nair@storeops.example", Role = StaffRole.Associate }
        };
        _store = seedUsers.ToDictionary(u => u.Id);
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<List<User>> ListAsync() => Task.FromResult(_store.Values.ToList());
}
