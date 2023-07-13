function _Create(props)
    local result = CardCreation:Unit(props)
    result:AddSubtype('Mage')

    -- TODO move to a enter play signal
    result.OnEnterP:AddLayer(function (playerID, tile)
        local card = SummonCard(result.id, playerID, 'starters::Inspiration')
        PlaceCardInHand(playerID, card.id)
        return nil, true
    end)
    return result
end