namespace StoreOps.Api.Modules.Staff;

using StoreOps.Api.Modules.Staff.Models;

// Staff is intentionally auth-only in this codebase: no CRUD HTTP surface
// (Section 3.6 — "staff is auth-only (no CRUD surface here)"). It exists so other
// modules have a read-only lookup target for user references (AssigneeId, ActorId).
public interface IStaffRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<List<User>> ListAsync();
}
