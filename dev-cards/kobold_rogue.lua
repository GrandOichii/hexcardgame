
-- TODO not tested
function _Create(props)
    local result = CardCreation:Unit(props)
    result:AddSubtype('Rogue')
    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:Moved(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.TURN_START)
        :Zone(ZONES.PLACED)
        :Effect(function (playerID, args)
            result.power = result.power + 1
        end)
        :Build()

    return result
end