-- TODO fix and add back
function _Create(props)
    local result = CardCreation:Spell(props)
    result.DamageValues.Damage = 4

    -- TODO this is messed up, have to check each Mage that can cast?
    -- TODO make so that can be cast only by mages with neighboring units
    -- TODO the easiest way for now is to just cancel the effect of the card if no Units are nearby
    -- result.CanPlayP:AddLayer(function (playerID)
    --     return nil, Common:HasNeighborUnit(result)
    -- end)

    result.EffectP:AddLayer(function(playerID, caster)
        local tiles = Common:GetNeighboringUnitsAndStructures(caster)
        -- TODO this is a band aid for the CanPlay function
        if #tiles == 0 then
            return nil, true
        end
        local choices = {}
        for _, tile in ipairs(tiles) do
            choices[#choices+1] = { tile.iPos, tile.jPos }
        end
        local tile = PickTile(playerID, result.id, choices)
        DealDamage(result.id, { tile.iPos, tile.jPos }, result.DamageValues.Damage)
        return nil, true
    end)
    return result
end