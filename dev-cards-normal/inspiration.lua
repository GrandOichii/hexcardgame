
function _Create(props)
    local result = CardCreation:Spell(props)
    result.DamageValues.Damage = 1
    result.EffectP:AddLayer(function(playerID, caster)
        DrawCards(playerID, 2)
        return nil, true
    end)
    return result
end