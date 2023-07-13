-- TODO not tested
function _Create(props)
    local result = CardCreation:Unit(props)

    result:AddSubtype('Warrior')

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersTurn(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.TURN_START)
        :Zone(ZONES.PLACED)
        :Effect(function (playerID, args)
            if result.power == result.life then
                result.power = result.power + 1
                result.life = result.life + 1
            end
        end)
        :Build()


    return result
end