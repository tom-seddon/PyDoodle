from pydoodle import *

class Rect:
    def __init__(self):
        self.a=V2(-5,-5)
        self.b=V2(5,5)
        self.theta=0

        add_translate_handles(attrs(self,"a","b"))
        add_rotate_handles(attrs(self,"theta"))

    def tick(self):
        set_colour(Colour(0.7,0.7,0.7))
        fbox(self.a,self.b)

        self.a_handle.tick()
        self.b_handle.tick()
        self.theta_handle.tick(self.a)

run(Rect())
