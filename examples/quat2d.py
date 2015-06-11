import math
from pydoodle import *

def q_dot(a,b):
    return a[0]*b[0]+a[1]*b[1]

def q_scaled(a,b):
    return (a[0]*b,a[1]*b)

def q_nrm(q):
    return q_scaled(q,1.0/math.sqrt(q_dot(q,q)))

def q_neg(q):
    return (-q[0],
            -q[1])

def lerp(a,b,t):
    return a+(b-a)*t
    
def q_lerp(qa,qb,t):
    return (lerp(qa[0],qb[0],t),
            lerp(qa[1],qb[1],t))

def q_xa(q):
    return V2(1-2*q[0]*q[0],
              2*q[1]*q[0])

def q_ya(q):
    return V2(-2*q[1]*q[0],
              1-2*q[0]*q[0])

def q_from_angle(theta):
    return (math.sin(theta*0.5),
            math.cos(theta*0.5))

class Quat2d:
    def __init__(self):
        self.theta0=0
        self.theta1=0

        add_rotate_handles(attrs(self,"theta0"))
        add_rotate_handles(attrs(self,"theta1"))

    def draw_q(self,
               v,
               q):
        set_colour(Colour(v,0,0))
        line(V2.V00,q_xa(q)*10)
        
        set_colour(Colour(0,v,0))
        line(V2.V00,q_ya(q)*10)

    def tick(self):
        q0=q_from_angle(self.theta0)
        q1=q_from_angle(self.theta1)
        
        qh=q_nrm(q_lerp(q0,q1,0.5))

        if q_dot(q0,q1)<0:
            qh=q_neg(qh)

        self.draw_q(1,q0)
        self.draw_q(.9,q1)
        self.draw_q(.5,qh)

        self.theta0_handle.tick(V2(20,0))
        self.theta1_handle.tick(V2(-20,0))

run(Quat2d())
