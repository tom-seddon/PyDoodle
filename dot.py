from pydoodle import *

class Dot:
    def __init__(self):
        self.a=V2(0,0)
        self.b=V2(10,0)
        self.c=V2(0,10)

    def tick(self):
        set_draw_colour_rgb(1,0,0)
        line(self.a,self.b)

        set_draw_colour_rgb(0,1,0)
        line(self.a,self.c)

        #handle(self,"a","b","c")

run(Dot())

