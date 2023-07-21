function _Create(props)
    local result = CardCreation:Spell(props)
    result.DamageValues.Damage = 1
    result.EffectP:AddLayer(function(playerID, caster)
        local tiles = Common:GetNeighboringUnits(caster)
        -- TODO this is a band aid for the CanPlay function
        if #tiles == 0 then
            return nil, true
        end
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