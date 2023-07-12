package.path = package.path..';../bots/?.lua'

local bcore = require 'bcore'


function _PromptAction(statej)
    return bcore:_PromptActionRandom(statej)
end


function _Setup(statej)
    bcore:_Setup(statej)
end


function _Update()
    bcore:_Update()
end


function _Cleanup()
    -- bcore:_Cleanup()
end