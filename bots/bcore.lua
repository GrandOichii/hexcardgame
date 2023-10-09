package.path = package.path..';../bots/lua/?.lua'

local json = require 'json'
-- math.randomseed(0)

local bcore = {}

function ReadJ(path)
    local file = io.open(path, 'r')
    if file == nil then
        print('Failed to open data file')
        os.exit(1)
    end
    local datat = file:read('*a')
    local data = json.parse(datat)

    if data == nil then
        print('Failed to parse data')
        os.exit(1)
    end
    file:close()
    return data
end

function WriteJ(path, data)
    local file = io.open(path, 'w')
    if file == nil then
        print('Failed to open data file')
        os.exit(1)
    end
    local t = json.stringify(data)
    -- print(t)
    file:write(t)
    -- file:flush()
    file:close()
end

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


ACC_MAP = {
    castle_distance_y = {
        fun = function (state, pID)
            local tiles = state.map.tiles
            local castleY = nil
            local unit_ys = {}
            for i, row in ipairs(tiles) do
                for j, tile in ipairs(row) do
                    -- check if is castle
                    if TableLength(tile) ~= 0 and TableLength(tile.entity) ~= 0 then
                        local en = tile.entity
                        if en.ownerID == pID then
                            if string.find(en.type, 'Unit') then
                                unit_ys[#unit_ys+1] = i
                            end
                        else
                            if en.name == 'Castle' then
                                castleY = i
                            end
                        end
                    end

                end
            end
            local closestD = nil
            for _, y in ipairs(unit_ys) do
                local v = math.abs(castleY - y)
                if closestD == nil or v < closestD then
                    closestD = v
                end
            end
            if closestD == nil then
                closestD = #tiles
            end
            return #tiles - closestD
        end,
        me = 10,
        enemy = -5
    },
    castle_life = {
        fun = function (state, pID)
            local tiles = state.map.tiles
            for _, row in ipairs(tiles) do
                for _, tile in ipairs(row) do
                    if TableLength(tile) > 0 and TableLength(tile.entity) > 0 and tile.entity.ownerID == pID and tile.entity.name == 'Castle' then
                        return tile.entity.life + tile.entity.defence
                    end
                end
            end
            return 0
        end,
        me = 10,
        enemy = -10
    },
    max_energy = {
        fun = function (state, pID)
            local tiles = state.map.tiles
            local result = 0
            for _, row in ipairs(tiles) do
                for _, tile in ipairs(row) do
                    if TableLength(tile) > 0 and TableLength(tile.entity) > 0 and tile.entity.ownerID == pID and tile.entity.name == 'Mana Drill' then
                        result = result + 1
                    end
                end
            end
            return result
        end,
        me = 3,
        enemy = -3
    },
    total_life = {
        fun = function (state, pID)
            local tiles = state.map.tiles
            local result = 0
            for _, row in ipairs(tiles) do
                for _, tile in ipairs(row) do
                    if TableLength(tile) > 0 and TableLength(tile.entity) > 0 and tile.entity.ownerID == pID then
                        result = result + tile.entity.life + tile.entity.defence
                    end
                end
            end
            return result
        end,
        me = 3,
        enemy = -3
    },
    total_power = {
        fun = function (state, pID)
            local tiles = state.map.tiles
            local result = 0
            for _, row in ipairs(tiles) do
                for _, tile in ipairs(row) do
                    if TableLength(tile) > 0 and TableLength(tile.entity) > 0 and tile.entity.ownerID == pID and string.find(tile.entity.type, 'Unit') then
                        result = result + tile.entity.power
                    end
                end
            end
            return result
        end,
        me = 3,
        enemy = -3
    },
    warrior_count = {
        fun = function (state, pID)
            local tiles = state.map.tiles
            local result = 0
            for _, row in ipairs(tiles) do
                for _, tile in ipairs(row) do
                    if TableLength(tile) > 0 and TableLength(tile.entity) > 0 and tile.entity.ownerID == pID and string.find(tile.entity.type, 'Warrior') then
                        result = result + 1
                    end
                end
            end
            return result
        end,
        me = 2,
        enemy = -2
    },
    rogue_count = {
        fun = function (state, pID)
            local tiles = state.map.tiles
            local result = 0
            for _, row in ipairs(tiles) do
                for _, tile in ipairs(row) do
                    if TableLength(tile) > 0 and TableLength(tile.entity) > 0 and tile.entity.ownerID == pID and string.find(tile.entity.type, 'Rogue') then
                        result = result + 1
                    end
                end
            end
            return result
        end,
        me = 2,
        enemy = -2
    },
    mage_count = {
        fun = function (state, pID)
            local tiles = state.map.tiles
            local result = 0
            for _, row in ipairs(tiles) do
                for _, tile in ipairs(row) do
                    if TableLength(tile) > 0 and TableLength(tile.entity) > 0 and tile.entity.ownerID == pID and string.find(tile.entity.type, 'Mage') then
                        result = result + 1
                    end
                end
            end
            return result
        end,
        me = 2,
        enemy = -2
    },
    structure_count = {
        fun = function (state, pID)
            local tiles = state.map.tiles
            local result = 0
            for _, row in ipairs(tiles) do
                for _, tile in ipairs(row) do
                    if TableLength(tile) > 0 and TableLength(tile.entity) > 0 and tile.entity.ownerID == pID and string.find(tile.entity.type, 'Structure') then
                        result = result + 1
                    end
                end
            end
            return result
        end,
        me = 2,
        enemy = -2
    },
    hand_count = {
        fun = function (state, pID)
            for _, player in ipairs(state.players) do
                if player.id == pID then
                    return player.handCount
                end
            end
            return 0
        end,
        me = 2,
        enemy = -2
    },
    
}

PREV_ADVANTAGE_TOTAL = 0
PREV_ADVANTAGE = nil
function CalculateAdvantage(state)
    local result1 = 0
    local result2 = {}
    local meID = state.myData.id
    local enemyID = ''

    for _, player in ipairs(state.players) do
        if player.id ~= meID then
            enemyID = player.id
            break
        end
    end

    for name, value in pairs(ACC_MAP) do
        local fun = value.fun
        local meW = value.me
        local enemyW = value.enemy
        local meV = meW * fun(state, meID)
        local enemyV = enemyW * fun(state, enemyID)
        result1 = result1 + meV + enemyV
        -- if meV + enemyV ~= 0 then
        result2[name] = meV + enemyV
        -- end
    end
    return result1, result2
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
    local height = nil
    local width = nil
    for _, player in ipairs(state.players) do
        if player.id == myID then
            me = player
            break
        end
    end
    height = #tiles
    for ii, row in ipairs(tiles) do
        width = #row
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
    for _, card in ipairs(state.myData.hand) do
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
            if (newI < 0 or newJ < 0 or newI >= height or newJ >= width) then
            else
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

DATA = ReadJ('../bots/data.json')
function PickAction(actions)
    local choices = {}
    local res = {}
    for _, action in pairs(actions) do
        local value = 0
        local a = action:ToShort()
        res[a] = action
        if DATA[a] ~= nil then
            for _, d in ipairs(DATA[a]) do
                value = value + d.total
            end
            value = math.floor(value / #(DATA[a]))
        end
        choices[a] = value
    end
    -- for key, value in pairs(choices) do
    --     print(key..' -> '..value..'  ')
    -- end
    local actionA = {}
    local valueA = {}
    for key, value in pairs(choices) do
        actionA[#actionA+1] = key
        valueA[#valueA+1] = value
    end
    -- for i, action in ipairs(actionA) do
    --     print(action..' '..valueA[i])
    -- end

    -- find the minimum
    local min = nil
    for _, value in ipairs(valueA) do
        if min == nil or value < min then
            min = value
        end
    end
    -- for i, value in ipairs(valueA) do
    --     print(i..' '..value)
    -- end
    -- print('MIN: '..min)
    -- subtract the minimum
    for i, _ in ipairs(valueA) do
        valueA[i] = valueA[i] - min
        if i > 1 then
            valueA[i] = valueA[i] + valueA[i-1]
        end
    end

    -- find max
    local max = nil
    for _, value in ipairs(valueA) do
        if max == nil or value > max then
            max = value
        end
    end

    -- for i, value in ipairs(valueA) do
    --     print(i..' '..value)
    -- end
    -- print('MAX: '..max)
    
    -- find the closest
    local r = math.random(max)
    -- print('GENERATED: '..r)
    -- print('\n\n')
    if max == 0 then
        return res[actionA[math.random(#actionA)]]
    end
    for i, value in ipairs(valueA) do
        if r <= value then
            return res[actionA[i]]
        end
    end
end

SAVED = {}
LAST_ACTION = nil
function bcore:_PromptActionComplex(statej)
    local state = ParseState(statej)

    local total, each = CalculateAdvantage(state)
    if LAST_ACTION ~= nil then
        if SAVED[LAST_ACTION] == nil then
            SAVED[LAST_ACTION] = {}
        end
        local res = {}
        for key, value in pairs(each) do
            res[key] = value - PREV_ADVANTAGE[key]
        end
        SAVED[LAST_ACTION][#SAVED[LAST_ACTION]+1] = {
            total = total - PREV_ADVANTAGE_TOTAL,
        }
    end
    PREV_ADVANTAGE = each
    PREV_ADVANTAGE_TOTAL = total
    
    local actions = GetAvailableActions(state)
    if #actions == 0 then
        return "pass"
    end
    local result = PickAction(actions)
    LAST_ACTION = result:ToShort()
    return result:ToStr()
end

function bcore:_PromptActionRandom(statej)
    local state = ParseState(statej)

    local total, each = CalculateAdvantage(state)
    if LAST_ACTION ~= nil then
        if SAVED[LAST_ACTION] == nil then
            SAVED[LAST_ACTION] = {}
        end
        local res = {}
        for key, value in pairs(each) do
            res[key] = value - PREV_ADVANTAGE[key]
        end
        SAVED[LAST_ACTION][#SAVED[LAST_ACTION]+1] = {
            total = total - PREV_ADVANTAGE_TOTAL,
            each = res
        }
    end
    PREV_ADVANTAGE = each
    PREV_ADVANTAGE_TOTAL = total
    
    local actions = GetAvailableActions(state)
    if #actions == 0 then
        return "pass"
    end
    -- local result = PickAction(actions)
    local result = actions[math.random(#actions)]
    LAST_ACTION = result:ToShort()
    return result:ToStr()
end


function bcore:_Setup(statej)
    local state = ParseState(statej)
    PREV_ADVANTAGE = CalculateAdvantage(state)
end


function bcore:_Update()

end


function bcore:_Cleanup()
    local path = '../bots/data.json'
    local data = ReadJ(path)
    for action, add in pairs(SAVED) do
        if data[action] == nil then
            data[action] = {}
        end
        for _, a in ipairs(add) do
            data[action][#data[action]+1] = a
        end
    end
    WriteJ(path, data)
end

return bcore