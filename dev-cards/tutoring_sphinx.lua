-- TODO not tested
function _Create(props)
    local result = CardCreation:Unit(props)
    result:AddSubtype('Mage')

    -- TODO? move to a signal?
    result.OnEnterP:AddLayer(function (playerID, tile)
        DrawCards(playerID, 2)
    end)
    return result
end