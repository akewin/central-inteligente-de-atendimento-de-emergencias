using Api.Dtos;
using Api.Interfaces.Repositories;
using Api.Middlewares;
using Api.Models;

namespace Api.Validations;

public class UserValidator
{
    private readonly IUserRepository _repository;

    public UserValidator(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task ValidateCreateAsync(UserCreateDto dto)
    {
        Guard.AgainstNullOrEmpty(dto.Username, nameof(dto.Username));
        Guard.AgainstNullOrEmpty(dto.Email, nameof(dto.Email));
        Guard.AgainstNullOrEmpty(dto.Password, nameof(dto.Password));
        Guard.AgainstNullOrEmpty(dto.FullName, nameof(dto.FullName));

        if (await _repository.ExistsByUsernameAsync(dto.Username))
            throw new AppException("Username já está em uso.");

        if (await _repository.ExistsByEmailAsync(dto.Email))
            throw new AppException("Email já está em uso.");

        if (dto.PermissionIds == null || !dto.PermissionIds.Any())
            throw new AppException("Usuário deve possuir ao menos uma permissão.");
    }

    public async Task ValidateUpdateAsync(UserUpdateDto dto, User user)
    {
        if (dto.Username != user.Username &&
            await _repository.ExistsByUsernameAsync(dto.Username))
        {
            throw new AppException("Username já está em uso.");
        }

        if (dto.Email != user.Email &&
            await _repository.ExistsByEmailAsync(dto.Email))
        {
            throw new AppException("Email já está em uso.");
        }
    }
}
