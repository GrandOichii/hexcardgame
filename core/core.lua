-- Trigger types
TRIGGERS = {
    -- CARD_DRAW = 'card_draw',
    -- LIFE_GAIN = 'life_gain',
    TURN_START = 'turn_start',
    -- TURN_END = 'turn_end',
    -- SPELL_CAST = 'spell_cast',
    UNIT_MOVE = 'unit_move',
    SPELL_CAST = 'spell_cast'
}


-- Zones
ZONES = {
    DISCARD = 'discard',
    PLACED = 'placed',
    HAND = 'hand',
    PLAYED = 'played',
}

-- Utility object
Utility = {}

-- Returns a string version of the table
function Utility:TableToStr(t)
    if type(t) == 'table' then
        local s = '{ '
        for k,v in pairs(t) do
            if type(k) ~= 'number' then k = '"'..k..'"' end
            s = s .. '['..k..'] = ' .. Utility:TableToStr(v) .. ','
        end
        return s .. '} '
    else
        return tostring(t)
    end
end


-- Returns the length of the table
function Utility:TableLength(T)
    local count = 0
    for _ in pairs(T) do count = count + 1 end
    return count
end


-- Effects and triggers

-- Main effect creation object
EffectCreation = {}

-- Returns a builder object that builds an activated effect
function EffectCreation:ActivatedEffectBuilder()
    local result = {
        args = {}
    }
    -- zone
    -- check
    -- effect
    -- cost

    function result:Build()
        local ae = {}
        -- checkers
        if self.args.zone == nil then
            error("Can't build trigger: zone is missing")
        end
        if self.args.effectF == nil then
            error("Can't build trigger: effectF is missing")
        end
        if self.args.checkF == nil then
            error("Can't build trigger: checkF is missing")
        end
        if self.args.costF == nil then
            error("Can't build trigger: costF is missing")
        end
        ae.zone = self.args.zone
        ae.effectF = self.args.effectF
        ae.checkF = self.args.checkF
        ae.costF = self.args.costF
        return ae
    end

    function result:Zone(zone)
        self.args.zone = zone
        return self
    end

    function result:Cost(costF)
        self.args.costF = costF
        return self
    end

    function result:Effect( effectF )
        self.args.effectF = effectF
        return self
    end

    function result:Check( checkF )
        self.args.checkF = checkF
        return self
    end

    return result
end


-- Returns a builder that builds a trigger
function EffectCreation:TriggerBuilder()
    local result = EffectCreation:ActivatedEffectBuilder()

    local oldBuildF = result.Build
    function result:Build()
        local t = oldBuildF(self)
        if self.args.on == nil then
            error("Can't build trigger: on is missing")
        end
        if self.args.isSilent == nil then
            error("Can't build trigger: isSilent is missing")
        end
        t.on = self.args.on
        t.isSilent = self.args.isSilent
        return t
    end

    function result:IsSilent( isSilent )
        self.args.isSilent = isSilent
        return self
    end

    function result:On( on )
        self.args.on = on
        return self
    end

    return result
end


-- Common triggers, effects, checkers, costs, e.t.c.
Common = {}


-- Returns a function that always returns true
function Common:NoCost()
    return function (...)
        return true
    end
end


-- Returns a function that returns true if the card's owner is the current player
function Common:IsOwnersTurn(card)
    return function (playerID, args)
        return args.playerID == GetOwnerID(card.id)
    end
end


-- Returns a function that returns true if the caster id is the same as the card's id
function Common:IsCaster(card)
    return function (playerID, args)
        return args.casterID == card.id
    end
end


-- Returns a function that checks whether the player has enough energy
function Common:HasEnoughEnergy( amount )
    return function ( playerID, ... )
        return GetShortInfo(playerID).energy >= amount
    end
end


-- Returns a function that forces player to pay energy, equal to the amount
function Common:PayEnergy( amount )
    return function ( playerID, ... )
        PayEnergy(playerID, amount)
        return true
    end
end


-- Pipeline layer
PipelineLayer = {}


-- Creates a new pipeline layer
function PipelineLayer.New(delegate, id)
    local result = {delegate=delegate, id=id}
    return result
end


-- Pipeline creation object
Pipeline = {}


