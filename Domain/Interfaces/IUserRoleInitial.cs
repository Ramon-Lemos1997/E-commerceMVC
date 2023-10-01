namespace Domain.Interfaces

{
    public interface IUserRoleInitial
    {
        Task RolesAsync();
        Task UsersAsync();
    }
}
