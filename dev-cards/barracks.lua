function _Create(props)
    local result = CardCreation:Structure(props)

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnerEntity(result) and Common:UnitEntered() and Common:IsNeighbor(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.ENTITY_ENTER)
        :Zone(ZONES.PLACED)
        :Effect(function (playerID, args)
            local card = GetCard(args.mid)
            card:AddBaseKeyword('Fast', true)
            -- TODO bad
            card.movement = card.maxMovement
        end)
        :Build()

    return result
end
