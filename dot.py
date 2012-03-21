from pydoodle import *

class Dot:
    def __init__(self):
        self.a=V2(0,0)
        self.b=V2(10,0)
        self.c=V2(0,10)

        tweakn(Attr(self,"a",Handle=TranslateHandle()),
               Attr(self,"b",Handle=TranslateHandle()),
               Attr(self,"c",Handle=TranslateHandle()))

    def tick(self):
        set_draw_colour_rgb(1,0,0)
        line(self.a,self.b)

        set_draw_colour_rgb(0,1,0)
        line(self.a,self.c)

        self.a_handle.tick()
        self.b_handle.tick()
        self.c_handle.tick()

        #handle(self,"a","b","c")

run(Dot())

