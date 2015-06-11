import math
from pydoodle import *

def get_spline_value(p0,p1,p2,p3,t):
    if t<0:
        return p0+t*(p1-p0)
    elif t>1:
         return p3+(t-1)*(p3-p2)
    else:
        v=0

        v+=p0*(1-t)*(1-t)*(1-t)
        v+=p1*3*(1-t)*(1-t)*t
        v+=p2*3*(1-t)*t*t
        v+=p3*t*t*t

        return v

class Spline:
    def __init__(self):
        self.p0=V2(0,0)
        self.p1=V2(10,0)
        self.p2=V2(90,100)
        self.p3=V2(100,100)
        self.t=0.0

        add_translate_handles(attrs(self,"p0","p1","p2","p3"))
        tweaks(attrs(self,"t"))

    def get_spline_pt(self,
                      t):
        return V2(get_spline_value(self.p0.x,
                                   self.p1.x,
                                   self.p2.x,
                                   self.p3.x,
                                   t),
                  get_spline_value(self.p0.y,
                                   self.p1.y,
                                   self.p2.y,
                                   self.p3.y,
                                   t))

    def tick(self):
        n=50
        w=4
        
        pts=[]

        for i in range(n):
            t=i/(n-1.0)
            t*=w
            t-=w*.5

            pts.append((self.get_spline_pt(t),
                        t))

            if len(pts)>=2:
                set_colour(Colour(255,0,0) if i%2==0 else Colour(0,255,0))
                line(pts[-2][0],pts[-1][0])

        # step, in units of distance
        step=10.0

        # current `t' value for interpolation
        t=-1

        # initial guess of appropriate delta t
        initial_dt=1/50.0

        npts=0

        while t<=2:
            pt=self.get_spline_pt(t)

            niter=0
            error=float("inf")
            dt=initial_dt

            next_t=t

            # # walk forwards to find first point that's ahead of the
            # # desired distance.
            # while niter<10:
            #     next_t=t+dt
            #     next_pt=self.get_spline_pt(next_t)

            #     dist=(next_pt-pt).len()

            #     fraction=dist/step

            #     if npts==0:
            #         print niter,next_t,dist,fraction

            #     if fraction>=1:
            #         break

            #     # Assume that relationship between t and distance is
            #     # linear.
            #     dt*=fraction

            #     niter+=1

            # # if this is too far ahead, do a binary search.
            # if fraction>1:
            #     min_t=t
            #     max_t=next_t

            #     niter=0

            #     while niter<10:
            #         mid_t=(min_t+max_t)*0.5

            #         mid_pt=self.get_spline_pt(mid_t)

            #         dist=(mid_pt-pt).len()

            #         if npts==0:
            #             print niter,min_t,max_t,dist,step
                        
            #         if dist<step:
            #             max_t=mid_t
            #         elif dist>step:
            #             min_t=mid_t

            #         niter+=1

            #     next_pt=mid_pt
            #     next_t=mid_t

            while niter<10:# and error>step/100.0:
                next_t=t+dt
                next_pt=self.get_spline_pt(next_t)

                dist=(next_pt-pt).len()

                fraction=step/dist

                if npts==3:
                    print step,dist,dt,fraction

                if abs(fraction-1)<1/100:
                    # that will do
                    break

                # Assume that locally, the relationship between `t'
                # and distance is linear.
                dt*=fraction

                niter+=1

            #print "%d: %f %f"%(npts,fraction,(next_pt-pt).len())

            set_colour(Colour(0,0,255) if npts==0 else Colour(0,255,255))
            circle(pt,5)

            pt=next_pt
            t=next_t

            npts+=1

            # if npts<5:
            #     print initial_dt

        self.p0_handle.tick()
        self.p1_handle.tick()
        self.p2_handle.tick()
        self.p3_handle.tick()

run(Spline())

# float GetSplineValue(float p0,float p1,float p2,float p3,float t)
# {
# 	if(t<0.f)
# 	{
# 		return p0+t*(p1-p0);
# 	}
# 	else if(t>1.f)
# 	{
# 		return p3+(t-1.f)*(p3-p2);
# 	}
# 	else
# 	{
# 		const float omt=1.f-t;
		
# 		const float omt2=omt*omt;
# 		const float t2=t*t;
		
# 		const float p0s=omt2*omt;
# 		const float p1s=3.f*omt2*t;
# 		const float p2s=3.f*omt*t2;
# 		const float p3s=t2*t;
		
# 		return p0s*p0+p1s*p1+p2s*p2+p3s*p3;
# 	}
# }
