function _Create(props)
    local result = CardCreation:Spell(props)
    result.EffectP:AddLayer(function(playerID, caster)
        caster.baseDefence = caster.baseDefence + 1
        caster:AddDefence(1)
        return nil, true
    end)
    return result
end