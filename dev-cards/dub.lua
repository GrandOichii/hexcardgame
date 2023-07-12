-- TODO not tested
function _Create(props)
    local result = CardCreation:Spell(props)
    result.DamageValues.damage = 2
    result.EffectP:AddLayer(function(playerID, caster)
        caster.type = caster.type..' Warrior'
        caster:AddSubtype('Warrior')
        return nil, true
    end)
    return result
end