from front.dev.window import ClientWindow

w = ClientWindow('localhost', 9090)
# w.config_connection()
w.set_screen_size(800, 600)
# w.go_fullscreen()
w.run()