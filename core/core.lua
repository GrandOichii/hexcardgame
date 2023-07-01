-- Trigger types
TRIGGERS = {
    CARD_DRAW = 'card_draw',
    LIFE_GAIN = 'life_gain',
    TURN_START = 'turn_start',
    TURN_END = 'turn_end',
    SPELL_CAST = 'spell_cast',
}


-- Zones
ZONES = {
    DISCARD = 'discard',
    UNITS = 'units',
    TREASURES = 'treasures',
    BOND = 'bond'
}

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

    return result
end


-- Unit creation
function CardCreation:Unit(props)
    local result = CardCreation:Card(props)

    result.maxMovement = 1
    result.movement = 0

    return result
end


-- Spell creation
function CardCreation:Spell(props)
    local result = CardCreation:Card(props)

    result.EffectP = Pipeline:New()
    result.EffectP:AddLayer(
        function(playerID, caster)
            Log('Player ' .. PlayerShortStr(playerID) .. ' played spell ' .. result.name .. ' using caster ' .. caster.name)
            return nil, true
        end
    )

    return result
end


-- Structure creation
function CardCreation:Structure(props)
    return CardCreation:Card(props)
end