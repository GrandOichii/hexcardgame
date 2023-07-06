
-- TODO not tested
function _Create(props)
    local result = CardCreation:Spell(props)
    result.DamageValues.Damage = 1
    result.EffectP:AddLayer(function(playerID, caster)
        -- TODO
        -- get all units (faster)
        -- iterate through all of them
        -- deal 3 damage to each one
    end)
    return result
end