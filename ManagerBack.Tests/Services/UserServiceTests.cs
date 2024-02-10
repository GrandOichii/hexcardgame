// using AutoMapper;
// using FakeItEasy;
// using FluentAssertions;
// using Microsoft.AspNetCore.Mvc;

// namespace ManagerBack.Tests.Services;

// public class UserServiceTests {
//     private readonly UserService _userService;
//     private readonly IUserRepository _userRepo;

//     // TODO add authorized tests

//     public UserServiceTests() {
//         var mC = new MapperConfiguration(cfg => {
//             cfg.AddProfile(new AutoMapperProfile());
//         });
//         var mapper = new Mapper(mC);
//         _userRepo = A.Fake<IUserRepository>();

//         _userService = new(mapper, _userService);
//     }

//     [Fact]
//     public async Task ShouldFetchByCID() {
//         // Arrange
//         var cid = "expansion::card";
//         var card = A.Fake<CardModel>();
//         A.CallTo(() => _cardRepo.ByCID(cid)).Returns(card);

//         // Act
//         var result = await _cardService.ByCID(cid);

//         // Assert
//         result.Should().Be(card);
//     }

// }