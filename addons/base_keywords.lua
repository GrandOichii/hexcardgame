
KEYWORD_MAP = {
    Virtuous = function (unit)
        unit.OnEnterP:AddLayer(function (playerID, tile)
            if not unit:HasKeyword('Virtuous') then
                return nil, true
            end
            local card = SummonCard(unit.id, playerID, 'dev::Healing Light')
            PlaceCardInHand(playerID, card.id)
            return nil, true
        end)
    end,
    Vile = function (unit)
        unit.OnEnterP:AddLayer(function (playerID, tile)
            if not unit:HasKeyword('Vile') then
                return nil, true
            end
            local card = SummonCard(unit.id, playerID, 'dev::Corrupting Darkness')
            PlaceCardInHand(playerID, card.id)
            return nil, true
        end)
    end,
    Fast = function (unit)
        unit.OnEnterP:AddLayer(function (playerID, tile)
            if not unit:HasKeyword('Fast') then
                return nil, true
            end
            unit.movement = unit.maxMovement
            return nil, true
        end)
    end
}


function ApplyKeywords(unit, keywordMap)
    for key, value in pairs(keywordMap) do
        value(unit)
    end
end


function _Apply()
    
    local prevF = CardCreation.Unit
    function CardCreation:Unit(props)
        local unit = prevF(CardCreation, props)

        unit.baseKeywords = {}
        unit.HasKeywordP = Pipeline:New()
        unit.HasKeywordP:AddLayer(
            function (keyword)
                for _, k in ipairs(unit.baseKeywords) do
                    if k == keyword then
                        return true, true
                    end
                end
                return false, true
            end
        )

        function unit:HasKeyword(keyword)
            local res, _ = unit.HasKeywordP:Exec(keyword)
            return res
        end

        function unit:AddBaseKeyword(keyword, addToText)
            unit.baseKeywords[#unit.baseKeywords+1] = keyword
            if addToText then
                unit.text = unit.text .. '\n{' .. keyword .. '}'
            end
        end

        ApplyKeywords(unit, KEYWORD_MAP)

        return unit
    end

end