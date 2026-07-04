using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Mappers;

public static class UserMapper
{
    public static UserDto ToCreateUser(this User user)
    {
        return new UserDto
        {
            Name = user.Name,
            Email = user.Email
        };
    }

    public static User ToUser(this UserDto userDto)
    {
        return new User
        {
            Id = userDto.Id,
            Name = userDto.Name,
            Email = userDto.Email
        };
    }

    public static List<User> ToUserList(this List<GetConnectedUserDto> connectedUsers)
    {
        var users = new List<User>();

        foreach (var connectedUser in connectedUsers)
        {
            var user = new User
            {
                Id = connectedUser.Id,
                Name = connectedUser.Username,
                Email = connectedUser.Email
            };

            users.Add(user);
        }

        return users;
    }
}