-- Creates a new pipeline
function Pipeline.New()
    local result = {
        layers = {},
        collectFunc = nil,
        collectInit = 0,
        result = 0
    }

    function result:AddLayer( delegate, id )
        id = id or nil
        self.layers[#self.layers+1] = PipelineLayer.New(delegate, id)
    end

    function result:Exec( ... )
        self.result = self.collectInit
        for i, layer in ipairs(self.layers) do
            local returned, success = layer.delegate(...)
            if not success then
                -- TODO
                return self.result, false
            end
            if self.collectFunc ~= nil then
                self.collectFunc(returned)
            end
        end
        return self.result, true
    end

    function result:Clear()
        self.layers = {}
    end

    function result:RemoveWithID( id )
        for i, layer in ipairs(self.layers) do
            if layer.id == id then
                table.remove(self.layers, i)
                return
            end
        end
        Log('WARN: TRIED TO REMOVE LAYER WITH ID '..' FROM TABLE, BUT FAILED')
    end

    return result
end


-- Card creation
CardCreation = {}


-- Basic card object
function CardCreation:Card(props)
    local required = {'name', 'cost', 'type', 'power', 'life'}
    local result = {}

    for _, value in ipairs(required) do
        result[value] = props[value]
    end
    result.basePower = result.power
    result.baseLife = result.life

    result.triggers = {}

    -- pipelines

    -- CanPlay pipeline
    result.CanPlayP = Pipeline.New()
    result.CanPlayP:AddLayer(
        function (playerID)
            return nil, Common:HasEnoughEnergy(result.cost)(playerID)
        end
    )
    function result:CanPlay(playerID)
        local _, res = self.CanPlayP:Exec(playerID)
        return res
    end

    -- PayCosts pipeline
    result.PayCostsP = Pipeline.New()
    result.PayCostsP:AddLayer(
        function (playerID)
            return nil, Common:PayEnergy(result.cost)(playerID)
        end
    )
    function result:PayCosts(playerID)
        local _, res = self.PayCostsP:Exec(playerID)
        return res
    end

    function result:IsUnit()
        return string.find(result.type, 'Unit')
    end

    return result
end


-- Dictionary of unit subtypes and manipulation functions
UNIT_SUBTYPE_MANIPULATION = {
    Mage = function (card)
        card.ModifySpellP = Pipeline:New()
        card.ModifySpellP:AddLayer(
            function(spell)
                Log('Modifying spell ' .. CardShortStr(spell.id) .. ' by caster ' .. CardShortStr(card.id))
                return nil, true
            end
        )
        function card:ModifySpell(spell)
            self.ModifySpellP:Exec(spell)
        end
        
        card.DemodifySpellP = Pipeline:New()
        card.DemodifySpellP:AddLayer(
            function(spell)
                Log('Demodifying spell ' .. CardShortStr(spell.id) .. ' by caster ' .. CardShortStr(card.id))
                return nil, true
            end
        )
        function card:DemodifySpell(spell)
            self.DemodifySpellP:Exec(spell)
        end
    end,

    Rogue = function (card)
        card.maxMovement = card.maxMovement + 1
    end,

    Warrior = function (card)
        card.triggers[#card.triggers+1] = EffectCreation:TriggerBuilder()
            :Check(function (playerID, args) return args.mid == card.id end)
            :Cost(Common:NoCost())
            :IsSilent(false)
            :On(TRIGGERS.UNIT_MOVE)
            :Zone(ZONES.PLACED)
            :Effect(function (playerID, args)
                local tile = args.tile
                TileOwnerSet(playerID, {{tile.iPos, tile.jPos}})
                return nil, true
            end)
        :Build()
    end
}


-- Placeable card
function CardCreation:Placeable(props)
    local result = CardCreation:Card(props)

    result.OnEnterP = Pipeline.New()
    result.OnEnterP:AddLayer(function (playerID, tile)
        Log('Card ' .. CardShortStr(result.id) .. ' entered play')
        return nil, true
    end)
    function result:OnEnter(playerID, tile)
        result.OnEnterP:Exec(playerID, tile)
    end

    result.baseDefence = 0
    result.maxDefence = result.baseDefence
    result.defence = result.maxDefence

    -- Adds defence to entity. !Does not modify the base defence.
    function result:AddDefence(amount)
        result.maxDefence = result.maxDefence + amount
        result.defence = result.defence + amount
    end

    -- Removes defence from entity. !Does not modify the base defence.
    function result:RemoveDefence(amount)
        -- TODO? zero checking?
        result.maxDefence = result.maxDefence - amount
        result.defence = result.defence - amount
    end

    return result
end


-- Unit creation
function CardCreation:Unit(props)
    local result = CardCreation:Placeable(props)

    result.maxMovement = 1
    result.movement = 0

    function result:AddSubtype(type)
        UNIT_SUBTYPE_MANIPULATION[type](result)
    end

    return result
end


-- Spell creation
function CardCreation:Spell(props)
    local result = CardCreation:Card(props)

    -- TODO add to canplay pipeline fetching all Mages and checking if any of them are under the player's control

    result.DamageValues = {}

    function result:IncreaseDamage(amount)
        for key, value in pairs(result.DamageValues) do
            result.DamageValues[key] = value + amount
        end
    end

    function result:DecreaseDamage(amount)
        for key, value in pairs(result.DamageValues) do
            result.DamageValues[key] = value + amount
        end
    end
    
    result.EffectP = Pipeline:New()
    result.EffectP:AddLayer(
        function(playerID, caster)
            Log('Player ' .. PlayerShortStr(playerID) .. ' played spell ' .. result.name .. ' using caster ' .. caster.name)
            return nil, true
        end
    )
    result.EffectP:AddLayer(
        function(playerID, caster)
            caster:ModifySpell(result)
            return nil, true
        end
    )
    function result:Effect(playerID, caster)
        self.EffectP:Exec(playerID, caster)
        caster:DemodifySpell(result)
    end

    return result
end


-- Structure creation
function CardCreation:Structure(props)
    return CardCreation:Placeable(props)
end