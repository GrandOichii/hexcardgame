function _Create(props)
    local result = CardCreation:Spell(props)
    result.DamageValues.Damage = 3
    result.EffectP:AddLayer(function(playerID, caster)
        local tiles = GetUnitsOnBoard()
        for _, tile in ipairs(tiles) do
            if string.find(tile.entity.type, 'Unit') then
                DealDamage(result.id, {tile.iPos, tile.jPos}, result.DamageValues.Damage)
            end
        end
        return nil, true
    end)
    return result
end