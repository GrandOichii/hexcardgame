import api from "./api"

const avatarMap: {[key: string] : string}  = {}

export const getAvatar = async (handle: string) =>  {
    if (!(handle in avatarMap)) {
        const req = await api.get(`/api/users/avatar/${handle}`)
        avatarMap[handle] = req.data
    }
    return avatarMap[handle]
}