
-- TODO not tested
function _Create(props)
    local result = CardCreation:Unit(props)
    result:AddSubtype('Warrior')
    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:WasDealtDamage(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.DAMAGE_DEALT)
        :Zone(ZONES.PLACED)
        :Effect(function (playerID, args)
            result.baseDefence = result.baseDefence + 1
            result:AddDefence(1)
        end)
        :Build()
    return result
end