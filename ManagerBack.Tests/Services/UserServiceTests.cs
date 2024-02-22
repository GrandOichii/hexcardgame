using AutoMapper;
using BCrypt.Net;
using FakeItEasy;
using FluentAssertions;
using ManagerBack.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ManagerBack.Tests.Services;

public class UserServiceTests {
    private readonly IMapper _mapper;
    private readonly UserService _userService;
    private readonly IUserRepository _userRepo;
    private readonly IValidator<PostUserDto> _validator;

    public UserServiceTests() {
        var mC = new MapperConfiguration(cfg => {
            cfg.AddProfile(new AutoMapperProfile());
        });
        _mapper = new Mapper(mC);
        _userRepo = A.Fake<IUserRepository>();
        _validator = A.Fake<IValidator<PostUserDto>>();
              
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        _userService = new(_mapper, configuration, _userRepo, _validator);
    }

    [Fact]
    public async Task ShouldRegister() {
        // Arrange
        var user = A.Fake<PostUserDto>();
        User? existing = null;
        A.CallTo(() => _validator.Validate(user)).DoesNothing();
        A.CallTo(() => _userRepo.ByUsername(user.Username)).Returns(existing);
        A.CallTo(() => _userRepo.Add(_mapper.Map<User>(user))).DoesNothing();

        // Act
        var result = await _userService.Register(user);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldNotRegister() {
        // Arrange
        var user = A.Fake<PostUserDto>();
        A.CallTo(() => _validator.Validate(user)).Throws(new InvalidRegisterCredentialsException(""));

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

    [Fact]
    public async Task ShouldNotLogin() {
        // Arrange
        var postUser = A.Fake<PostUserDto>();
        User? nullUser = null;
        A.CallTo(() => _userRepo.ByUsername(postUser.Username)).Returns(nullUser);

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