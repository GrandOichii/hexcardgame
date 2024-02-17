import * as SecureStore from "expo-secure-store";
import { Platform } from "react-native";

const local: {[key: string] : string} = {}

// TODO bad, change

const isWeb = () => Platform.OS == 'web'

export const getStored = async (key: string) => {
    if (isWeb()) {
        return local[key]
    }
    return await SecureStore.getItemAsync(key)   
}

export const setStored = async (key: string, value: string) => {
    if (isWeb()) {
        local[key] = value
        return
    }
    return await SecureStore.setItemAsync(key, value)
}

export const unsetStored = async (key: string) => {
    if (isWeb()) {
        delete local[key]
        return
    }
    return await SecureStore.deleteItemAsync(key)
}