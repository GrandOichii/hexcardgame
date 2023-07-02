

function _Create(props)
    local result = CardCreation:Unit(props)
    result:AddSubtype('Rogue')
    return result
end