local pIDs = GetPlayerIDs()
-- player 1
-- set ownership
TileOwnerSet({
    {2, 0},
    {0, 1},
    {1, 1},
    {2, 1},
    {3, 1},
    {4, 1},
    {1, 2},
    {2, 2},
    {3, 2},
}, pIDs[0])
-- place entities
CreateAndPutEntity(pIDs[0], {2, 1}, "dev::Castle")
CreateAndPutEntity(pIDs[0], {0, 1}, "dev::Mana Drill")
CreateAndPutEntity(pIDs[0], {1, 1}, "dev::Mana Drill")
CreateAndPutEntity(pIDs[0], {1, 2}, "dev::Mana Drill")
-- player 2
-- set ownership
TileOwnerSet({
    {2, 0},
    {0, 1},
    {1, 1},
    {2, 1},
    {3, 1},
    {4, 1},
    {1, 2},
    {2, 2},
    {3, 2},
}, pIDs[0])
-- place entities
CreateAndPutEntity(pIDs[0], {2, 1}, "dev::Castle")
CreateAndPutEntity(pIDs[0], {0, 1}, "dev::Mana Drill")
CreateAndPutEntity(pIDs[0], {1, 1}, "dev::Mana Drill")
CreateAndPutEntity(pIDs[0], {1, 2}, "dev::Mana Drill")