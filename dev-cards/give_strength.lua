
-- TODO not tested
function _Create(props)
    local result = CardCreation:Spell(props)
    result.EffectP:AddLayer(function(playerID, caster)
        caster.power = caster.power + 2
        caster.life = caster.life + 2
        return nil, true
    end)
    return result
end