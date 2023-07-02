function _Create(props)
    local result = CardCreation:Mage(props)

    result.ModifySpellP:AddLayer(function (spell)
        spell:IncreaseDamage(1)
        return nil, true
    end)
    result.DemodifySpellP:AddLayer(function (spell)
        spell:DecreaseDamage(1)
        return nil, true
    end)

    return result
end