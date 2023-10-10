-- TODO not tested
function _Create(props)
    local result = CardCreation:Unit(props)

    result:AddBaseKeyword('Vile', false)

    return result
end