function _Create(props)
    local result = CardCreation:Unit(props)
    result:AddSubtype('Mage')
    return result
end