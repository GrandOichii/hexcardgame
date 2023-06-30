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