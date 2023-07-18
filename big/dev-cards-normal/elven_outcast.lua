function _Create(props)
    local result = CardCreation:Unit(props)
    result:AddSubtype('Rogue')

    -- TODO replace with trigger
    result.OnEnterP:AddLayer(function (playerID, tile)
        DealDamage(result.id, {tile.iPos, tile.jPos}, 1)
        return nil, true
    end)

    return result
end