from pydoodle import *
import math

class PtInRect:
    def __init__(self):
        self.hsize=V2(5,5)
        self.pt=V2(0,0)
        self.theta=0

        add_translate_handles(attrs(self,"hsize","pt"))
        add_rotate_handles(attrs(self,"theta"))

    def tick(self):
        scale=2
        xa=V2.from_angle(self.theta)*scale
        ya=xa.perp()

        a=-self.hsize.x*xa-self.hsize.y*ya
        b=self.hsize.x*xa-self.hsize.y*ya
        c=self.hsize.x*xa+self.hsize.y*ya
        d=-self.hsize.x*xa+self.hsize.y*ya

        ptx=V2.dot(self.pt,xa)/V2.dot(xa,xa)
        pty=V2.dot(self.pt,ya)/V2.dot(ya,ya)

        #inside=abs(ptx)<abs(hbx) and abs(pty)<abs(hby)
        inside=abs(ptx)<self.hsize.x and abs(pty)<self.hsize.y
        set_colour(Colour(1,0,0) if inside else Colour(0,0,0))

        line(a,b)
        line(b,c)
        line(c,d)
        line(d,a)

        print "ptx=%s pty=%s"%(ptx,pty)
        #print "hbx=%s hby=%s"%(hbx,hby)
        print "half size: %s"%self.hsize
        
        self.hsize_handle.tick()
        self.pt_handle.tick()
        self.theta_handle.tick(V2(0,0))

run(PtInRect())
