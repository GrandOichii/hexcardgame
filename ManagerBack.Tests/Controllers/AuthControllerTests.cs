using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Tests.Controllers;

public class AuthControllerTests {

    private readonly IUserService _userService;
    private readonly AuthController _authController;
    public AuthControllerTests() {
        _userService = A.Fake<IUserService>();

        _authController = new(_userService);
    }


    [Fact]
    public async Task ShouldRegister() {
        // Arrange
        var postUser = A.Fake<PostUserDto>();
        var user = A.Fake<GetUserDto>();
        A.CallTo(() => _userService.Register(postUser)).Returns(user);

        // Act
        var result = await _authController.Register(postUser);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    } 

    
    [Fact]
    public async Task ShouldNotRegister() {
        // Arrange
        var postUser = A.Fake<PostUserDto>();
        var user = A.Fake<GetUserDto>();
        A.CallTo(() => _userService.Register(postUser)).Throws(new UsernameTakenException(""));

        // Act
        var result = await _authController.Register(postUser);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }   
}