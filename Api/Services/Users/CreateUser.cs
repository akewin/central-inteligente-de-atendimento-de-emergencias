using Api.Auditing.Services;
using Api.Dtos;
using Api.Interfaces.Repositories;
using Api.Models;
using Api.Security.Passwords;
using Api.Validations;

namespace Api.Services.Users;

public class CreateUser
{
    private readonly IUserRepository _repository;
    private readonly UserValidator _validator;

    public CreateUser(
        IUserRepository repository,
        UserValidator validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<User> ExecuteAsync(UserCreateDto dto)
    {
        await _validator.ValidateCreateAsync(dto);

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            FullName = dto.FullName,
            Password = PasswordHash.Generate(dto.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Active = true,
            UnitId = dto.UnitId,
            HospitalId = dto.HospitalId
        };

        return await _repository.CreateAsync(user);
    }
}
