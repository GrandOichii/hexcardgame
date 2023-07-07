-- TODO not tested
function _Create(props)
    local result = CardCreation:Spell(props)
    result.DamageValues.damage = 2
    result.EffectP:AddLayer(function(playerID, caster)
        caster.defence = caster.defence + 1
        caster:AddDefence(1)
        return nil, true
    end)
    return result
end