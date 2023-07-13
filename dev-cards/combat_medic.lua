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
            local tile = GetTileWith(result.id)

            local neighbors = GetNeighbors({ tile.iPos, tile.jPos })
            for _, neighbor in ipairs(neighbors) do
                if neighbor ~= nil and neighbor.entity ~= nil and neighbor.entity:IsUnit() and neighbor.entity.ownerID == playerID then
                    neighbor.entity.life = neighbor.entity.life + 1
                    if neighbor.entity.life > neighbor.entity.baseLife then
                        neighbor.entity.life = neighbor.entity.baseLife
                    end
                end
            end
        end)
        :Build()
    return result
end