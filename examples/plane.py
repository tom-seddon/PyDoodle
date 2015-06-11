from pydoodle import *
import math

class Plane:
    def __init__(self):
        self.theta=0
        self.pt=V2(0,0)

        add_rotate_handles(attrs(self,"theta"))
        add_translate_handles(attrs(self,"pt"))

    def tick(self):
        n=V2.from_angle(self.theta-math.pi/2)
        d=V2.dot(self.pt,n)

        set_colour(Colour(1,0,0))
        line(self.pt,self.pt+n*50)

        set_colour(Colour(0,0,0))
        line(self.pt-n.perp()*100,self.pt+n.perp()*100)

        set_colour(Colour(0,1,0))
        line(V2(0,0),n*d)

        num_tests=10
        for i in range(num_tests):
            t=i/(num_tests-1.0)

            a=((i-num_tests/2)*5)*V2(1,1)

            b=a-(V2.dot(a,n)-d)*n
            
            set_colour(Colour(t,t,0))
            line(a,b)

        self.theta_handle.tick(self.pt)
        self.pt_handle.tick()

        print "nx=%s ny=%s d=%s"%(n.x,n.y,d)

run(Plane())
