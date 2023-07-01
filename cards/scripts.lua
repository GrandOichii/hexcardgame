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

function _Create(props)
    local result = CardCreation:Spell(props)
    result.EffectP:AddLayer(function(playerID, caster)
        local tile = GetTileWith(caster.id)
        if tile == nil then
            -- TODO warn
            return
        end
        local dead = DealDamage(result.id, {tile.iPos, tile.jPos}, 1)
        if dead then
            print('amogus')
            return nil, true
        end
        DrawCards(playerID, 2)
        return nil, true
    end)
    return result
end