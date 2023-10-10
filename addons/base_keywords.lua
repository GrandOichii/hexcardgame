
KEYWORD_MAP = {
    Virtuous = function (entity)
        entity.OnEnterP:AddLayer(function (playerID, tile)
            if not entity:HasKeyword('Virtuous') then
                return nil, true
            end
            local card = SummonCard(entity.id, playerID, 'dev::Healing Light')
            PlaceCardInHand(playerID, card.id)
            return nil, true
        end)
    end,
    Vile = function (entity)
        entity.OnEnterP:AddLayer(function (playerID, tile)
            if not entity:HasKeyword('Vile') then
                return nil, true
            end
            local card = SummonCard(entity.id, playerID, 'dev::Corrupting Darkness')
            PlaceCardInHand(playerID, card.id)
            return nil, true
        end)
    end,
    Fast = function (entity)
        entity.OnEnterP:AddLayer(function (playerID, tile)
            if not entity:HasKeyword('Fast') then
                return nil, true
            end
            entity.movement = entity.maxMovement
            return nil, true
        end)
    end
}


function ApplyKeywords(entity, keywordMap)
    for key, value in pairs(keywordMap) do
        value(entity)
    end
end


function _Apply()
    
    local prevF = CardCreation.Placeable
    function CardCreation:Placeable(props)
        local entity = prevF(CardCreation, props)

        entity.baseKeywords = {}
        entity.HasKeywordP = Pipeline:New()
        entity.HasKeywordP:AddLayer(
            function (keyword)
                for _, k in ipairs(entity.baseKeywords) do
                    if k == keyword then
                        return 1, true
                    end
                end
                return 0, true
            end
        )

        function entity:HasKeyword(keyword)
            local res, _ = entity.HasKeywordP:Exec(keyword)
            return res > 0
        end

        function entity:AddBaseKeyword(keyword, addToText)
            entity.baseKeywords[#entity.baseKeywords+1] = keyword
            if addToText then
                entity.text = entity.text .. '\n{' .. keyword .. '}'
            end
        end

        ApplyKeywords(entity, KEYWORD_MAP)

        return entity
    end

end