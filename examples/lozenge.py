from pydoodle import *

class Lozenge:
    def __init__(self,
                 a,
                 b,
                 w):
        self.a=a
        self.b=b
        self.w=w

        add_translate_handles(attrs(self,"a","b"))
        tweaks(attrs(self,"w"))

    def tick(self,
             p):
        set_colour(Colour(0,0,0))

        ba=self.b-self.a
        wa=ba.perp().normalized()*self.w*0.5

        line(self.a-wa,self.b-wa)
        line(self.a+wa,self.b+wa)
        line(self.a,self.b)
        circle(self.a,self.w/2)
        circle(self.b,self.w/2)

        set_colour(Colour(0.5,0.5,0.5))

        line(self.a,p)
        ap=p-self.a

        ap_ba=V2.dot(ap,ba)
        ba_ba=V2.dot(ba,ba)

        if ba_ba>0:
            t=ap_ba/ba_ba
            
            p2=V2.lerp(self.a,self.b,ap_ba/ba_ba)
            circle(p2,10)

            if t>=0 and t<=1:
                pass
        
        self.a_handle.tick()
        self.b_handle.tick()

class Loz:
    def __init__(self):
        self.la=Lozenge(V2(0,0),V2(50,0),10.0)
        self.lb=Lozenge(V2(0,0),V2(0,50),10.0)
        self.p=V2(0,0)
        add_translate_handles(attrs(self,"p"))

    def tick(self):
        self.la.tick(self.p)
        self.lb.tick(self.p)
        
        self.p_handle.tick()
        
run(Loz())
