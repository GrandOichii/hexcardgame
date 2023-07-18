function _Create(props)
    local result = CardCreation:Spell(props)
    result.DamageValues.Damage = 3

    result.EffectP:AddLayer(function(playerID, caster)
        local tiles = GetUnitsOnBoard()
        local choices = {}
        for _, tile in ipairs(tiles) do
            choices[#choices+1] = { tile.iPos, tile.jPos }
        end
        local tile = PickTile(playerID, result.id, choices)
        DealDamage(result.id, { tile.iPos, tile.jPos }, result.DamageValues.Damage)
        return nil, true
    end)
    return result
end