function _Create(props) 
    local result = CardCreation:Unit(props) 
    result:AddTimer(2, function ()
        result.power = result.power + 1
        result.life = result.life + 1
    end)
    return result
end