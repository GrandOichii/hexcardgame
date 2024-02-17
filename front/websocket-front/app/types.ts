
type Chat = {
	id: string
    participants: string[]
	messages: Message[]
}

type Message = {
    uhandle: string,
    text: string
}
