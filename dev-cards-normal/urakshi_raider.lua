
function _Create(props)
    local result = CardCreation:Unit(props)

    result:AddSubtype('Warrior')

    -- TODO replace with special keyword
    result.OnEnterP:AddLayer(function (playerID, tile)
        result.movement = result.maxMovement
        return nil, true
    end)

    return result
end