
-- TODO not tested
function _Create(props)
    local result = CardCreation:Spell(props)
    result.DamageValues.Damage = 3
    result.EffectP:AddLayer(function(playerID, caster)
        local units = GetUnitsOnBoard()
        for _, unit in ipairs(units) do
            DealDamage(result.id, unit.id, result.DamageValues.Damage)
        end
    end)
    return result
end