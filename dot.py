from pydoodle import *

class Dot:
    def __init__(self):
        self.a=V2(0,0)
        self.b=V2(325,-31)
        self.c=V2(238,114)

        print dir(self)

        a=attrs(self,"a","b","c")
        tweaks(a)
        add_translate_handles(a)

    def tick(self):
        set_draw_colour(Colour(1,0,0))
        line(self.a,self.b)

        set_draw_colour(Colour(0,1,0))
        line(self.a,self.c)

        ab=self.b-self.a
        ac=self.c-self.a

        abab=V2.dot(ab,ab)
        acac=V2.dot(ac,ac)
        
        abac=V2.dot(ab,ac)

        if abab!=0:
            set_draw_colour(Colour(0,0.5,0))
            pt=V2.lerp(self.a,self.b,abac/abab)
            circle(pt,5)
            line(pt,self.c)

        if acac!=0:
            set_draw_colour(Colour(0.5,0,0))
            pt=V2.lerp(self.a,self.c,abac/acac)
            circle(pt,5)
            line(pt,self.b)

        self.a_handle.tick()
        self.b_handle.tick()
        self.c_handle.tick()

        #handle(self,"a","b","c")

run(Dot())

