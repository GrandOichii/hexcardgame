
function _Create(props)
    local result = CardCreation:Spell(props)
    result.DamageValues.damage = 2
    result.EffectP:AddLayer(function(playerID, caster)
        local tile = GetTileWith(caster.id)
        
        local neighbors = GetNeighbors({ tile.iPos, tile.jPos })
        for _, neighbor in ipairs(neighbors) do
            if neighbor ~= nil and neighbor.entity ~= nil and neighbor.entity.ownerID ~= playerID then
                print(neighbor.entity.ownerID..' '..playerID)
                DealDamage(result.id, {neighbor.iPos, neighbor.jPos}, result.DamageValues.damage)
            end
        end

        return nil, true
    end)
    return result
end