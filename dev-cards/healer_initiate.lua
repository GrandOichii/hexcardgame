
function _Create(props)
    local result = CardCreation:Unit(props)

    result:AddBaseKeyword('Virtuous', false)
    
    return result
end