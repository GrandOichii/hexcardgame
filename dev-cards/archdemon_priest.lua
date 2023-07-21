-- TODO not tested
function _Create(props)
    local result = CardCreation:Unit(props)

    -- TODO move to a keyword
    result.OnEnterP:AddLayer(function (playerID, tile)
        local card = SummonCard(result.id, playerID, 'dev::Corrupting Darkness')
        PlaceCardInHand(playerID, card.id)
        return nil, true
    end)
    return result
end