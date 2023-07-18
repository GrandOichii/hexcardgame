function _Create(props)
    local result = CardCreation:Unit(props)
    result:AddSubtype('Mage')
    result:AddSubtype('Warrior')
    return result
end