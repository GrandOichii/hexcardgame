
function _Create(props)
    local result = CardCreation:Spell(props)
    result.EffectP:AddLayer(function(playerID, caster)
        local tile = GetTileWith(caster.id)
        local tiles = GetNeighbors( {tile.iPos, tile.jPos} )
        tiles[#tiles+1] = tile
        for _, t in ipairs(tiles) do
            TileOwnerSet(playerID, { {t.iPos, t.jPos} })
        end
        return nil, true
    end)
    return result
end