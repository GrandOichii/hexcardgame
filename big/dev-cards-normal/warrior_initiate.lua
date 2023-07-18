function _Create(props)
    local result = CardCreation:Unit(props)
    result:AddSubtype('Warrior')
    return result
end