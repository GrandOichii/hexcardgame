using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Tests.Controllers;

public class CardExpansionTests {
    private readonly ExpansionService _expansionService;
    private readonly ICardRepository _cardRepo;

    // TODO add authorized tests

    public CardExpansionTests() {
        _cardRepo = A.Fake<ICardRepository>();

        _expansionService = new(_cardRepo);
    }

    [Fact]
    public async Task ShouldFetchByCID() {
        // Arrange
        var cid = "expansion::card";
        var card = A.Fake<CardModel>();
        A.CallTo(() => _cardRepo.ByCID(cid)).Returns(card);

        // Act
        var result = await _cardService.ByCID(cid);

        // Assert
        result.Should().Be(card);
    }

}