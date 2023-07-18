function _Create(props)
    local result = CardCreation:Unit(props)
    result:AddSubtype('Mage')
    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsCaster(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.SPELL_CAST)
        :Zone(ZONES.PLACED)
        :Effect(function (playerID, args)
            result.power = result.power + 1
            result.life = result.life + 1
        end)
        :Build()
    return result
end