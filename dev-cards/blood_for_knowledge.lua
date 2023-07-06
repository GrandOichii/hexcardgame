
function _Create(props)
    local result = CardCreation:Spell(props)
    result.DamageValues.Damage = 1
    result.EffectP:AddLayer(function(playerID, caster)
        local tile = GetTileWith(caster.id)
        if tile == nil then
            return nil, true
        end
        local dead = DealDamage(result.id, {tile.iPos, tile.jPos}, result.DamageValues.Damage)
        if dead then
            print('amogus')
            return nil, true
        end
        DrawCards(playerID, 2)
        return nil, true
    end)
    return result
end