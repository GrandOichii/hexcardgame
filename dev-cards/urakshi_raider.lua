
function _Create(props)
    local result = CardCreation:Unit(props)

    result:AddSubtype('Warrior')

    result:AddBaseKeyword('Fast', false)

    return result
end