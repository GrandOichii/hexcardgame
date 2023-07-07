function _Create(props)
    local result = CardCreation:Unit(props)
    result:AddSubtype('Warrior')
    result.baseDefence = result.baseDefence + 1
    result:AddDefence(1)
    return result
end