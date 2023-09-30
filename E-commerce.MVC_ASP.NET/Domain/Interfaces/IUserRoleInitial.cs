namespace E_commerce.MVC_ASP.NET.Domain.Interfaces

{
    public interface IUserRoleInitial
    {
        Task RolesAsync();
        Task UsersAsync();
    }
}
