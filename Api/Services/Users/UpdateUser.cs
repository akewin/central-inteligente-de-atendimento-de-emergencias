using Api.Dtos;
using Api.Interfaces.Repositories;
using Api.Middlewares;
using Api.Models;
using Api.Security.Passwords;
using Api.Validations;

namespace Api.Services.Users;

public class UpdateUser
{
    private readonly IUserRepository _repository;
    private readonly UserValidator _validator;

    public UpdateUser(
        IUserRepository repository,
        UserValidator validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<User> ExecuteAsync(UserUpdateDto dto)
    {
        Guard.AgainstNonPositiveInt(dto.Id);

        var user = await _repository.GetByIdAsync(dto.Id)
            ?? throw new AppException("Usuário não encontrado.");

        await _validator.ValidateUpdateAsync(dto, user);

        if (!string.IsNullOrWhiteSpace(dto.Username))
            user.Username = dto.Username;

        if (!string.IsNullOrWhiteSpace(dto.Email))
            user.Email = dto.Email;

        if (!string.IsNullOrWhiteSpace(dto.FullName))
            user.FullName = dto.FullName;

        if (!string.IsNullOrWhiteSpace(dto.Password))
            user.Password = PasswordHash.Generate(dto.Password);

        if (dto.HasUnitId)
            user.UnitId = dto.UnitId;
        if (dto.HasHospitalId)
            user.HospitalId = dto.HospitalId;

        user.UpdatedAt = DateTime.UtcNow;

        return await _repository.UpdateAsync(user);
    }
}
