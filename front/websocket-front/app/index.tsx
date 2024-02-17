import { SafeAreaView, Text, View } from 'react-native'
import { useEffect, useState } from 'react'
import { getStored } from '../storage'
import api from '../api'

const Index = () => {

    const [loggedIn, setLoggedIn] = useState(false)
    
    const checkLogin = async () => {        
        const token = await getStored('jwt_token')
        if (token) {
            api.defaults.headers.common = { 'Authorization': `Bearer ${token}` }
        }
        setLoggedIn(!!token)
        
    }

    useEffect(() => { checkLogin() }, [])

    return <SafeAreaView style={{flex: 1}}>
    <Text>Hello, world</Text>
    </SafeAreaView>
}

export default Index