package.path = package.path..';../bots/lua/?.lua'

local json = require 'json'
-- math.randomseed(0)

function TableToStr(t)
    if type(t) == 'table' then
        local s = '{ '
        for k,v in pairs(t) do
            if type(k) ~= 'number' then k = '"'..k..'"' end
            s = s .. '['..k..'] = ' .. TableToStr(v) .. ','
        end
        return s .. '} '
    else
        return tostring(t)
    end
end

function TableLength(T)
    if T == nil then
        return 0
    end
    local count = 0
    for _ in pairs(T) do count = count + 1 end
    return count
end

ActionPart = {}
function ActionPart:New()
    local result = {}

    function result:ToStr()
        return "<ToStr-not-implemented>"
    end

    function result:ToShort()
        return "<ToShort-not-implemented>"
    end

    return result
end


function ActionPart:Card(card)
    local result = ActionPart:New()
    result.card = card

    function result:ToStr()
        return self.card.mid
    end

    function result:ToShort()
        return self.card.id
    end
    
    return result
end


function ActionPart:Tile(tile)
    local result = ActionPart:New()
    result.tile = tile
    function result:ToShort()
        return self.tile.iPos..'.'..self.tile.jPos
    end
    return result
end


function ActionPart:SourceTile(tile)
    local result = ActionPart:Tile(tile)

    function result:ToStr()
        return self:ToShort()
    end

    return result
end


function ActionPart:DirectionTile(tile)
    local result = ActionPart:Tile(tile)
    function result:ToStr()
        -- TODO
        return ''..((self.tile.i+5)%6)
    end
    return result
end


Actions = {}
function Actions:New(name, short)
    short = short or name

    local result = {}
    result.name = name
    result.short = short
    result.parts = {}

    function result:ToStr()
        local s = self.name
        for _, part in ipairs(self.parts) do
            s = s..' '..part:ToStr()
        end
        return s
    end

    function result:ToShort()
        local s = self.short
        for _, part in ipairs(self.parts) do
            s = s..' '..part:ToShort()
        end
        return s
    end

    return result
end


function Actions:PlayCard()
    local result = Actions:New('play')

    return result
end


function Actions:MoveEntity()
    local result = Actions:New('move')

    return result
end


function Actions:Attack()
    local result = Actions:New('move', 'attack')

    return result
end

DIR_ARR = {
    {
        {-2, 0},
        {-1, 1},
        {1, 1},
        {2, 0},
        {1, 0},
        {-1, 0}
    },
    {
        {-2, 0},
        {-1, 0},
        {1, 0},
        {2, 0},
        {1, -1},
        {-1, -1},
    }
}

function GetAvailableActions(state)
    local myID = state.myData.id
    local tiles = state.map.tiles
    local tilesForEntities = {}
    local tilesForSpells = {}
    local moveTilesFrom = {}
    local me = nil
    for _, player in ipairs(state.players) do
        if player.id == myID then
            me = player
            break
        end
    end
    for ii, row in ipairs(tiles) do
        for jj, tile in ipairs(row) do
            local i = ii - 1
            local j = jj - 1
            if TableLength(tile) == 0 then
                goto continue
            end
            local t = {
                tile = tile,
                iPos = i,
                jPos = j
            }
            if TableLength(tile.entity) == 0 then
                if tile.ownerID == myID then
                    tilesForEntities[#tilesForEntities+1] = t
                end
                goto continue
            end

            local en = tile.entity
            if en.ownerID == myID then
                if string.find(en.type, 'Unit') then
                    if en.movement > 0 then
                        moveTilesFrom[#moveTilesFrom+1] = t
                    end
                    if string.find(en.type, 'Mage') then
                        tilesForSpells[#tilesForSpells+1] = t
                    end
                end
            end
            ::continue::
        end
    end
    -- playing cards
    local result = {}
    for _, card in ipairs(state.myData.myHand) do
        if card.cost <= me.energy then
            if card.type == 'Spell' then
                -- spell
                for _, tile in ipairs(tilesForSpells) do
                    local action = Actions:PlayCard()
                    action.parts[#action.parts+1] = ActionPart:Card(card)
                    action.parts[#action.parts+1] = ActionPart:SourceTile(tile)
                    result[#result+1] = action
                end
            else
                -- entity
                for _, tile in ipairs(tilesForEntities) do
                    local action = Actions:PlayCard()
                    action.parts[#action.parts+1] = ActionPart:Card(card)
                    action.parts[#action.parts+1] = ActionPart:SourceTile(tile)
                    result[#result+1] = action
                end
            end
        end

    end

    -- moving entities
    for _, tile in ipairs(moveTilesFrom) do
        local dirs = DIR_ARR[(tile.iPos % 2) + 1]
        for i, dir in ipairs(dirs) do
            local newI = tile.iPos + dir[1]
            local newJ = tile.jPos + dir[2]
            local t = tiles[newI+1][newJ+1]
            if TableLength(t) ~= 0 then
                if TableLength(t.entity) == 0 then
                    local action = Actions:MoveEntity()
                    action.parts[#action.parts+1] = ActionPart:SourceTile(tile)
                    action.parts[#action.parts+1] = ActionPart:DirectionTile({tile=t, iPos=newI, jPos = newJ, i=i})
                    result[#result+1] = action
                else
                    local en = t.entity
                    if en.ownerID ~= myID then
                        local action = Actions:Attack()
                        action.parts[#action.parts+1] = ActionPart:SourceTile(tile)
                        action.parts[#action.parts+1] = ActionPart:DirectionTile({tile=t, iPos=newI, jPos = newJ, i=i})
                        result[#result+1] = action
                    end
                end
            end
        end
    end
    return result
end


function ParseState(state)
    local result = json.parse(state)
    if result ~= nil then
        return result
    end
    return {}
end


function _PromptAction(statej)
    local state = ParseState(statej)
    local actions = GetAvailableActions(state)
    -- for _, action in ipairs(actions) do
    --     print(action:ToShort()..'  '..action:ToStr())
    -- end
    if #actions == 0 then
        return "pass"
    end
    -- TODO not tested
    local result = actions[math.random(#actions)]
    print('Result: '..result:ToShort()..' '..result:ToStr())
    return result:ToStr()
end


function _Setup()

end


function _Update()

end
