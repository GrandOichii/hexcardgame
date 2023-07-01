-- pipeline
PipelineLayer = {}
function PipelineLayer.New(delegate, id)
    local result = {delegate=delegate, id=id}
    return result
end

Pipeline = {}
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

-- card creation
CardCreation = {}

-- basic card object
function CardCreation:Card(props)
    local required = {'name', 'cost', 'type', 'power', 'life'}
    local result = {}

    for _, value in ipairs(required) do
        result[value] = props[value]
    end

    return result
end

-- unit
function CardCreation:Unit(props)
    local result = CardCreation:Card(props)

    result.maxMovement = 1
    result.movement = 0

    return result
end

-- spell
function CardCreation:Spell(props)
    local result = CardCreation:Card(props)

    result.EffectP = Pipeline:New()
    result.EffectP:AddLayer(
        function(player, caster)
            Log('Player ' .. PlayerShortStr(player.id) .. ' played spell ' .. result.name .. ' using caster ' .. caster.name)
            return nil, true
        end
    )

    return result
end