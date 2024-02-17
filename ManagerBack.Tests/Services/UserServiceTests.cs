using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ManagerBack.Tests.Services;

public class UserServiceTests {
    private readonly IMapper _mapper;
    private readonly UserService _userService;
    private readonly IUserRepository _userRepo;

    public UserServiceTests() {
        var mC = new MapperConfiguration(cfg => {
            cfg.AddProfile(new AutoMapperProfile());
        });
        _mapper = new Mapper(mC);
        _userRepo = A.Fake<IUserRepository>();

              
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        _userService = new(_mapper, configuration, _userRepo, new PostUserDtoValidator());
    }

    [Fact]
    public async Task ShouldRegister() {
        // Arrange
        var user = new PostUserDto {
            Username = "username",
            Password = "password"
        };
        User? existing = null;
        A.CallTo(() => _userRepo.ByUsername(user.Username)).Returns(existing);
        A.CallTo(() => _userRepo.Add(_mapper.Map<User>(user))).DoesNothing();

        // Act
        var result = await _userService.Register(user);

        // Assert
        result.Should().NotBeNull();
    }

    // * user validation is already checked in the endpoint tests, so they should be here i think

    [Fact]
    public async Task ShouldNotRegister() {
        // Arrange
        var user = new PostUserDto {
            Username = "username",
            Password = "password"
        };
        A.CallTo(() => _userRepo.ByUsername(user.Username)).Returns(A.Fake<User>());

        // Act
        var act = () => _userService.Register(user);

        // Assert
        await act.Should().ThrowAsync<UsernameTakenException>();
    }   

    [Fact]
    public async Task ShouldLogin() {
        // Arrange
        var postUser = new PostUserDto {
            Username = "user",
            Password = "password"
        };
        var user = new User {
            Id = "u1",
            Username = postUser.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(postUser.Password)
        };
        A.CallTo(() => _userRepo.ByUsername(postUser.Username)).Returns(user);

        // Act
        var result = await _userService.Login(postUser);

        // Assert
        result.Should().NotBeNull();
    }

    // TODO? should these tests be here

    [Fact]
    public async Task ShouldNotLogin() {
        // Arrange
        var postUser = new PostUserDto {
            Username = "user",
            Password = "password"
        };
        User? user = null;
        A.CallTo(() => _userRepo.ByUsername(postUser.Username)).Returns(user);

        // Act
        var act = () => _userService.Login(postUser);

        // Assert
        await act.Should().ThrowAsync<InvalidLoginCredentialsException>();
    }

    [Fact]
    public async Task ShouldNotLoginWrongPassword() {
        // Arrange
        var postUser = new PostUserDto {
            Username = "user",
            Password = "password"
        };
        var user = new User {
            Id = "u1",
            Username = postUser.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("stronger-password")
        };
        A.CallTo(() => _userRepo.ByUsername(postUser.Username)).Returns(user);

        // Act
        var act = () => _userService.Login(postUser);

        // Assert
        await act.Should().ThrowAsync<InvalidLoginCredentialsException>();
    }

}