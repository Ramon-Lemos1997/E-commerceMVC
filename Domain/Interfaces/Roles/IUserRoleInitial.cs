namespace Domain.Interfaces.Roles
{
    public interface IUserRoleInitial
    {
        Task RolesAsync();
        Task UsersAsync();
    }
}
