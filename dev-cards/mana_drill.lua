function _Create(props)
    local result = CardCreation:Structure(props)
        result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersTurn(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.TURN_START)
        :Zone(ZONES.PLACED)
        :Effect(function (playerID, args)
        AddEnergy(playerID, 1)
    end)
        :Build()
    return result
end